using CommunityToolkit.Mvvm.ComponentModel;

namespace ClassIslandHide;

public partial class Settings : ObservableObject
{
    [ObservableProperty] private string _lastGhostExePath = "";

    [ObservableProperty] private string _rawExePath = "";

    private bool _enableAntiDebug = true;
    public bool EnableAntiDebug
    {
        get => _enableAntiDebug;
        set => SetProperty(ref _enableAntiDebug, value);
    }
    
    private string _fakeProcessDescription = "System Background Process";
    public string FakeProcessDescription
    {
        get => _fakeProcessDescription;
        set => SetProperty(ref _fakeProcessDescription, value);
    }
    
    private bool _randomizeWindowClass = true;
    public bool RandomizeWindowClass
    {
        get => _randomizeWindowClass;
        set => SetProperty(ref _randomizeWindowClass, value);
    }
}