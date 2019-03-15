using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIViewLoginPanel : PanelBase
{
	public InputField userName = null;
	public InputField userpwd = null;
	public UIButton loginButton = null;
	public UIButton toResigerButton = null;

	protected override void InitPanelData(PanelData data)
	{
		var panelData = (UIViewLoginData)data;
		userName = panelData.userName;
		userpwd = panelData.userpwd;
		loginButton = panelData.loginButton;
		toResigerButton = panelData.toResigerButton;

		loginButton.ButtonLabel = "登陆";
		toResigerButton.ButtonLabel = "注册";

		loginButton.onClick.RemoveAllListeners();
		toResigerButton.onClick.RemoveAllListeners();

		loginButton.onClick.AddListener(() =>
		{
			UIManager.Instance.ClosePanel(this, false);
		});
		toResigerButton.onClick.AddListener(() =>
		{
			UIManager.Instance.ClosePanel(this, false);
		});
	}

	public override void Update()
	{
		
	}
}
