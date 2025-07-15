namespace CarController.Services.PS4;

/// <summary>
/// Describes a found gamepad device.
/// </summary>
internal class GamepadDevice
{
    public required Guid Guid { get; set; }
    public required string Name { get; set; }

    public override string ToString()
    {
        return $"(Guid: {Guid}, Name: {Name})";
    }
}