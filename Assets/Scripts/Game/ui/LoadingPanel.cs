using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LoadingPanel : PanelData
{
	public Text DownInfoText = null;
	public Slider ProgressSlider = null;
	public Text ProgressText = null;

	protected override void Init()
	{
		DownInfoText.text = "正在准备...";
		ProgressSlider.value = 0;
		ProgressText.text = 0 + "%";
	}

	private int downMaxCount = -1;
	private int downCurCount = -1;
	private float progress = 0;
	void Update ()
	{
		switch (ResManager.Instance.GetResManagerState())
		{
			case ResManager.ResManagerState.None:
				break;
			case ResManager.ResManagerState.EDownABManifest:
				DownInfoText.text = "下载Manifest...";
				break;
			case ResManager.ResManagerState.ECheckResVersion:
				DownInfoText.text = "检测版本...";
				break;
			case ResManager.ResManagerState.EUpdateAB:
				DownInfoText.text = HttpManager.Instance.GetDownLoadInfo();
				if (downMaxCount == -1)
				{
					downMaxCount = HttpManager.Instance.GetDownLoadListCount();
				}
				downCurCount = HttpManager.Instance.GetDownLoadListCount();
				progress = 1f - 1f*downCurCount/downMaxCount;
				ProgressSlider.value = progress;
				ProgressText.text = string.Format("{0}%",(int)(progress*100));
				break;
			case ResManager.ResManagerState.EAsyncLoadRes:
				break;
			case ResManager.ResManagerState.EError:
				break;
			case ResManager.ResManagerState.EFree:
				ProgressSlider.value = 1;
				ProgressText.text = string.Format("{0}%", (int)(1 * 100));
				DownInfoText.text = "完成...";
				break;
			default:
				break;
		}
	}
}
