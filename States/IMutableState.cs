namespace CarController.States;

public interface IMutableState
{
    public State State { get; }
    public void ChangeState(State state);
}