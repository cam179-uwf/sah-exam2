using CarController.Services;
using CarController.Services.PS4;
using SharpDX.DirectInput;

CarService carService;

Console.WriteLine("Connecting...");

try
{
    carService = new CarService("BoxCarESP32");
    Console.WriteLine("Connected to BoxCarESP32.");
}
catch (Exception e)
{
    Console.WriteLine(e);
    return;
}

PS4Joystick joystick;

try
{
    joystick = new PS4Joystick();
    Console.WriteLine("Joystick found and connected.");
}
catch (Exception e)
{
    Console.WriteLine(e);
    return;
}

var isInAutonomousMode = false;
var isInAutonomousModeInitialization = false;
var allowXButtonPress = true;

byte previousSpeed = 0;

var isLeftIrSensorDetected = false;
var isRightIrSensorDetected = false;

carService.OnLeftIrSensorDetected += () =>
{
    Console.WriteLine("left ir sensor detected.");
    isLeftIrSensorDetected = true;
};
carService.OnRightIrSensorDetected += () =>
{
    Console.WriteLine("right ir sensor detected.");
    isRightIrSensorDetected = true;
};

while (true)
{
    var isXButtonPressed = joystick.IsButtonPressed(PS4Buttons.X);

    if (!isXButtonPressed) allowXButtonPress = true;
    
    if (isInAutonomousMode)
    {
        if (isXButtonPressed && allowXButtonPress)
        {
            Console.WriteLine("Switched to manual mode.");
            isInAutonomousMode = false;
            allowXButtonPress = false;
            continue;
        }
        
        if (isInAutonomousModeInitialization)
        {
            await carService.ChangeSpeed(255);
            await carService.MoveForwards();
            isInAutonomousModeInitialization = false;
        }

        if (isLeftIrSensorDetected)
        {
            await carService.TurnRight();

            await Task.Delay(2000);
            
            await carService.MoveForwards();
            
            isLeftIrSensorDetected = false;
            isRightIrSensorDetected = false;
        }
        
        if (isRightIrSensorDetected)
        {
            await carService.TurnLeft();

            await Task.Delay(2000);
            
            await carService.MoveForwards();
            
            isLeftIrSensorDetected = false;
            isRightIrSensorDetected = false;
        }
            
        continue;
    }
    
    if (isXButtonPressed && allowXButtonPress)
    {
        Console.WriteLine("Switched to autonomous mode.");
        isInAutonomousMode = true;
        isInAutonomousModeInitialization = true;
        allowXButtonPress = false;
        continue;
    }
    
    if (joystick.IsButtonPressed(PS4Buttons.R2))
    {
        var speed = (byte)((byte)((255 - 150) * joystick.RightPaddle) + 150);
        if (speed < previousSpeed - 1 || speed > previousSpeed + 1)
        {
            Console.WriteLine($"Speed: {speed}");
            await carService.ChangeSpeed(speed);
        }
        
        if (joystick.IsButtonPressed(PS4Buttons.LeftArrow))
        {
            await carService.TurnLeft();    
        }
        else if (joystick.IsButtonPressed(PS4Buttons.RightArrow))
        {
            await carService.TurnRight();
        }
        else if (joystick.IsButtonPressed(PS4Buttons.UpArrow))
        {
            await carService.MoveForwards();
        } 
        else if (joystick.IsButtonPressed(PS4Buttons.DownArrow))
        {
            await carService.MoveBackwards();
        }
        else if (!carService.IsStoppedMoving)
        {
            await carService.StopMoving();
        }
        
        previousSpeed = speed;
    }
    else if (!carService.IsStoppedMoving)
    {
        await carService.StopMoving();
    }
    else
    {
        await Task.Delay(10);
    }
}