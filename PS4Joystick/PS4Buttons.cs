using SharpDX.DirectInput;

namespace CarController.Services.PS4;

public enum PS4Buttons
{
    // RightArrow = (PointOfViewControllers0, Value: 9000)
    // LeftArrow = (PointOfViewControllers0, Value: 27000)
    // UpArrow = (PointOfViewControllers0, Value: 0)
    // DownArrow = (PointOfViewControllers0, Value: 18000)
    LeftArrow = 0,
    RightArrow = 1,
    UpArrow = 2,
    DownArrow = 3,
    Square = JoystickOffset.Buttons0,
    Triangle = JoystickOffset.Buttons3,
    Circle = JoystickOffset.Buttons2,
    X = JoystickOffset.Buttons1,
    R1 = JoystickOffset.Buttons5,
    L1 = JoystickOffset.Buttons4,
    R2 = JoystickOffset.Buttons7,
    L2 = JoystickOffset.Buttons6,
    Options = JoystickOffset.Buttons9,
    Share = JoystickOffset.Buttons8,
    PsButton = JoystickOffset.Buttons12,
    RightJoystick = JoystickOffset.Buttons11,
    LeftJoystick = JoystickOffset.Buttons10,
    TouchPad = JoystickOffset.Buttons13
}