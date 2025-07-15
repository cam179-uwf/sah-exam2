namespace CarController.States;

public abstract class State
{
    protected readonly IMutableState _mutableState;
    
    protected State(IMutableState mutableState)
    {
        _mutableState = mutableState;
    }

    protected void ChangeState(State state)
    {
        // exit the previous state
        _mutableState.State.OnExit();
        
        // change to and enter the new state
        _mutableState.ChangeState(state);
        _mutableState.State.OnEnter();
    }
    
    public abstract Task OnEnter();
    public abstract Task OnUpdate();
    public abstract Task OnExit();
}