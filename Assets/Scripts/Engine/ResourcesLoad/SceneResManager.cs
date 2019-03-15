using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SceneResManager : Singleton<SceneResManager>
{
	private AsyncOperation asyncOperation = null;
	private bool isLoadEnd = false;
	private string curSceneName = "";

	public void LoadAsset(string resName, LoadderData data)
	{
		curSceneName = resName;
		isLoadEnd = false;
		var resPath = string.Format("prefabs/scenes/{0}", resName);
		ResManager.Instance.LoadAsset(resPath, OnLoadCallBack, data);
	}

	public void LoadResources(string resName, LoadderData data)
	{
		curSceneName = resName;
		isLoadEnd = false;
		var resPath = string.Format("prefabs/scenes/{0}", resName);
		OnLoadCallBack(resPath, null, data);
	}

	private void OnLoadCallBack(string resname, object obj, LoadderData data)
	{
		Game.Instance.StartCoroutine(LoadSceneAsync(curSceneName));
	}

	private IEnumerator LoadSceneAsync(string resname)
	{
		asyncOperation = SceneManager.LoadSceneAsync(resname);
		yield return new WaitWhile(() => asyncOperation.progress < 1);
		asyncOperation = null;
		isLoadEnd = true;
	}

	public bool IsLoadEnd()
	{
		return isLoadEnd;
	}

	public float GetProgress()
	{
		if (asyncOperation == null) return 1;
		return asyncOperation.progress;
	}
}
