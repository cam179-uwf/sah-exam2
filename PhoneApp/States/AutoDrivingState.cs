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
        Console.WriteLine("Car is autonomously driving.");
        
        _carService.OnLeftIrSensorDetected += OnLeftIrSensorDetected;
        _carService.OnRightIrSensorDetected += OnRightIrSensorDetected;
        MainPage.GlobalAutoModeButtonClicked += OnGlobalAutoModeButtonClicked;
        
        await _carService.ChangeSpeed(255);
        await _carService.MoveForwards();
    }

    public override async Task OnUpdate()
    {
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
        MainPage.GlobalAutoModeButtonClicked -= OnGlobalAutoModeButtonClicked;
        
        return Task.CompletedTask;
    }
}