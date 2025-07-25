using CarController.Services;
using CarController.Services.PS4;
using CarController.States;

namespace CarController.Runnables.States;

public class DrivingState : State
{
    private readonly PS4Joystick _joystick;
    private readonly CarService _carService;
    private byte _previousSpeed;

    public DrivingState(IMutableState mutableState, PS4Joystick joystick, CarService carService) : base(mutableState)
    {
        _joystick = joystick;
        _carService = carService;
    }

    public override Task OnEnter()
    {
        Console.WriteLine("Entered manual drive.");
        return Task.CompletedTask;
    }

    public override async Task OnUpdate()
    {
        _joystick.Update();
        
        if (_joystick.OnButtonDown(PS4Buttons.X))
        {
            await ChangeState(new AutoDrivingState(MutableState, _joystick, _carService));
            return;
        }

        if (!_joystick.IsButtonPressed(PS4Buttons.R2))
        {
            await ChangeState(new IdleState(MutableState, _joystick, _carService));
            return;
        }
        
        var speed = (byte)((byte)((255 - 150) * _joystick.RightPaddle) + 150);
        if (speed < _previousSpeed - 1 || speed > _previousSpeed + 1)
        {
            // Console.WriteLine($"Speed: {speed}");
            await _carService.ChangeSpeed(speed);
        }
    
        // change our direction depending on our input
        if (_joystick.IsButtonPressed(PS4Buttons.LeftArrow))
        {
            await _carService.TurnLeft();    
        }
        else if (_joystick.IsButtonPressed(PS4Buttons.RightArrow))
        {
            await _carService.TurnRight();
        }
        else if (_joystick.IsButtonPressed(PS4Buttons.UpArrow))
        {
            await _carService.MoveForwards();
        } 
        else if (_joystick.IsButtonPressed(PS4Buttons.DownArrow))
        {
            await _carService.MoveBackwards();
        }
        // if we aren't pressing one of the directional arrows on
        // the controller than just stop the car
        else if (!_carService.IsStoppedMoving)
        {
            await _carService.StopMoving();
        }
    
        _previousSpeed = speed;
    }

    public override Task OnExit()
    {
        return Task.CompletedTask;
    }
}