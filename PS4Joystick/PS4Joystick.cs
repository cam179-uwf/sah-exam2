using System.Timers;
using SharpDX.DirectInput;
using Timer = System.Timers.Timer;

namespace CarController.Services.PS4;

/// <summary>
/// Defines a PS4 controller and its events.
/// </summary>
public class PS4Joystick
{
    public float LeftJoystickDeadZone { get; private set; } = 0.15F;
    public float RightJoystickDeadZone { get; private set; } = 0.15F;
    public float MoveX { get; private set; }
    public float MoveY { get; private set; }
    public float RotationX { get; private set; }
    public float RotationY { get; private set; }
    public float LeftPaddle { get; private set; }
    public float RightPaddle { get; private set; }

    private readonly Joystick _joystick;
    private GamepadDevice GamepadDevice { get; set; }
    private readonly Dictionary<PS4Buttons, bool> _buttonStates = [];
    private readonly Dictionary<PS4Buttons, bool> _previousButtonStates = [];

    public PS4Joystick()
    {
        // initialize the button state dictionaries with all the PS4 buttons
        foreach (var value in Enum.GetValues(typeof(PS4Buttons)))
        {
            _buttonStates.Add((PS4Buttons)value, false);
            _previousButtonStates.Add((PS4Buttons)value, false);
        }
        
        // find an available gamepad device, otherwise throw an error
        GamepadDevice = FindAvailableJoysticks().FirstOrDefault() 
                        ?? throw new PS4JoystickException("No available controllers found.");
        
        // don't really know why we have to do this with this library
        var directInput = new DirectInput();
        
        // anyway, we register a new joystick using that directInput
        _joystick = new Joystick(directInput, GamepadDevice.Guid);

        // we set a buffer size
        _joystick.Properties.BufferSize = 128;
        
        // and then we acquire the joystick's focus
        _joystick.Acquire();
    }
    
