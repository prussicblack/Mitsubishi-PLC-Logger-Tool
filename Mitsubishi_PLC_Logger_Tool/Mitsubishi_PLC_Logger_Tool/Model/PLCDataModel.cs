using System;
using System.Collections.Generic;
using System.Text;

namespace Mitsubishi_PLC_Logger_Tool.Model
{

    using System;
    using System.Buffers.Binary;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.Json.Serialization;
    using System.Threading;

    public class PlcLoggerConfig
    {
        public PlcConnection Connection { get; set; } = new();
        public List<PlcGroup> Groups { get; set; } = new();
    }

    public class PlcConnection
    {
        public string PlcType { get; set; } = "MitsubishiQ";
        public string Protocol { get; set; } = "UDP";
        public string IpAddress { get; set; } = "192.168.1.10";

        public int Port { get; set; } = 5000;
        public int Timeout { get; set; } = 3000;
        public int PollingIntervalMs { get; set; } = 100;
    }

    public class PlcGroup
    {
        public string ID { get; set; } = string.Empty;
        public string DeviceCode { get; set; } = "D";
        public string AddressNumberType { get; set; } = "Decimal";
        public string Type { get; set; } = "Word";
        public string StartAddress { get; set; } = string.Empty;

        public int ReadSize { get; set; }

        public List<PlcLink> Links { get; set; } = new();

        public void Initialize()
        {
            foreach (PlcLink link in Links)
            {
                link.Initialize();
            }
        }

    }

    public class PlcLink
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int Size { get; set; } = 1;

        public int? BitIndex { get; set; }
        public int? BitSize { get; set; }

        public string ValueType { get; set; } = "INT";


        private byte[] _rawValue = Array.Empty<byte>();
        [JsonIgnore]
        public byte[] RawValue => Volatile.Read(ref _rawValue);

        //RawValue는 레이스 상태일 수 있으므로, Volatile.Read를 사용하여 안전하게 읽도록 처리.
        //public byte[] RawValue { get; set; } = Array.Empty<byte>();

        public void Initialize()
        {
            int byteSize = Math.Max(1, Size) * 2;

            Interlocked.Exchange(ref _rawValue, new byte[byteSize]);
        }


        public void SetRawValue(byte[] value)
        {
            Interlocked.Exchange(ref _rawValue, value);
        }


        public object? GetValue()
        {
            // 한 번만 읽어서 로컬에 보관
            byte[] data = Volatile.Read(ref _rawValue);

            if (data.Length == 0)
                return null;

            // 비트 필드 추출 (최대 64비트)
            if (BitIndex.HasValue && BitSize.HasValue)
            {
                int bitIndex = BitIndex.Value;
                int bitSize = BitSize.Value;

                if (bitIndex < 0 || bitSize < 1 || bitSize > 64)
                    return null;

                if ((long)bitIndex + bitSize > data.Length * 8L)
                    return null;

                if ((long)bitIndex + bitSize > 64)
                    return null;

                if (ValueType == "ASCII" || ValueType == "FLOAT")
                    return null;

                ulong raw = 0;

                int byteCount = Math.Min(data.Length, 8);

                for (int i = 0; i < byteCount; i++)
                {
                    raw |= (ulong)data[i] << (i * 8);
                }

                ulong mask = ulong.MaxValue;

                if (bitSize < 64)
                    mask = (1UL << bitSize) - 1;

                ulong value = (raw >> bitIndex) & mask;

                if (ValueType == "BOOL")
                    return value != 0;

                return value;
            }

            // 일반 데이터 해석
            switch (ValueType.ToUpperInvariant())
            {
                case "BOOL":
                    return data[0] != 0;

                case "INT":
                    if (data.Length < 2)
                        return null;

                    return BinaryPrimitives.ReadInt16LittleEndian(data);

                case "UINT":
                    if (data.Length < 2)
                        return null;

                    return BinaryPrimitives.ReadUInt16LittleEndian(data);

                case "DINT":
                    if (data.Length < 4)
                        return null;

                    return BinaryPrimitives.ReadInt32LittleEndian(data);

                case "UDINT":
                    if (data.Length < 4)
                        return null;

                    return BinaryPrimitives.ReadUInt32LittleEndian(data);

                case "FLOAT":
                    if (data.Length < 4)
                        return null;

                    int bits = BinaryPrimitives.ReadInt32LittleEndian(data);
                    return BitConverter.Int32BitsToSingle(bits);

                case "ASCII":
                    return Encoding.ASCII.GetString(data).TrimEnd('\0');

                default:
                    return null;
            }
        }
    }

}
