using UnityEngine;
using System.Collections;

public class UIViewLogoPanel : PanelBase
{
	protected override void InitPanelData(PanelData data)
	{
		var panelData = (UIViewLogoData) data;
		var tex2D = new Texture2D(100,100);
		panelData.logo.sprite = Sprite.Create(tex2D, new Rect(0, 0, tex2D.width, tex2D.height), Vector2.one*0.5f);
	}


	private bool update = true;
	private float duration = 5.0f;
	public override void Update()
	{
		if(!update) return;
		if (duration < 0)
		{
			UIManager.Instance.OpenPanel(this, UIPanelID.ELogin, OpenPanelType.ShowParent, true);
			update = false;
		}
		else
		{
			duration -= Time.deltaTime;
		}
	}
}
