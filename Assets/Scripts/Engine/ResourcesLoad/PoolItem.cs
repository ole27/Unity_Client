using UnityEngine;
using System.Collections;

public class PoolItem : MonoBehaviour
{
	public string prefabName = "";
	public bool used = false;

	public void Init(string resname, bool used = false)
	{
		this.used = used;
		prefabName = resname;
		if (used != gameObject.activeSelf) gameObject.SetActive(used);
	}

	public void Spawn()
	{
		if (used) return;
		used = true;
		if (!gameObject.activeSelf) gameObject.SetActive(true);
	}

	public void Recycle()
	{
		if (!used) return;
		used = false;
		if (gameObject.activeSelf) gameObject.SetActive(false);
		ResPoolManager.Instance.Recycle(prefabName, this);
	}
}
