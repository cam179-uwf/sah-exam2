using CarController.Runnables.States;
using CarController.Services;
using CarController.Services.PS4;
using CarController.States;

CarService carService;

Console.WriteLine("Connecting...");

// try connecting to the car
try
{
    carService = new CarService(new InTheHandBluetoothClient("BoxCarESP32"));
    await carService.Connect();
    Console.WriteLine("Connected to BoxCarESP32.");
}
catch (Exception e)
{
    Console.WriteLine(e);
    return;
}

PS4Joystick joystick;

// try connecting to a joystick
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

// run the state machine
var stateMachine = new StateMachine();
await stateMachine.Start(new IdleState(stateMachine, joystick, carService));

// wait for the state machine to stop
if (stateMachine.UpdaterTask is not null)
{
    await stateMachine.UpdaterTask;
}
