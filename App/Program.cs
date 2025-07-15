using CarController.Runnables.States;
using CarController.Services;
using CarController.Services.PS4;
using CarController.States;

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

var stateMachine = new StateMachine();
await stateMachine.Start(new IdleState(stateMachine, joystick, carService));