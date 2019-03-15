using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIViewLogoData : PanelData
{
	public Image logo = null;

	protected override void Init()
	{
		if (!GetUiComponent(ref logo, transform, "logo")) enabled = false;
	}
}
