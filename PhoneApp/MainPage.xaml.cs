using CarController.Services;
using CarController.States;
using PhoneApp.States;

namespace PhoneApp;

public partial class MainPage
{
    private readonly CarService _carService;
    private readonly StateMachine _stateMachine = new();
    private bool _isConnecting;
    private bool _isAutoMode;

    // events for UI events
    public static event Action? GlobalStopButtonClicked;
    public static event Action<bool>? GlobalAutoModeButtonClicked; 
    public static event Action? GlobalLeftButtonClicked; 
    public static event Action? GlobalRightButtonClicked;
    public static event Action? GlobalUpButtonClicked; 
    public static event Action? GlobalDownButtonClicked;
    public static event Action<byte>? GlobalChangeSpeed;
    
    public MainPage()
    {
#if ANDROID
        _carService = new CarService(new AndroidClassicBluetoothClient("BoxCarESP32"));
#else
        _carService = new CarService(new InTheHandBluetoothClient("BoxCarESP32"));
#endif
        
        InitializeComponent();

        ConnectButton.Clicked += ConnectClicked;
        AutoModeButton.Clicked += AutoModeButtonClicked;
        
        LeftButton.Clicked += (_, _) => GlobalLeftButtonClicked?.Invoke();
        RightButton.Clicked += (_, _) => GlobalRightButtonClicked?.Invoke();
        UpButton.Clicked += (_, _) => GlobalUpButtonClicked?.Invoke();
        DownButton.Clicked += (_, _) => GlobalDownButtonClicked?.Invoke();
        StopButton.Clicked += (_, _) => GlobalStopButtonClicked?.Invoke();
    }

    /// <summary>
    /// Whenever the speed slider is changed raise an event.
    /// </summary>
    private void SpeedSliderChanged(object? sender, ValueChangedEventArgs e)
    {
        if (_carService.IsConnected)
        {
            GlobalChangeSpeed?.Invoke((byte)e.NewValue);
        }
    }

    /// <summary>
    /// Whenever the connect button is clicked try to connect.
    /// </summary>
    private void ConnectClicked(object? sender, EventArgs e)
    {
        if (!_isConnecting)
        { 
            Task.Run(ConnectOrDisconnect);
        }
    }

    /// <summary>
    /// Switch between being in auto mode and not being in auto mode.
    /// </summary>
    private void AutoModeButtonClicked(object? sender, EventArgs e)
    {
        if (!_carService.IsConnected) return;
        
        _isAutoMode = !_isAutoMode;
        
        GlobalAutoModeButtonClicked?.Invoke(_isAutoMode);

        Dispatcher.Dispatch(() =>
        {
            AutoModeButton.Background = _isAutoMode ? 
                new SolidColorBrush(Colors.Green) : 
                new SolidColorBrush(Colors.Gray);
        });
    }

    private async Task ConnectOrDisconnect()
    {
        try
        {
            _isConnecting = true;

            // we need to request permission to use bluetooth on Android
            await RequestBluetoothPermissionsAsync();

            // if we are already connected then just disconnect
            // and stop our state machine
            if (_carService.IsConnected)
            {
                await _stateMachine.Stop();
                _carService.Disconnect();

                Dispatcher.Dispatch(() =>
                {
                    ConnectButton.Text = "Connect";
                    ConnectButton.Background = new SolidColorBrush(Colors.Gray);
                });

                return;
            }

            Dispatcher.Dispatch(() =>
            {
                ConnectButton.Text = "Connecting...";
                ConnectButton.Background = new SolidColorBrush(Colors.LightYellow);
            });
            
            // connect to the car and start our state machine
            await _carService.Connect();
            await _stateMachine.Start(new IdleState(_stateMachine, _carService));

            Dispatcher.Dispatch(() =>
            {
                ConnectButton.Text = "Disconnect";
                ConnectButton.Background = new SolidColorBrush(Colors.Green);
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);

            Dispatcher.Dispatch(() =>
            {
                ConnectButton.Text = "Connect";
                ConnectButton.Background = new SolidColorBrush(Colors.Gray);
            });
        }
        finally
        {
            _isConnecting = false;
        }
    }
    
    /// <summary>
    /// Just requests bluetooth permissions on Android.
    /// </summary>
    private async Task RequestBluetoothPermissionsAsync()
    {
#if ANDROID
        var statusConnect = await Permissions.RequestAsync<BluetoothConnectPermission>();
        var statusScan = await Permissions.RequestAsync<BluetoothScanPermission>();

        if (statusConnect != PermissionStatus.Granted || statusScan != PermissionStatus.Granted)
        {
            // Handle denied permissions
            await Console.Error.WriteLineAsync("Bluetooth permissions not granted.");
        }
#endif
    }
}