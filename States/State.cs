namespace CarController.States;

/// <summary>
/// An abstract class for defining a state.
/// </summary>
public abstract class State
{
    protected readonly IMutableState MutableState;
    
    protected State(IMutableState mutableState)
    {
        MutableState = mutableState;
    }
    
    /// <summary>
    /// Changes the currently running state.
    /// </summary>
    /// <param name="state"></param>
    protected async Task ChangeState(State state)
    {
        // exit the previous state
        await MutableState.State.OnExit();
        
        // change to and enter the new state
        MutableState.ChangeState(state);
        await MutableState.State.OnEnter();
    }
    
    /// <summary>
    /// Triggered when a state is entered.
    /// </summary>
    /// <returns></returns>
    public abstract Task OnEnter();
    
    /// <summary>
    /// Triggered when the state machine's Updator function commands it to.
    /// </summary>
    /// <returns></returns>
    public abstract Task OnUpdate();
    
    /// <summary>
    /// Triggered when a state is exited.
    /// </summary>
    /// <returns></returns>
    public abstract Task OnExit();
}