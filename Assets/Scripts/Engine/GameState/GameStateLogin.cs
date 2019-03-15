using UnityEngine;
using System.Collections;

public class GameStateLogin : GameState
{
	public override void Enter()
	{
		EnterState(EGameState.Login);
		EnterSubState(EGameSubState.Login_Prepare);
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
			case EGameSubState.Login_Prepare:
				GameStateManager.Instance.EnterSubState(EGameSubState.Login_CheckVersion);
				break;
			case EGameSubState.Login_CheckVersion:
				GameStateManager.Instance.EnterSubState(EGameSubState.Login_UpdateVersion);
				//GameStateManager.Instance.ChaneSubState(EGameSubState.Login_UI);
				break;
			case EGameSubState.Login_UpdateVersion:
				GameStateManager.Instance.EnterSubState(EGameSubState.Login_UI);
				break;
			case EGameSubState.Login_UI:
				GameStateManager.Instance.EnterState(new GameStateMenu());
				break;
		}
	}
}
