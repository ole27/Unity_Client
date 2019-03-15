using UnityEngine;
using System.Collections;

public class GameStateMenu : GameState
{
	public override void Enter()
	{
		EnterState(EGameState.Menu);
		EnterSubState(EGameSubState.Menu_Prepare);
		Debug.Log("Enter " + GetGameState() + " " + GetGameSubState());
	}

	public override void Exit()
	{
		Debug.Log("Exit " + GetGameState() + " " + GetGameSubState());
	}

	protected override void Update(EGameSubState subState)
	{
		Debug.Log(GetGameState() + " " + GetGameSubState());
		switch (subState)
		{
			case EGameSubState.Menu_Prepare:
				GameStateManager.Instance.EnterSubState(EGameSubState.Menu_Main);
				break;
			case EGameSubState.Menu_Main:
				GameStateManager.Instance.EnterSubState(EGameSubState.Menu_Shop);
				break;
			case EGameSubState.Menu_Shop:
				GameStateManager.Instance.EnterSubState(EGameSubState.Menu_Equment);
				break;
			case EGameSubState.Menu_Equment:
				GameStateManager.Instance.EnterState(new GameStatePlay());
				break;
		}
	}
}
