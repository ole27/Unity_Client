using UnityEngine;
using UnityEngine.UI;

public class DownLoading : MonoBehaviour
{
	public Text LoadingInfo = null;
	// Use this for initialization
	void Start ()
	{
		if (LoadingInfo == null)
			LoadingInfo = GetComponentInChildren<Text>();
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (LoadingInfo == null) return;

		LoadingInfo.text = HttpManager.Instance.GetDownLoadInfo();
	}
}
