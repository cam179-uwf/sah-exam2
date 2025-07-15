namespace CarController.States;

public class StateMachine : IDisposable, IMutableState
{
    private CancellationTokenSource? _cancellationTokenSource;
    private bool _isStarted;

    public State State { get; private set; } = null!;
    
    public Task Start(State root)
    {
        if (_isStarted) return Task.CompletedTask;
        _isStarted = true;
        
        State = root;
        
        State.OnEnter();

        _cancellationTokenSource = new CancellationTokenSource();
        return Task.Run(Updater);
    }

    public void Stop()
    {
        if (!_isStarted) return;
        _isStarted = false;
        
        _cancellationTokenSource?.Cancel();
        State.OnExit();
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