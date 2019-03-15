using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class AssetBundleMotify
{
	public string name;
	public bool add;

	public AssetBundleMotify(string name, bool add)
	{
		this.name = name;
		this.add = add;
	}
}

public class LoadderData
{
	public string assetBoudleName = "";
}

public delegate void LoadderCallBack(string resName, object obj, LoadderData data);
public delegate void LoadderCallBack<in T>(string resName, T obj, LoadderData data);

public class ResManager : Singleton<ResManager>
{
	public enum ResManagerState
	{
		None,
		EDownABManifest,	//
		ECheckResVersion,	//
		EUpdateAB,
		EAsyncLoadRes,
		EError,				//
		EFree,				//
	}

	public bool UseAssetBoudle = true;
	public bool UpdateAssetBoudleVesion = true;

	private ResManagerState resManagerState = ResManagerState.EFree;

	private static string _serverAssetbundleRelPath = null;
	public static string ServerAssetbundleRelPath
	{
		get
		{
			if(_serverAssetbundleRelPath == null)
#if UNITY_STANDALONE_WIN
				_serverAssetbundleRelPath = "builds_win/AssetBundles/"; ;
#elif UNITY_ANDROID
				_serverAssetbundleRelPath = "builds_android/AssetBundles/";
#elif UNITY_IOS
				_serverAssetbundleRelPath = "builds_ios/AssetBundles/";
#endif
			return _serverAssetbundleRelPath;
		}
	}

	private static string _serverAssetbundleAbsPath;
	public static string ServerAssetbundleAbsPath
	{
		get
		{
			if (_serverAssetbundleAbsPath==null)
				_serverAssetbundleAbsPath = Game.server_root + ServerAssetbundleRelPath;
			return _serverAssetbundleAbsPath;
		}
	}

	private static string _localAssetbundlePath = null;
	public static string LocalAssetbundlePath
	{
		get
		{
			if(_localAssetbundlePath == null)
#if UNITY_STANDALONE_WIN || UNITY_EDITOR || UNITY_EDITOR_64 || UNITY_EDITOR_WIN || UNITY_STANDALONE
				_localAssetbundlePath = string.Format("{0}/../builds/AssetBundles/", Application.dataPath);
#elif UNITY_ANDROID
				_localAssetbundlePath = string.Format("{0}/../builds/AssetBundles/", Application.persistentDataPath);
#elif UNITY_IOS
				_localAssetbundlePath = string.Format("{0}/../builds/AssetBundles/", Application.persistentDataPath);
#endif
			return _localAssetbundlePath;
		} 
	}

	private AssetBundleManifest _manifest;

	private readonly Dictionary<string, AssetBundle> _objDic = new Dictionary<string, AssetBundle>();
	private readonly List<string> _wwwList = new List<string>();

	private bool init = false;

	public void Init(bool UseAssetBoudle = true, bool UpdateAssetBoudleVesion = true)
	{
		this.UseAssetBoudle = UseAssetBoudle;
		this.UpdateAssetBoudleVesion = UpdateAssetBoudleVesion;
		init = !UseAssetBoudle;
		if (!UseAssetBoudle) return;
		var sabServerPath = string.Format("{0}{1}", ServerAssetbundleAbsPath, "AssetBundles");
		var sabLocalPath = string.Format("{0}{1}", LocalAssetbundlePath, "AssetBundles");

		if (UpdateAssetBoudleVesion)
		{
			var fileInfo = new FileInfo(sabLocalPath);
			if(fileInfo.Exists) fileInfo.Delete();
		}

		resManagerState = ResManagerState.EDownABManifest;
		if (UpdateAssetBoudleVesion)
			HttpManager.Instance.DownLoad(sabServerPath, sabLocalPath, OnDownLoadSAbCallBack);
		else
			OnDownLoadSAbCallBack(new HttpHelperRet(true, ""), null);
	}

