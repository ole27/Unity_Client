
public enum EGameState
{
	None = -1,
	Login,
	Menu,
	Play,
}

public enum EGameSubState
{
	None = -1,
	//////
	Login_Prepare,
	Login_CheckVersion,
	Login_UpdateVersion,
	Login_UI,
	//////
	Menu_Prepare,
	Menu_Main,
	Menu_Shop,
	Menu_Equment,
	//////
	Play_Prepare,
	Play_Settle,
}
