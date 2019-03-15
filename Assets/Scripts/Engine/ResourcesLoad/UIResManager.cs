using UnityEngine;
using System.Collections;
using System;

public class UIResManager : Singleton<UIResManager>
{
	private class UIResLoadderData : LoadderData
	{
		private LoadderCallBack<GameObject> LoadCallBack;
		private LoadderData data;

		public UIResLoadderData(LoadderCallBack<GameObject> loadCallBack, LoadderData data)
		{
			LoadCallBack = loadCallBack;
			this.data = data;
		}

		public void Invoke(string resname, object obj)
		{
			if (obj != null)
			{
				var o = GameObject.Instantiate(obj as GameObject);
				LoadCallBack.Invoke(resname, o, data);
				return;
			}
			LoadCallBack.Invoke(resname, null, data);
		}
	}

	public void LoadAsset(string resName, LoadderCallBack<GameObject> LoadCallBack, LoadderData data)
	{
		if (LoadCallBack == null)
			return;

		if (string.IsNullOrEmpty(resName))
			LoadCallBack(null, null, data);

		var resPath = string.Format("prefabs/ui/{0}", resName);

		if (ResPoolManager.Instance.HasSpawn(resPath))
			ResPoolManager.Instance.LoadAsset(resPath, LoadCallBack, data);
		else
		{
			var loadData = new UIResLoadderData(LoadCallBack, data);
			ResManager.Instance.LoadAsset(resPath, OnLoadCallBack, loadData);
		}
	}

	public void LoadResources(string resName, LoadderCallBack<GameObject> LoadCallBack, LoadderData data)
	{
		if (LoadCallBack == null)
			return;

		if (string.IsNullOrEmpty(resName))
			LoadCallBack(null, null, data);

		var resPath = string.Format("prefabs/ui/{0}", resName);

		if (ResPoolManager.Instance.HasSpawn(resPath))
			ResPoolManager.Instance.LoadAsset(resPath, LoadCallBack, data);
		else
		{
			var loadData = new UIResLoadderData(LoadCallBack, data);
			ResManager.Instance.LoadResources(resPath, OnLoadCallBack, loadData);
		}
	}

	private void OnLoadCallBack(string resname, object obj, LoadderData data)
	{
		var loadData = (UIResLoadderData)data;
		loadData.Invoke(resname, obj);
	}
}