	private void OnDownLoadSAbCallBack(HttpHelperRet ret, object data)
	{
		if (ret.isOk)
		{
			Game.Instance.StartCoroutine(CheckResVersion());
		}
		else
		{
			resManagerState = ResManagerState.EError;
			init = true;
			if (GMManager.IsInEditor) Debug.LogError(string.Format("{0}\nlog: {1}", ret.isOk, ret.log));
		}
	}

	private IEnumerator CheckResVersion()
	{
		resManagerState = ResManagerState.ECheckResVersion;
		var _cabPath = string.Format("{0}{1}", LocalAssetbundlePath, "AssetBundles_old");
		var _sabPath = string.Format("{0}{1}", LocalAssetbundlePath, "AssetBundles");
		var _cabwwwPath = string.Format("file://{0}", _cabPath);
		var _sabwwwPath = string.Format("file://{0}", _sabPath);

		if (!UpdateAssetBoudleVesion)
		{
			var fileInfo = new FileInfo(_sabPath);
			if (fileInfo.Exists)
				fileInfo.CopyTo(_cabPath, true);
			else
			{
				init = true;
				if (GMManager.IsInEditor) Debug.LogError(string.Format("load '{0}' err\nlog: '{0}' no here !", _sabPath));
				yield break;
			}
		}

		AssetBundleManifest cabVersion = null;
		var updateList = new List<AssetBundleMotify>();

		var www = new WWW(_sabwwwPath);
		yield return www;
		if (!string.IsNullOrEmpty(www.error))
		{
			init = true;
			if (GMManager.IsInEditor) Debug.LogError(string.Format("load '{0}' err\nlog: {1}", _sabwwwPath, www.error));
			yield break;
		}

		_manifest = www.assetBundle.LoadAsset("AssetBundleManifest") as AssetBundleManifest;
		if (_manifest == null)
		{
			Debug.LogError("_manifest null");
			yield break;
		}
		var sabBytes = www.bytes;
		www.assetBundle.Unload(false);
		www.Dispose();

		var cabJson = new FileInfo(_cabPath);
		if (cabJson.Exists)
		{
			www = new WWW(_cabwwwPath);
			yield return www;
			if (!string.IsNullOrEmpty(www.error))
			{
				init = true;
				if (GMManager.IsInEditor) Debug.LogError(string.Format("load '{0}' err\nlog: {1}", _cabwwwPath, www.error));
				yield break;
			}

			cabVersion = www.assetBundle.LoadAsset("AssetBundleManifest") as AssetBundleManifest;
			var cabBytes = www.bytes;
			www.assetBundle.Unload(false);
			www.Dispose();

			if (cabBytes.SequenceEqual(sabBytes))
			{
				resManagerState = ResManagerState.EFree;
				init = true;
				if (GMManager.IsInEditor) Debug.Log("The same version!");
				yield break;
			}
		}
		updateList.Clear();
		GetUpdateList(ref updateList, _manifest, cabVersion);

		if (GMManager.IsInEditor)
		{
			var log = "无更新资源!";
			if (updateList.Count > 0)
			{
				log = "需要更新以下资源:";
				foreach (var ab in updateList)
				{
					if (ab.add) log += "\n更新: " + ab.name;
					else log += "\n删除: " + ab.name;
				}
			}
			Debug.Log(log);
		}

		var delCount = 0;
		var downloadCount = 0;
		resManagerState = ResManagerState.EUpdateAB;

		while (updateList.Count > 0)
		{
			var item = updateList[0];
			updateList.RemoveAt(0);
			if (item.add)
			{
				if(string.IsNullOrEmpty(item.name)) continue;
				downloadCount++;
				var sabServerPath = string.Format("{0}{1}", ServerAssetbundleAbsPath, item.name);
				var sabLocalPath = string.Format("{0}{1}", LocalAssetbundlePath, item.name);
				var fileInfo = new FileInfo(sabLocalPath);
				var mydir = fileInfo.Directory;
				if (!mydir.Exists) mydir.Create();
				HttpManager.Instance.DownLoad(sabServerPath, sabLocalPath, OnDownLoadAbCallBack);
			}
			else
			{
				delCount++;
				var filePath = string.Format("{0}{1}", LocalAssetbundlePath, item.name);
				var fileInfo = new FileInfo(filePath);
				if (fileInfo.Exists) fileInfo.Delete();
			}
		}
		updateList = null;

		if (downloadCount == 0)
			OnDownLoadAbCallBack(new HttpHelperRet(true, ""), null);
	}

