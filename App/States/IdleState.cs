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
        
        // if the x button is pressed down then switch to the auto driving state
        if (_joystick.OnButtonDown(PS4Buttons.X))
        {
            await ChangeState(new AutoDrivingState(MutableState, _joystick, _carService));
            return;
        }
        
        // if the R2 button is pressed then switch to the driving state
        if (_joystick.IsButtonPressed(PS4Buttons.R2))
        {
            await ChangeState(new DrivingState(MutableState, _joystick, _carService));
        }
        // if the car hasn't stopped then stop the car
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