using CarController.Services;
using CarController.Services.PS4;
using CarController.States;

namespace CarController.Runnables.States;

public class AutonomousDriveState : State
{
    private readonly PS4Joystick _Joystick;
    private readonly CarService _carService;
    private bool _isLeftIrSensorDetected;
    private bool _isRightIrSensorDetected;
    
    public AutonomousDriveState(IMutableState mutableState, PS4Joystick joystick, CarService carService) : base(mutableState)
    {
        _Joystick = joystick;
        _carService = carService;
    }
    
    private void OnLeftIrSensorDetected()
    {
        Console.WriteLine("Left IR sensor detected.");
        _isLeftIrSensorDetected = true;
    }

    private void OnRightIrSensorDetected()
    {
        Console.WriteLine("Right IR sensor detected.");
        _isRightIrSensorDetected = true;
    }

    public override async Task OnEnter()
    {
        Console.WriteLine("Entered autonomous drive.");

        _carService.OnLeftIrSensorDetected += OnLeftIrSensorDetected;
        _carService.OnRightIrSensorDetected += OnRightIrSensorDetected;
        
        await _carService.ChangeSpeed(255);
        await _carService.MoveForwards();
    }

    public override async Task OnUpdate()
    {
        _Joystick.Update();
        
        if (_Joystick.OnButtonDown(PS4Buttons.X))
        {
            ChangeState(new IdleState(_mutableState, _Joystick, _carService));
            return;
        }

        if (_isLeftIrSensorDetected)
        {
            await _carService.TurnRight();

            await Task.Delay(2000);
            
            await _carService.MoveForwards();
            
            _isLeftIrSensorDetected = false;
            _isRightIrSensorDetected = false;
        }
        
        if (_isRightIrSensorDetected)
        {
            await _carService.TurnLeft();

            await Task.Delay(2000);
            
            await _carService.MoveForwards();
            
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