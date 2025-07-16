using CarController.Services;
using CarController.States;

namespace PhoneApp.States;

public class AutoDrivingState : State
{
    private readonly CarService _carService;
    private bool _isLeftIrSensorDetected;
    private bool _isRightIrSensorDetected;
    
    public AutoDrivingState(IMutableState mutableState, CarService carService) : base(mutableState)
    {
        _carService = carService;
    }
    
    /// <summary>
    /// Handle when auto mode button is clicked event.
    /// </summary>
    private async void OnGlobalAutoModeButtonClicked(bool isAutoMode)
    {
        try
        {
            if (!isAutoMode)
            {
                await ChangeState(new IdleState(MutableState, _carService));
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
        }
    }
    
    /// <summary>
    /// Handle when left IR sensor is triggered event.
    /// </summary>
    private void OnLeftIrSensorDetected()
    {
        Console.WriteLine("Left IR sensor detected.");
        _isLeftIrSensorDetected = true;
    }

    /// <summary>
    /// Handle when right IR sensor is triggered event.
    /// </summary>
    private void OnRightIrSensorDetected()
    {
        Console.WriteLine("Right IR sensor detected.");
        _isRightIrSensorDetected = true;
    }
    
    public override async Task OnEnter()
    {
        Console.WriteLine("Car is autonomously driving.");
        
        _carService.OnLeftIrSensorDetected += OnLeftIrSensorDetected;
        _carService.OnRightIrSensorDetected += OnRightIrSensorDetected;
        MainPage.GlobalAutoModeButtonClicked += OnGlobalAutoModeButtonClicked;
        
        // set the cars speed to max
        await _carService.ChangeSpeed(255);
        
        // move the car forward
        await _carService.MoveForwards();
    }

    public override async Task OnUpdate()
    {
        // if the left IR sensor was detected do this
        if (_isLeftIrSensorDetected)
        {
            // turn the car right for 2 seconds
            await _carService.TurnRight();

            await Task.Delay(2000);
            
            // start moving forward again
            await _carService.MoveForwards();
            
            // clear any sensor detected events
            _isLeftIrSensorDetected = false;
            _isRightIrSensorDetected = false;
        }
        
        // if the right IR sensor was detected do this
        if (_isRightIrSensorDetected)
        {
            // turn the car left for 2 seconds
            await _carService.TurnLeft();

            await Task.Delay(2000);
            
            // start moving forward again
            await _carService.MoveForwards();
            
            // clear any sensor detected events
            _isLeftIrSensorDetected = false;
            _isRightIrSensorDetected = false;
        }
    }

    public override Task OnExit()
    {
        _carService.OnLeftIrSensorDetected -= OnLeftIrSensorDetected;
        _carService.OnRightIrSensorDetected -= OnRightIrSensorDetected;
        MainPage.GlobalAutoModeButtonClicked -= OnGlobalAutoModeButtonClicked;
        
        return Task.CompletedTask;
    }
}