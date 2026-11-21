using Android.App;
using Android.Content.PM;
using Avalonia;
using Avalonia.Android;

namespace Mitsubishi_PLC_Logger_Tool.Android;

[Activity(
    Label = "Mitsubishi_PLC_Logger_Tool.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity
{
}
