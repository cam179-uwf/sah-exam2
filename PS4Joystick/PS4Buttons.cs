using SharpDX.DirectInput;

namespace CarController.Services.PS4;

/// <summary>
/// All the buttons located on a PS4 remote controller.
/// </summary>
public enum PS4Buttons
{
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