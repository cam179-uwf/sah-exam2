using CarController.Services;
using CarController.States;

namespace PhoneApp.States;

public class IdleState : State
{
    private readonly CarService _carService;
    
    public IdleState(IMutableState mutableState, CarService carService) : base(mutableState)
    {
        _carService = carService;
    }
    
    // I really don't want to write out a comment for each
    // of the following methods because they all speak for
    // themselves, and I am just going to repeat what the method
    // names already say
    // so scroll down to the logic
    //
    // basically, these methods handle our UI button events

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
            await ChangeState(new DrivingState(MutableState, _carService));
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
            await ChangeState(new DrivingState(MutableState, _carService));
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
            await ChangeState(new DrivingState(MutableState, _carService));
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
            await ChangeState(new DrivingState(MutableState, _carService));
            await _carService.MoveBackwards();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
        }
    }
    
    public override Task OnEnter()
    {
        Console.WriteLine("Idling...");
        
        MainPage.GlobalAutoModeButtonClicked += OnGlobalAutoModeButtonClicked;
        MainPage.GlobalRightButtonClicked += OnGlobalRightButtonClicked;
        MainPage.GlobalLeftButtonClicked += OnGlobalLeftButtonClicked;
        MainPage.GlobalUpButtonClicked += OnGlobalUpButtonClicked;
        MainPage.GlobalDownButtonClicked += OnGlobalDownButtonClicked;
        
        return Task.CompletedTask;
    }

    public override async Task OnUpdate()
    {
        // if the car hasn't stopped then stop the car
        if (!_carService.IsStoppedMoving)
        {
            await _carService.StopMoving();
        }
    }

    public override Task OnExit()
    {
        MainPage.GlobalAutoModeButtonClicked -= OnGlobalAutoModeButtonClicked;
        MainPage.GlobalRightButtonClicked -= OnGlobalRightButtonClicked;
        MainPage.GlobalLeftButtonClicked -= OnGlobalLeftButtonClicked;
        MainPage.GlobalUpButtonClicked -= OnGlobalUpButtonClicked;
        MainPage.GlobalDownButtonClicked -= OnGlobalDownButtonClicked;
        
        return Task.CompletedTask;
    }
}