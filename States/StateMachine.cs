namespace CarController.States;

/// <summary>
/// A state machine that drives verious states.
/// </summary>
public class StateMachine : IMutableState
{
    public Task? UpdaterTask { get; private set; }
    public State State { get; private set; } = null!;
    
    private CancellationTokenSource? _cancellationTokenSource;
    private bool _isStarted;
    
    /// <summary>
    /// Start running the state machine with an initial state.
    /// </summary>
    /// <param name="root"></param>
    public async Task Start(State root)
    {
        if (_isStarted) return;
        _isStarted = true;
        
        State = root;
        
        await State.OnEnter();

        _cancellationTokenSource = new CancellationTokenSource();
        UpdaterTask = Task.Run(Updater);
    }

    /// <summary>
    /// Stop running the state machine.
    /// </summary>
    public async Task Stop()
    {
        if (!_isStarted) return;
        _isStarted = false;

        if (_cancellationTokenSource is not null)
        {
            await _cancellationTokenSource.CancelAsync();
        }
        
        await State.OnExit();

        if (UpdaterTask is not null)
        {
            await UpdaterTask;
        }
    }
    
    /// <summary>
    /// Updates the state machine periodically.
    /// </summary>
    private async Task Updater()
    {
        while (_cancellationTokenSource?.IsCancellationRequested is false)
        {
            await State.OnUpdate();
            await Task.Delay(10); // 0.01 seconds
        }
    }
    
    public void ChangeState(State state)
    {
        State = state;
    }
}