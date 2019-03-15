using UnityEngine;
using System.Collections;

public class GameStatePlay : GameState
{
	public override void Enter()
	{
		EnterState(EGameState.Play);
		EnterSubState(EGameSubState.Play_Prepare);
		//Debug.Log("Enter " + GetGameState() + " " + GetGameSubState());
	}

	public override void Exit()
	{
		//Debug.Log("Exit " + GetGameState() + " " + GetGameSubState());
	}

	protected override void Update(EGameSubState subState)
	{
		switch (subState)
		{
			case EGameSubState.Play_Prepare:
				//Debug.Log(GetGameState() + " " + GetGameSubState());
				GameStateManager.Instance.EnterSubState(EGameSubState.Play_Settle);
				break;
			case EGameSubState.Play_Settle:
				//Debug.Log(GetGameState() + " " + GetGameSubState());
				GameStateManager.Instance.EnterSubState(EGameSubState.None);
				break;
		}
	}
}