	private void GetUpdateList(ref List<AssetBundleMotify> updateList, AssetBundleManifest sabVersion, AssetBundleManifest cabVersion)
	{
		if (cabVersion == null)
		{
			updateList.AddRange(sabVersion.GetAllAssetBundles().Select(t => new AssetBundleMotify(t, true)));
			return;
		}
		
		var sabList = new List<string>(sabVersion.GetAllAssetBundles());
		var cabList = new List<string>(cabVersion.GetAllAssetBundles());

		foreach (var tmp in cabList)
		{
			var del = true;
			for (var j = 0; j < sabList.Count; j++)
			{
				if (!tmp.Equals(sabList[j])) continue;
				sabList.RemoveAt(j);
				break;
			}
			if (del) updateList.Add(new AssetBundleMotify(tmp, false));
		}

		sabList = new List<string>(sabVersion.GetAllAssetBundles());
		cabList = new List<string>(cabVersion.GetAllAssetBundles());

		foreach (var tmp in sabList)
		{
			var add = true;
			for (var j = 0; j < cabList.Count; j++)
			{
				if (!tmp.Equals(cabList[j])) continue;
				cabList.RemoveAt(j);
				var shash = sabVersion.GetAssetBundleHash(tmp);
				var chash = cabVersion.GetAssetBundleHash(tmp);
				if (!shash.Equals(chash)) continue;
				var filePath = string.Format("{0}{1}", LocalAssetbundlePath, tmp);
				var fileInfo = new FileInfo(filePath);
				if (fileInfo.Exists) add = false;
				break;
			}
			if (add) updateList.Add(new AssetBundleMotify(tmp, true));
		}
	}

	private void OnDownLoadAbCallBack(HttpHelperRet ret, object data)
	{
		if(GMManager.IsInEditor) Debug.Log("已下载: " + ret.isOk + " " + ret.log);
		if (!HttpManager.Instance.DownLoadEnd()) return;
		resManagerState = ResManagerState.EFree;
		var _cabPath = string.Format("{0}{1}", LocalAssetbundlePath, "AssetBundles_old");
		var _sabPath = string.Format("{0}{1}", LocalAssetbundlePath, "AssetBundles");
		var fileInfo = new FileInfo(_sabPath);
		fileInfo.CopyTo(_cabPath, true);
		init = true;
	}

	public bool InitEnd()
	{
		return init;
	}

	public void LoadResources(string resName, LoadderCallBack callBack, LoadderData data)
	{
		if (string.IsNullOrEmpty(resName))
		{
			callBack(null, null, data);
			return;
		}

		object obj = Resources.Load(resName);
		callBack(resName, obj, data);
	}

	public void LoadAsset(string resName, LoadderCallBack callBack, LoadderData data)
	{
		if (string.IsNullOrEmpty(resName))
		{
			callBack(null, null, data);
			return;
		}

		if (UseAssetBoudle)
			Game.Instance.StartCoroutine(IeLoadAsset(resName, callBack, data));
		else
		{
			object obj = Resources.Load(resName);
			callBack(resName, obj, data);
		}
	}

