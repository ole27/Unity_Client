using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIViewLoginData : PanelData
{
	public InputField userName = null;
	public InputField userpwd = null;
	public UIButton loginButton = null;
	public UIButton toResigerButton = null;

	protected override void Init()
	{
		if(!GetUiComponent(ref userName, transform, "userName")) enabled = false;
		if(!GetUiComponent(ref userpwd, transform, "userpwd")) enabled = false;
		if(!GetUiComponent(ref loginButton, transform, "loginButton")) enabled = false;
		if(!GetUiComponent(ref toResigerButton, transform, "toResigerButton")) enabled = false;
	}
}
