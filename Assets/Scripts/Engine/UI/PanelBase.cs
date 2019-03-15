using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class PanelBase
{
	public bool quitAniEnd = true;

	private UIPanelID panelID;
	private PanelData panelData = null;

	private PanelBase parentPanel = null;
	private List<PanelBase> childsPanel = new List<PanelBase>();

	#region open
	public void OpenPanel(PanelBase parent, UIPanelID id, OpenPanelType forParentType, bool waitAni)
	{
		Reset();

		panelID = id;

		SetParentPanel(parent);

		if (parent == null)
		{
			ShowPanel(forParentType);
			return;
		}

		if (waitAni)
		{
			parent.AddQuitAniCallBack(()=>ShowPanel(forParentType));
		}
	}

	private void Reset()
	{
		//panelData = null;
		parentPanel = null;
		childsPanel.Clear();
	}

	private void SetParentPanel(PanelBase panel)
	{
		parentPanel = panel;
		if (parentPanel != null) parentPanel.AddChildsPanel(this);
	}

	private void AddChildsPanel(PanelBase panel)
	{
		childsPanel.Add(panel);
	}

	private void AddQuitAniCallBack(Action callBack)
	{
		if (quitAniEnd)
		{
			callBack.Invoke();
		}
	}

	private void ShowPanel(OpenPanelType forParentType)
	{
		InitPanelData(panelData);
	}
	#endregion

	#region close
	public void ClosePanel(bool waitAni, bool pool)
	{
		if (waitAni)
		{
			AddQuitAniCallBack(()=>HidePanel(pool));
			return;
		}

		HidePanel(pool);
	}

	private void HidePanel(bool pool)
	{
		if (pool) Hide();
		else Delete();
		if (parentPanel != null)
		{
			parentPanel.Show();
			RemoveFromParentPanel();
		}
	}

	private void Show()
	{
		panelData.SetActive(true);
	}

	private void Delete()
	{
		GameObject.Destroy(panelData.gameObject);
		panelData = null;
	}

	private void Hide()
	{
		panelData.SetActive(false);
	}

	private void RemoveFromParentPanel()
	{
		if (parentPanel != null) parentPanel.RemoveChildsPanel(this);
		parentPanel = null;
	}

	private void RemoveChildsPanel(PanelBase panel)
	{
		childsPanel.Remove(panel);
	}

	#endregion

	public void SetPanelData(PanelData cpn)
	{
		panelData = cpn;
	}

	public UIPanelID GetPanelID()
	{
		return panelID;
	}

	protected abstract void InitPanelData(PanelData data);

	public abstract void Update();
}
