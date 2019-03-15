using System;
using System.Collections.Generic;
using UnityEngine;

public enum UIPanelID
{
	ELogo,
	ELogin,
	ERegister,
}

public enum OpenPanelType
{
	ShowParentAndInteractive,
	ShowParent,
	HideParent,
	DelParent
}

public class UIPanelConfig
{
	public string prefabName;
	public string scriptName;

	public UIPanelConfig(string prefabName, string scriptName)
	{
		this.prefabName = prefabName;
		this.scriptName = scriptName;
	}
}

public class UIManager : Singleton<UIManager>
{
	private bool isInitEnd = false;

	private RectTransform uiRootRectTransform = null;

	private List<PanelBase> runPanelBaseList = new List<PanelBase>();
	private Dictionary<UIPanelID, PanelBase> mUIPanelPoolDic = new Dictionary<UIPanelID, PanelBase>();

	public class UILoadderData : LoadderData
	{
		public UIPanelID id;
		//public UIPanelConfig config;
		public PanelBase parentPanel;
		public OpenPanelType forParentType;
		public bool waitAni;
	}

	private Dictionary<UIPanelID, UIPanelConfig> mUIPanelConfigDic = new Dictionary<UIPanelID, UIPanelConfig>();

	public UIManager()
	{
		AddConfig(UIPanelID.ELogo, "ui_view_logo", "UIViewLogoPanel");
		AddConfig(UIPanelID.ELogin, "ui_view_login", "UIViewLoginPanel");
		AddConfig(UIPanelID.ERegister, "ui_view_register", "UIViewRegisterPanel");
	}

	private void AddConfig(UIPanelID id, string prefabName, string scriptName)
	{
		if (mUIPanelConfigDic.ContainsKey(id))
		{
			if(GMManager.IsInEditor) Debug.LogError("重复的UIPanelID " + id);
			return;
		}

		mUIPanelConfigDic.Add(id, new UIPanelConfig(prefabName, scriptName));
	}

	public void Init(Transform uiRoot)
	{
		isInitEnd = false;
		runPanelBaseList.Clear();
		mUIPanelPoolDic.Clear();
		if (uiRoot == null)
			UIResManager.Instance.LoadAsset("canvas", UIRootLoadCallBack, null);
		else
			UIRootLoadCallBack(uiRoot.name, uiRoot.gameObject, null);
	}

	private void UIRootLoadCallBack(string resName, GameObject obj, LoadderData data)
	{
		if (obj == null)
		{
			if(GMManager.IsInEditor) Debug.LogError(resName + "is null!");
			isInitEnd = true;
			return;
		}

		GameObject.DontDestroyOnLoad(obj);

		uiRootRectTransform = obj.transform as RectTransform;
		isInitEnd = true;
	}

	public bool IsLoadEnd()
	{
		return isInitEnd;
	}

	public void OpenPanel(PanelBase panelBase, UIPanelID id, OpenPanelType forParentType, bool waitAni)
	{
		if (!isInitEnd) return;
		if (mUIPanelConfigDic.ContainsKey(id))
		{
			if (mUIPanelPoolDic.ContainsKey(id))
			{
				mUIPanelPoolDic[id].OpenPanel(panelBase, id, forParentType, waitAni);
				runPanelBaseList.Add(mUIPanelPoolDic[id]);
			}
			else
			{
				var data = new UILoadderData
				{
					id = id,
					//config = mUIPanelConfigDic[id],
					parentPanel = panelBase,
					forParentType = forParentType,
					waitAni= waitAni
				};
				UIResManager.Instance.LoadAsset(mUIPanelConfigDic[id].prefabName, UILoadCallBack, data);
			}
		} else
		{
			if(GMManager.IsInEditor) Debug.LogError("mUIPanelConfigDic 不存在 id:" + id);
		}
	}

	private void UILoadCallBack(string resName, GameObject obj, LoadderData data)
	{
		if(obj == null)
		{
			if(GMManager.IsInEditor) Debug.LogError("UI " + resName + "不存在!");
			return;
		}

		try
		{
			obj.transform.SetParent(uiRootRectTransform, false);
			var loadData = (UILoadderData)data;
			var config = mUIPanelConfigDic[loadData.id];// loadData.config;
			var type = Type.GetType(config.scriptName);
			var panel = Activator.CreateInstance(type) as PanelBase;
			var cpn = obj.GetComponent<PanelData>();
			panel.SetPanelData(cpn);
			panel.OpenPanel(loadData.parentPanel, loadData.id, loadData.forParentType, loadData.waitAni);
			runPanelBaseList.Add(panel);
		}
		catch (Exception e)
		{
			if(GMManager.IsInEditor) Debug.LogError(e);
		}
	}

	public void ClosePanel(PanelBase panelBase, bool waitAni, bool pool = false)
	{
		runPanelBaseList.Remove(panelBase);
		if(pool) mUIPanelPoolDic.Add(panelBase.GetPanelID(), panelBase);
		panelBase.ClosePanel(waitAni, pool);
		if (!pool) panelBase = null;
	}

	public void Update()
	{
		if (runPanelBaseList.Count == 0) return;
		for (var i = 0; i < runPanelBaseList.Count; i++)
		{
			runPanelBaseList[i].Update();
		}
	}

}
