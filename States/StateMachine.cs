namespace CarController.States;

public class StateMachine : IDisposable, IMutableState
{
    private CancellationTokenSource? _cancellationTokenSource;
    private bool _isStarted;
    public Task? UpdaterTask { get; private set; }

    public State State { get; private set; } = null!;
    
    public async Task Start(State root)
    {
        if (_isStarted) return;
        _isStarted = true;
        
        State = root;
        
        await State.OnEnter();

        _cancellationTokenSource = new CancellationTokenSource();
        UpdaterTask = Task.Run(Updater);
    }

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

    private async Task Updater()
    {
        while (_cancellationTokenSource?.IsCancellationRequested is false)
        {
            await State.OnUpdate();
            await Task.Delay(10); // 0.01 seconds
        }
    }

    public void Dispose()
    {
        State.OnExit();
    }

    public void ChangeState(State state)
    {
        State = state;
    }
}