	private IEnumerator IeLoadAsset(string resName, LoadderCallBack callBack, LoadderData data)
	{
		var boudleName = data == null ? resName : string.IsNullOrEmpty(data.assetBoudleName) ? resName : data.assetBoudleName;

		if (_objDic.ContainsKey(boudleName))
		{
			var resNames = resName.Split('/');
			if (callBack != null) callBack(resName, _objDic[boudleName].LoadAsset(resNames[resNames.Length - 1]), data);
			yield break;
		}

		resManagerState = ResManagerState.EAsyncLoadRes;
		if (_wwwList.Contains(boudleName))
		{
			yield return new WaitWhile(() => _wwwList.Contains(boudleName));
			if (_objDic.ContainsKey(boudleName))
			{
				var resNames = resName.Split('/');
				if (callBack != null) callBack(resName, _objDic[boudleName].LoadAsset(resNames[resNames.Length - 1]), data);
				resManagerState = ResManagerState.EFree;
				yield break;
			}
		}

		_wwwList.Add(boudleName);

		yield return LoadAllDependencies(boudleName);

		var objwww = WWW.LoadFromCacheOrDownload(string.Format("file://{0}{1}.unity3d", ResManager.LocalAssetbundlePath, boudleName), _manifest.GetAssetBundleHash(string.Format("{0}.unity3d", boudleName)), 0);
		yield return objwww;
		if (!string.IsNullOrEmpty(objwww.error))
		{
			Debug.Log(objwww.error);
			if (callBack != null) callBack(resName, null, data);
		}
		else
		{
			_objDic.Add(boudleName, objwww.assetBundle);
			var resNames = resName.Split('/');
			var obj = objwww.assetBundle.LoadAsset(resNames[resNames.Length - 1]);
			if (callBack != null) callBack(resName, obj, data);
		}
		_wwwList.Remove(boudleName);
		resManagerState = ResManagerState.EFree;
	}

	private IEnumerator LoadAllDependencies(string resName)
	{
		if (_objDic.ContainsKey(resName))
		{
			if (GMManager.IsInEditor) Debug.Log("_objDic ContainsKey");
			yield break;
		}
		if (_manifest == null)
		{
			Debug.LogError("_manifest is null");
			yield break;
		}

		var dps = _manifest.GetAllDependencies(resName + ".unity3d");
		foreach (var dependency in dps)
		{
			var dependencyOnly = dependency.Replace(".unity3d", "");
			if (_objDic.ContainsKey(dependencyOnly)) continue;

			if (_wwwList.Contains(dependencyOnly))
			{
				yield return new WaitWhile(() => _wwwList.Contains(dependencyOnly));
				if (_objDic.ContainsKey(dependencyOnly))
					yield break;
			}

			_wwwList.Add(dependencyOnly);

			var dUrl = string.Format("file://{0}{1}", ResManager.LocalAssetbundlePath, dependency);
			var dwww = WWW.LoadFromCacheOrDownload(dUrl, _manifest.GetAssetBundleHash(dependency));
			yield return dwww;
			if (_objDic.ContainsKey(dependencyOnly)) continue;
			if (!string.IsNullOrEmpty(dwww.error))
			{
				Debug.LogError(dependency + " " + dwww.error);
			}
			else
			{
				if (dwww.assetBundle != null)
					_objDic.Add(dependencyOnly, dwww.assetBundle);
				else
					Debug.LogError(dependency + " ab is null");
			}
			_wwwList.Remove(dependencyOnly);
		}
	}

	public ResManagerState GetResManagerState()
	{
		return resManagerState;
	}

	public void Destroy(GameObject o = null)
	{
		if (o != null)
		{
			var cpns = o.GetComponentsInChildren<PoolItem>();
			if (cpns.Length != 0)
			{
				for (var i = 0; i < cpns.Length; i++)
				{
					cpns[i].Recycle();
				}
				return;
			}
			GameObject.Destroy(o);
		}
		Resources.UnloadUnusedAssets();
		GC.Collect();
	}
}
