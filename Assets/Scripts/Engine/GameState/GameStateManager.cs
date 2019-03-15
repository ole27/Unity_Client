

public class GameStateManager : Singleton<GameStateManager>
{
	private GameState curGameState = null;
	//private GameState preGameState = null;

	public void Init()
	{
		curGameState = null;
	}

	public void Update()
	{
		if(curGameState == null) return;
		curGameState.Update();
	}

	public void EnterState(GameState state)
	{
		if(curGameState != null)
			curGameState.Exit();
		//preGameState = curGameState;
		curGameState = state;
		curGameState.Enter();
	}

	public EGameState GetGameState()
	{
		return curGameState == null ? EGameState.None : curGameState.GetGameState();
	}

	public EGameSubState GetGameSubState()
	{
		return curGameState == null ? EGameSubState.None : curGameState.GetGameSubState();
	}

	public void EnterSubState(EGameSubState subState)
	{
		curGameState.EnterSubState(subState);
	}
}
