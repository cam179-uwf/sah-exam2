namespace CarController.States;

public abstract class State
{
    protected readonly IMutableState MutableState;
    
    protected State(IMutableState mutableState)
    {
        MutableState = mutableState;
    }

    protected async Task ChangeState(State state)
    {
        // exit the previous state
        await MutableState.State.OnExit();
        
        // change to and enter the new state
        MutableState.ChangeState(state);
        await MutableState.State.OnEnter();
    }
    
    public abstract Task OnEnter();
    public abstract Task OnUpdate();
    public abstract Task OnExit();
}