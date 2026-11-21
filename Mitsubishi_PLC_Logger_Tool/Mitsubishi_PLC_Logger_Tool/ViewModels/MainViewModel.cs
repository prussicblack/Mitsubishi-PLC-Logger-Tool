using CommunityToolkit.Mvvm.ComponentModel;

namespace Mitsubishi_PLC_Logger_Tool.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string Greeting { get; set; } = "Welcome to Avalonia!";
}
