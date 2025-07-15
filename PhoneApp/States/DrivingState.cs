using CarController.Services;
using CarController.States;

namespace PhoneApp.States;

public class DrivingState : State
{
    private readonly CarService _carService;
    
    public DrivingState(IMutableState mutableState, CarService carService) : base(mutableState)
    {
        _carService = carService;
    }
    
    private async void OnGlobalAutoModeButtonClicked(bool isAutoMode)
    {
        try
        {
            if (isAutoMode)
            {
                await ChangeState(new AutoDrivingState(MutableState, _carService));
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
        }
    }
    
    private async void OnGlobalRightButtonClicked()
    {
        try
        {
            await _carService.TurnRight();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
        }
    }
    
    private async void OnGlobalLeftButtonClicked()
    {
        try
        {
            await _carService.TurnLeft();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
        }
    }
    
    private async void OnGlobalUpButtonClicked()
    {
        try
        {
            await _carService.MoveForwards();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
        }
    }
    
    private async void OnGlobalDownButtonClicked()
    {
        try
        {
            await _carService.MoveBackwards();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
        }
    }
    
    private async void OnGlobalStopButtonClicked()
    {
        try
        {
            await ChangeState(new IdleState(MutableState, _carService));
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
        }
    }

    private async void OnGlobalChangeSpeed(byte newSpeed)
    {
        try
        {
            await _carService.ChangeSpeed(newSpeed);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
        }
    }

    public override Task OnEnter()
    {
        Console.WriteLine("Car is driving.");
        
        MainPage.GlobalAutoModeButtonClicked += OnGlobalAutoModeButtonClicked;
        MainPage.GlobalRightButtonClicked += OnGlobalRightButtonClicked;
        MainPage.GlobalLeftButtonClicked += OnGlobalLeftButtonClicked;
        MainPage.GlobalUpButtonClicked += OnGlobalUpButtonClicked;
        MainPage.GlobalDownButtonClicked += OnGlobalDownButtonClicked;
        MainPage.GlobalStopButtonClicked += OnGlobalStopButtonClicked;
        MainPage.GlobalChangeSpeed += OnGlobalChangeSpeed;
        
        return Task.CompletedTask;
    }

    public override Task OnUpdate()
    {
        return Task.CompletedTask;
    }

    public override Task OnExit()
    {
        MainPage.GlobalAutoModeButtonClicked -= OnGlobalAutoModeButtonClicked;
        MainPage.GlobalRightButtonClicked -= OnGlobalRightButtonClicked;
        MainPage.GlobalLeftButtonClicked -= OnGlobalLeftButtonClicked;
        MainPage.GlobalUpButtonClicked -= OnGlobalUpButtonClicked;
        MainPage.GlobalDownButtonClicked -= OnGlobalDownButtonClicked;
        MainPage.GlobalStopButtonClicked -= OnGlobalStopButtonClicked;
        MainPage.GlobalChangeSpeed -= OnGlobalChangeSpeed;
        
        return Task.CompletedTask;
    }
}