    /// <summary>
    /// Finds all the connected gamepad devices.
    /// </summary>
    /// <returns></returns>
    private static IEnumerable<GamepadDevice> FindAvailableJoysticks()
    {
        return new DirectInput()
            .GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly)
            .Select(di => new GamepadDevice { Guid = di.InstanceGuid, Name = di.InstanceName })
            .AsEnumerable();
    }

    /// <summary>
    /// Determines if a button is currently pressed.
    /// </summary>
    public bool IsButtonPressed(PS4Buttons ps4Button) => _buttonStates[ps4Button];
    
    /// <summary>
    /// Only true when a button has just been pressed.
    /// </summary>
    /// <param name="ps4Button"></param>
    /// <returns></returns>
    public bool OnButtonDown(PS4Buttons ps4Button) => !_previousButtonStates[ps4Button] && _buttonStates[ps4Button];
    
    /// <summary>
    /// Only true when a button has just been released.
    /// </summary>
    /// <param name="ps4Button"></param>
    /// <returns></returns>
    public bool OnButtonUp(PS4Buttons ps4Button) => _previousButtonStates[ps4Button] && !_buttonStates[ps4Button];
    
    /// <summary>
    /// Update method to update the joystick's data.
    /// This method must be called by the user.
    /// </summary>
    public void Update()
    {
        _joystick.Poll();
        var bufferedData = _joystick.GetBufferedData();
        
        // set all of our previous button states to the current button states
        _previousButtonStates[PS4Buttons.Square] = _buttonStates[PS4Buttons.Square];
        _previousButtonStates[PS4Buttons.Triangle] = _buttonStates[PS4Buttons.Triangle];
        _previousButtonStates[PS4Buttons.Circle] = _buttonStates[PS4Buttons.Circle];
        _previousButtonStates[PS4Buttons.X] = _buttonStates[PS4Buttons.X];
        _previousButtonStates[PS4Buttons.R1] = _buttonStates[PS4Buttons.R1];
        _previousButtonStates[PS4Buttons.L1] = _buttonStates[PS4Buttons.L1];
        _previousButtonStates[PS4Buttons.R2] = _buttonStates[PS4Buttons.R2];
        _previousButtonStates[PS4Buttons.L2] = _buttonStates[PS4Buttons.L2];
        _previousButtonStates[PS4Buttons.Options] = _buttonStates[PS4Buttons.Options];
        _previousButtonStates[PS4Buttons.Share] = _buttonStates[PS4Buttons.Share];
        _previousButtonStates[PS4Buttons.PsButton] = _buttonStates[PS4Buttons.PsButton];
        _previousButtonStates[PS4Buttons.RightJoystick] = _buttonStates[PS4Buttons.RightJoystick];
        _previousButtonStates[PS4Buttons.LeftJoystick] = _buttonStates[PS4Buttons.LeftJoystick];
        _previousButtonStates[PS4Buttons.TouchPad] = _buttonStates[PS4Buttons.TouchPad];
        _previousButtonStates[PS4Buttons.RightArrow] = _buttonStates[PS4Buttons.RightArrow];
        _previousButtonStates[PS4Buttons.LeftArrow] = _buttonStates[PS4Buttons.LeftArrow];
        _previousButtonStates[PS4Buttons.UpArrow] = _buttonStates[PS4Buttons.UpArrow];
        _previousButtonStates[PS4Buttons.DownArrow] = _buttonStates[PS4Buttons.DownArrow];

        // go through the buffer and depending on which
        // events were collected by the buffer change
        // values depending on those events
        //
        // for example: buttons presses, joystick movements, trigger movements... etc.
        foreach (var state in bufferedData)
        {
            switch (state.Offset)
            {
                case JoystickOffset.X:
                    // 0.00003052F == 1 / 2^15
                    var moveX = state.Value * 0.00003052F - 1; // just to normalize the vector
                    MoveX = Math.Clamp(Math.Abs(moveX) > LeftJoystickDeadZone ? moveX : 0, -1, 1);
                    break;
                case JoystickOffset.Y:
                    var moveY = -(state.Value * 0.00003052F - 1); // just to normalize the vector
                    MoveY = Math.Clamp(Math.Abs(moveY) > LeftJoystickDeadZone ? moveY : 0, -1, 1);
                    break;
                case JoystickOffset.Z:
                    var rotationX = state.Value * 0.00003052F - 1; // just to normalize the vector
                    RotationX = Math.Clamp(Math.Abs(rotationX) > RightJoystickDeadZone ? rotationX : 0, -1, 1);
                    break;
                case JoystickOffset.RotationZ:
                    var rotationY = -(state.Value * 0.00003052F - 1); // just to normalize the vector
                    RotationY = Math.Clamp(Math.Abs(rotationY) > RightJoystickDeadZone ? rotationY : 0, -1, 1);
                    break;
                case JoystickOffset.RotationX:
                    LeftPaddle = Math.Clamp(state.Value * 0.00003052F * 0.5F, 0, 1);
                    break;
                case JoystickOffset.RotationY:
                    RightPaddle = Math.Clamp(state.Value * 0.00003052F * 0.5F, 0, 1);
                    break;
                case JoystickOffset.Buttons0:
                    _buttonStates[PS4Buttons.Square] = state.Value > 0;
                    break;
                case JoystickOffset.Buttons3:
                    _buttonStates[PS4Buttons.Triangle] = state.Value > 0;
                    break;
                case JoystickOffset.Buttons2:
                    _buttonStates[PS4Buttons.Circle] = state.Value > 0;
                    break;
                case JoystickOffset.Buttons1:
                    _buttonStates[PS4Buttons.X] = state.Value > 0;
                    break;
                case JoystickOffset.Buttons5:
                    _buttonStates[PS4Buttons.R1] = state.Value > 0;
                    break;
                case JoystickOffset.Buttons4:
                    _buttonStates[PS4Buttons.L1] = state.Value > 0;
                    break;
                case JoystickOffset.Buttons7:
                    _buttonStates[PS4Buttons.R2] = state.Value > 0;
                    break;
                case JoystickOffset.Buttons6:
                    _buttonStates[PS4Buttons.L2] = state.Value > 0;
                    break;
                case JoystickOffset.Buttons9:
                    _buttonStates[PS4Buttons.Options] = state.Value > 0;
                    break;
                case JoystickOffset.Buttons8:
                    _buttonStates[PS4Buttons.Share] = state.Value > 0;
                    break;
                case JoystickOffset.Buttons12:
                    _buttonStates[PS4Buttons.PsButton] = state.Value > 0;
                    break;
                case JoystickOffset.Buttons11:
                    _buttonStates[PS4Buttons.RightJoystick] = state.Value > 0;
                    break;
                case JoystickOffset.Buttons10:
                    _buttonStates[PS4Buttons.LeftJoystick] = state.Value > 0;
                    break;
                case JoystickOffset.Buttons13:
                    _buttonStates[PS4Buttons.TouchPad] = state.Value > 0;
                    break;
                case JoystickOffset.PointOfViewControllers0:
                    // TODO: possible problem is if two directions are pressed at once
                    switch (state.Value)
                    {
                        case 9000:
                            _buttonStates[PS4Buttons.RightArrow] = true;
                            break;
                        case 27000:
                            _buttonStates[PS4Buttons.LeftArrow] = true;
                            break;
                        case 0:
                            _buttonStates[PS4Buttons.UpArrow] = true;
                            break;
                        case 18000:
                            _buttonStates[PS4Buttons.DownArrow] = true;
                            break;
                        default:
                            _buttonStates[PS4Buttons.RightArrow] = false;
                            _buttonStates[PS4Buttons.LeftArrow] = false;
                            _buttonStates[PS4Buttons.UpArrow] = false;
                            _buttonStates[PS4Buttons.DownArrow] = false;
                            break;
                    }
                    break;
            }
        }
    }

    public string ToAxisString()
        => $"Axis[ MoveX: {MoveX:N2}, MoveY: {MoveY:N2}, RotationX: {RotationX:N2}, RotationY: {RotationY:N2}, LeftPaddle: {LeftPaddle:N2}, RightPaddle: {RightPaddle:N2} ]\n";

    public string ToButtonString()
        => _buttonStates.Aggregate(string.Empty, (current, state) => current + $"Button: {state.Key}, IsPressed: {state.Value}\n");
    
    public override string ToString() => ToAxisString() + ToButtonString();
}

// this is just an exception for handling problems
public class PS4JoystickException(string message) : Exception(message);