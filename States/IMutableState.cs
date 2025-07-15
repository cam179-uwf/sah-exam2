namespace CarController.States;

/// <summary>
/// An interface to define a mutable state.
/// </summary>
public interface IMutableState
{
    public State State { get; }
    public void ChangeState(State state);
}