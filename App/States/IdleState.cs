using CarController.Services;
using CarController.Services.PS4;
using CarController.States;

namespace CarController.Runnables.States;

public class IdleState : State
{
    private readonly PS4Joystick _joystick;
    private readonly CarService _carService;
    
    public IdleState(IMutableState mutableState, PS4Joystick joystick, CarService carService) : base(mutableState)
    {
        _joystick = joystick;
        _carService = carService;
    }

    public override Task OnEnter()
    {
        Console.WriteLine("Idling...");
        return Task.CompletedTask;
    }

    public override async Task OnUpdate()
    {
        _joystick.Update();
        
        if (_joystick.OnButtonDown(PS4Buttons.X))
        {
            ChangeState(new AutonomousDriveState(_mutableState, _joystick, _carService));
            return;
        }
        
        if (_joystick.IsButtonPressed(PS4Buttons.R2))
        {
            ChangeState(new DriveState(_mutableState, _joystick, _carService));
        }
        else if (!_carService.IsStoppedMoving)
        {
            await _carService.StopMoving();
        }
    }

    public override Task OnExit()
    {
        return Task.CompletedTask;
    }
}