using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class ResPoolManager : Singleton<ResPoolManager>
{
	private Dictionary<string, SpawnPool> SpawnPoolDic = new Dictionary<string, SpawnPool>();
	private Transform spawnRoot = null;

	public void Init()
	{
		//return;
		var o = GameObject.Find("SpawnRoot") ?? new GameObject("SpawnRoot");
		GameObject.DontDestroyOnLoad(o);
		spawnRoot = o.transform;
		SpawnPoolDic.Clear();

		var sp = new SpawnPool
		{
			parentRoot = spawnRoot,
			prefabName = "prefabs/models/dj001_kirigaya_kazuto",
			spawnCount = 2
		};
		sp.Init();
		SpawnPoolDic.Add(sp.prefabName, sp);
		var sp1 = new SpawnPool
		{
			parentRoot = spawnRoot,
			prefabName = "prefabs/models/dj002_asuna",
			spawnCount = 2
		};
		sp1.Init();
		SpawnPoolDic.Add(sp1.prefabName, sp1);
	}

	public bool IsInitEnd()
	{
		return SpawnPoolDic.Aggregate(true, (current, item) => current & item.Value.IsInitEnd());
	}

	public void LoadAsset(string resName, LoadderCallBack<GameObject> LoadCallBack, LoadderData data)
	{
		LoadCallBack(resName, SpawnPoolDic[resName].GetSpwan(), data);
	}

	public bool HasSpawn(string resPath)
	{
		return SpawnPoolDic.ContainsKey(resPath) && SpawnPoolDic[resPath].HasSpawn();
	}

	internal void Recycle(string prefabName, PoolItem poolItem)
	{
		SpawnPoolDic[prefabName].Recycle(poolItem);
	}
}

[System.Serializable]
public class SpawnPool
{
	public string prefabName = "";
	public int spawnCount = 0;

	public Transform parentRoot = null;
	public GameObject prefab = null;

	private List<PoolItem> poolItemList = new List<PoolItem>();
	private bool isInitEnd = true;
	public void Init()
	{
		isInitEnd = false;
		poolItemList.Clear();
		if (string.IsNullOrEmpty(prefabName)) return;
		if (prefab == null)
		{
			ResManager.Instance.LoadAsset(prefabName, OnLoadCallBack, null);
			return;
		}
		OnLoadCallBack(prefabName, prefab, null);
	}

	private void OnLoadCallBack(string resname, object obj, LoadderData data)
	{
		if (prefab == null)
		{
			if (obj == null) return;
			prefab = (GameObject) obj;
		}
		var j = spawnCount;
		for (var i = 0; i < j; i++)
		{
			var o = GameObject.Instantiate(prefab);
			o.transform.parent = parentRoot;
			var cpn = o.AddComponent<PoolItem>();
			cpn.Init(resname);
			poolItemList.Add(cpn);
		}
		isInitEnd = true;
	}

	public GameObject GetSpwan()
	{
		if (poolItemList.Count == 0)
		{
			if (prefab == null) return null;
			var o = GameObject.Instantiate(prefab);
			o.transform.parent = parentRoot;
			var cpn = o.AddComponent<PoolItem>();
			cpn.Init(prefabName, true);
			return o;
		}
		else
		{
			var cpn = poolItemList[0];
			poolItemList.RemoveAt(0);
			cpn.Spawn();
			return cpn.gameObject;
		}
	}

	public bool HasSpawn()
	{
		return poolItemList.Count > 0;
	}

	public bool IsInitEnd()
	{
		return isInitEnd;
	}

	public void Recycle(PoolItem poolItem)
	{
		poolItem.transform.parent = parentRoot; 
		poolItemList.Add(poolItem);
	}
}