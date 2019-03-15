using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GMManager : Singleton<GMManager>
{
	public delegate void GmCommandActionDelegate(string[] args = null);

	public static bool IsInEditor = true;
	private Dictionary<string, GmCommandActionDelegate> GMCommandDic = new Dictionary<string, GmCommandActionDelegate>();

	public void Execute(string[] args)
	{
		if (!GMCommandDic.ContainsKey(args[0]))
		{
			if (IsInEditor) Debug.LogError(string.Format("Command: '{0}' is not here !", args[0]));
			return;
		}
		var action = GMCommandDic[args[0]];
		if (action == null)
		{
			if (IsInEditor) Debug.LogError(string.Format("Command: '{0}' Action is null !", args[0]));
			return;
		}

		var list = args.ToList();
		list.RemoveAt(0);
		action(list.ToArray());
	}

	public void AddCommand(string command, GmCommandActionDelegate action)
	{
		if (GMCommandDic.ContainsKey(command))
		{
			if (IsInEditor) Debug.LogError(string.Format("Command: '{0}' has been here!", command));
			return;
		}

		GMCommandDic.Add(command, action);
	}
}
