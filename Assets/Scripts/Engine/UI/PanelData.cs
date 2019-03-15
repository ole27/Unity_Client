using UnityEngine;
using System.Collections;

public abstract class PanelData : MonoBehaviour
{
	void Awake()
	{
		Init();
	}

	public bool GetUiComponent<T>(ref T cpn, Transform trf, string gameObjectName)
	{
		if (cpn != null) return true;
		var a = trf.Find(gameObjectName);
		if (a == null) return false;
		cpn = a.GetComponent<T>();
		return cpn != null;
	}

	public void Show()
	{
		gameObject.SetActive(true);
	}

	protected abstract void Init();

	public void SetActive(bool b)
	{
		if (gameObject.activeSelf != b)
			gameObject.SetActive(b);
	}
}
