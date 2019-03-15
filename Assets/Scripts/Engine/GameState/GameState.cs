

public abstract class GameState
{
	protected EGameSubState _subState = EGameSubState.None;
	protected EGameSubState _preSubState = EGameSubState.None;
	protected EGameState _state;

	public abstract void Enter();

	public void EnterState(EGameState state)
	{
		_state = state;
	}

	public void EnterSubState(EGameSubState subState)
	{
		_preSubState = _subState;
		_subState = subState;
	}

	public abstract void Exit();

	public virtual EGameState GetGameState()
	{
		return _state;
	}

	public virtual EGameSubState GetGameSubState()
	{
		return _subState;
	}

	public virtual EGameSubState GetGamePreSubState()
	{
		return _preSubState;
	}

	public void Update()
	{
		Update(_subState);
	}

	protected abstract void Update(EGameSubState subState);
}
