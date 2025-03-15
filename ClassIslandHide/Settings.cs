using CommunityToolkit.Mvvm.ComponentModel;

namespace ClassIslandHide;

public partial class Settings : ObservableObject
{
    [ObservableProperty] private string _lastGhostExePath = "";
}