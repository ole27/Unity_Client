using System;
using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
	public string server_ip = "127.0.0.1";
	public string server_port = "55000";
	public string server_relRoot = "Web/Game";
	public static string server_root = "";

	public bool useAB = true;
	public bool updateABVesion = true;
	public bool showDebugLog = true;

	public static Game Instance = null;

	private GameObject uiRoot2D = null;

	void Awake()
	{
		Instance = this;
		DontDestroyOnLoad(this);
		server_root = string.Format("http://{0}/{1}/", server_ip, server_relRoot);

		uiRoot2D = GameObject.Instantiate(Resources.Load<GameObject>("UIRoot2D"));
		uiRoot2D.name = "UIRoot2D";
		GameObject.DontDestroyOnLoad(uiRoot2D);
		var loadingUi = GameObject.Instantiate(Resources.Load<GameObject>("LoadingPanel"));
		loadingUi.transform.SetParent(uiRoot2D.transform, false);

		StartCoroutine(Init());
	}

	IEnumerator Init ()
	{
		yield return new WaitForEndOfFrame();

		GMManager.IsInEditor = showDebugLog;
		GMManager.Instance.AddCommand("quit", DebugUI.Instance.ClosGame);
		GMManager.Instance.AddCommand("fps", DebugUI.Instance.StartFps);
		GMManager.Instance.AddCommand("stopfps", DebugUI.Instance.StopFps);
	
		HttpManager.Instance.Init();

		ResManager.Instance.Init(useAB, updateABVesion);
		yield return new WaitUntil(()=> ResManager.Instance.InitEnd());

		GameStateManager.Instance.Init();
		GameStateManager.Instance.EnterState(new GameStatePlay());

		ResPoolManager.Instance.Init();
		yield return new WaitUntil(ResPoolManager.Instance.IsInitEnd);

		SceneResManager.Instance.LoadAsset("scene001", null);
		yield return new WaitUntil(SceneResManager.Instance.IsLoadEnd);

		UIManager.Instance.Init(uiRoot2D.transform);
		yield return new WaitUntil(UIManager.Instance.IsLoadEnd);
		UIManager.Instance.OpenPanel(null, UIPanelID.ELogo, OpenPanelType.ShowParent, false);

		ModelResManager.Instance.LoadAsset("dj001_kirigaya_kazuto", LoadCallBack1, null);
		ModelResManager.Instance.LoadAsset("dj002_asuna", LoadCallBack3, null);
		ModelResManager.Instance.LoadAsset("dj001_kirigaya_kazuto", LoadCallBack2, null);
		ModelResManager.Instance.LoadAsset("dj002_asuna", LoadCallBack4, null);
		VfxResManager.Instance.LoadAsset("vfx_001", LoadCallBack5, null);
	}

	private void LoadCallBack5(string resname, GameObject obj, LoadderData data)
	{
		if (obj == null)
		{
			Debug.Log("is null!!!!!!!");
			return;
		}
		obj.transform.position = new Vector3(-5.14f, 3.23f, 9.07f);
	}

	GameObject[] gobjs = new GameObject[4];
	private void LoadCallBack1(string resname, GameObject obj, LoadderData data)
	{
		if (obj == null)
		{
			Debug.Log("is null!!!!!!!");
			return;
		}

		//Debug.Log(data == null ? "data is null" : "data is not null");
		gobjs[0] = obj;
		gobjs[0].transform.position = new Vector3(-5.14f, 2.23f, 9.07f);
	}

	private void LoadCallBack2(string resname, GameObject obj, LoadderData data)
	{
		if (obj == null)
		{
			Debug.Log("is null!!!!!!!");
			return;
		}

		//Debug.Log(data == null ? "data is null" : "data is not null");
		gobjs[1] = obj;
		gobjs[1].transform.position = new Vector3(-2.757f, 1.91f, 7.728f);
	}

	private void LoadCallBack3(string resname, GameObject obj, LoadderData data)
	{
		if (obj == null)
		{
			Debug.Log("is null!!!!!!!");
			return;
		}

		//Debug.Log(data == null ? "data is null" : "data is not null");
		gobjs[2] = obj;
		gobjs[2].transform.position = new Vector3(-2.757f, 1.91f, 8.728f);
	}

	private void LoadCallBack4(string resname, GameObject obj, LoadderData data)
	{
		if (obj == null)
		{
			Debug.Log("is null!!!!!!!");
			return;
		}

		//Debug.Log(data == null ? "data is null" : "data is not null");
		gobjs[3] = obj;
		gobjs[3].transform.position = new Vector3(-2.757f, 1.91f, 9.728f);
	}

	void Update ()
	{
		GameStateManager.Instance.Update();
		HttpManager.Instance.Update();
		UIManager.Instance.Update();
	}

	void OnDestroy()
	{
		Resources.UnloadUnusedAssets();
		GC.Collect();
	}

	void OnGUI()
	{
		if (GUI.Button(new Rect(0, 0, 100, 50), "d1"))
		{
			if (gobjs[0] != null)// Destroy(gobjs[0]);
			ResManager.Instance.Destroy(gobjs[0]);
			gobjs[0] = null;
		}
		if (GUI.Button(new Rect(0, 50, 100, 50), "d2"))
		{
			if (gobjs[1] != null)// Destroy(gobjs[1]);
			ResManager.Instance.Destroy(gobjs[1]);
			gobjs[1] = null;
		}
		if (GUI.Button(new Rect(0, 100, 100, 50), "d3"))
		{
			if (gobjs[2] != null)// Destroy(gobjs[2]);
			ResManager.Instance.Destroy(gobjs[2]);
			gobjs[2] = null;
		}
		if (GUI.Button(new Rect(0, 150, 100, 50), "d4"))
		{
			if (gobjs[3] != null)// Destroy(gobjs[3]);
			ResManager.Instance.Destroy(gobjs[3]);
			gobjs[3] = null;
		}

		if (GUI.Button(new Rect(100, 0, 100, 50), "a1"))
		{
			ModelResManager.Instance.LoadAsset("dj001_kirigaya_kazuto", LoadCallBack1, null);
		}
		if (GUI.Button(new Rect(100, 50, 100, 50), "a2"))
		{
			ModelResManager.Instance.LoadAsset("dj001_kirigaya_kazuto", LoadCallBack2, null);
		}
		if (GUI.Button(new Rect(100, 100, 100, 50), "a3"))
		{
			ModelResManager.Instance.LoadAsset("dj002_asuna", LoadCallBack3, null);
		}
		if (GUI.Button(new Rect(100, 150, 100, 50), "a4"))
		{
			ModelResManager.Instance.LoadAsset("dj002_asuna", LoadCallBack4, null);
		}
	}
}
