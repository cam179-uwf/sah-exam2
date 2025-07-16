using CarController.Services;
using CarController.Services.PS4;
using CarController.States;

namespace CarController.Runnables.States;

public class AutoDrivingState : State
{
    private readonly PS4Joystick _joystick;
    private readonly CarService _carService;
    private bool _isLeftIrSensorDetected;
    private bool _isRightIrSensorDetected;
    
    public AutoDrivingState(IMutableState mutableState, PS4Joystick joystick, CarService carService) : base(mutableState)
    {
        _joystick = joystick;
        _carService = carService;
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
        Console.WriteLine("Entered autonomous drive.");

        // set the cars speed to max
        _carService.OnLeftIrSensorDetected += OnLeftIrSensorDetected;
        _carService.OnRightIrSensorDetected += OnRightIrSensorDetected;
        
        // move the car forward
        await _carService.ChangeSpeed(255);
        await _carService.MoveForwards();
    }

    public override async Task OnUpdate()
    {
        _joystick.Update();
        
        // if x is pressed go back to idle state
        if (_joystick.OnButtonDown(PS4Buttons.X))
        {
            await ChangeState(new IdleState(MutableState, _joystick, _carService));
            return;
        }

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
        
        return Task.CompletedTask;
    }
}