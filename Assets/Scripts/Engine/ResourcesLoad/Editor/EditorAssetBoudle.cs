using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Object = UnityEngine.Object;

public class AssetBundleConfig
{
	public static string AssetbundlePath
	{
		get
		{
			return Application.dataPath + "/../" + ResManager.ServerAssetbundleRelPath;
		}
	}

	public static BuildTarget AssetbundleBuildTarget
	{
		get
		{
#if UNITY_STANDALONE_WIN
			return BuildTarget.StandaloneWindows;
#elif UNITY_ANDROID
			return BuildTarget.Android;
#elif UNITY_IOS
			return BuildTarget.iOS;
#endif
		}
	}
	// "..../../Assets/"
	private static string _projectPath;
	public static string ProjectPath
	{
		get
		{
			if (string.IsNullOrEmpty(_projectPath))
				_projectPath = new DirectoryInfo(Application.dataPath + "/../").FullName.Replace('\\', '/');
			return _projectPath;
		}
	}

	// "..../../Assets/Resources/"
	private static string _projectResourcespath;
	public static string ProjectResourcespath
	{
		get
		{
			if (string.IsNullOrEmpty(_projectResourcespath))
				_projectResourcespath = new DirectoryInfo(Application.dataPath + "/Resources/").FullName.Replace('\\', '/');
			return _projectResourcespath;
		}
	}

	public static string Suffix = ".unity3d";
}

public class EditorAssetBoudle : EditorWindow
{
	private delegate void EditorAssetBoudleCallBack();
	private class AssetBundleData
	{
		public AssetImporter importer = null;
		public string assetBundleName;

		public AssetBundleData(AssetImporter importer, string assetBundleName)
		{
			this.importer = importer;
			this.assetBundleName = assetBundleName;
		}
	}

	private static readonly string[] Filtersuffix = new string[] {".prefab", ".mat", ".png", ".tga", ".unity", ".exr", ".asset"};
	private static readonly List<AssetBundleData> FileList = new List<AssetBundleData>();
	private List<EditorAssetBoudleCallBack> callBack = new List<EditorAssetBoudleCallBack>();
	private Vector2 _scrollViewPostion;

	[MenuItem("AB Editor/AssetBundleEditor")]
	static void AssetBundleEditor()
	{
		var window = GetWindow(typeof (EditorAssetBoudle), true, "Editor AssetBundle Name");
		window.Show();
		window.depthBufferBits = 0;
	}

	void Awake()
	{
		callBack = new List<EditorAssetBoudleCallBack>();
		EditorUtility.ClearProgressBar();
		ReflashSelect();
	}

	void OnGUI()
	{
		GUILayout.BeginVertical();
		GUILayout.BeginHorizontal();
		ShowTopButton();
		GUILayout.EndHorizontal();
		_scrollViewPostion = GUILayout.BeginScrollView(_scrollViewPostion);
		ShowAssetBundleList();
		GUILayout.EndScrollView();
		GUILayout.BeginHorizontal();
		ShowOtherUI();
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
	}

	void Update()
	{
		if (callBack.Count > 0)
		{
			while (callBack.Count > 0)
			{
				callBack[0].Invoke();
				callBack.RemoveAt(0);
			}
		}
	}

	private bool delmanifest = true;
	private void ShowOtherUI()
	{
		GUI.contentColor = Color.white;
		delmanifest = GUILayout.Toggle(delmanifest, "删除.manifest文件");
	}

	private void ShowAssetBundleList()
	{
		for (var i = 0; i < FileList.Count; i++)
		{
			GUILayout.BeginHorizontal();
			GUI.contentColor = Color.white;
			if (GUILayout.Button("-",GUILayout.Width(20)))
			{
				FileList.RemoveAt(i);
				break;
			}
			EditorGUILayout.ObjectField(FileList[i].importer, typeof (AssetImporter),false,GUILayout.Width(40));
			GUILayout.BeginVertical(GUILayout.Width(40));
			if (GUILayout.Button("清除", GUILayout.Width(40)))
			{
				FileList[i].importer.assetBundleName = null;
				break;
			}
			if (GUILayout.Button("设置", GUILayout.Width(40)))
			{
				FileList[i].importer.assetBundleName = FileList[i].assetBundleName;
				break;
			}
			GUILayout.EndVertical();
			GUILayout.BeginVertical();
			GUILayout.Label(FileList[i].importer.assetBundleName);
			if (string.IsNullOrEmpty(FileList[i].importer.assetBundleName))
			{
				GUI.contentColor = Color.red;
			}
			else
			{
				var abObjName = FileList[i].importer.assetBundleName.ToLower();
				var abName = FileList[i].assetBundleName.ToLower();
				GUI.contentColor = abObjName != abName ? Color.red : Color.green;
			}
			FileList[i].assetBundleName = GUILayout.TextField(FileList[i].assetBundleName);
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
		}
	}

	private void ShowTopButton()
	{
		if (GUILayout.Button("刷新选择"))
		{
			callBack.Add(ReflashSelect);
		}
		if (GUILayout.Button("清除所有资源"))
		{
			FileList.Clear();
		}
		if (GUILayout.Button("清除所有资源名"))
		{
			callBack.Add(ClearAllAssetBundleName);
		}
		if (GUILayout.Button("设置所有资源名"))
		{
			callBack.Add(SetAllAssetBundleName);
		}
		if (GUILayout.Button("设置统一资源名"))
		{
			EditorAssetBoudleSub.AssetBundleEditor();
		}
		if (GUILayout.Button("打包资源"))
		{
			callBack.Add(CreateAssetBundle);
		}
	}

	private void ReflashSelect()
	{
		FileList.Clear();
		var selectedAsset = Selection.GetFiltered(typeof(Object), SelectionMode.Unfiltered);
		var files = new List<FileInfo>();
		foreach (var fullPath in selectedAsset.Select(item => AssetBundleConfig.ProjectPath + AssetDatabase.GetAssetPath(item)))
		{
			if (Directory.Exists(fullPath))
			{
				var dir = new DirectoryInfo(fullPath);
				files.AddRange(dir.GetFiles("*", SearchOption.AllDirectories));
			}
			else if (File.Exists(fullPath))
			{
				var fileInfo = new FileInfo(fullPath);
				files.Add(fileInfo);
			}
		}

		for (var i = 0; i < files.Count; ++i)
		{
			var fileInfo = files[i];
			foreach (var suffix in Filtersuffix)
			{
				var fileName = fileInfo.Name.ToLower();
				if (!fileName.EndsWith(suffix)) continue;
				var path = fileInfo.FullName.Replace('\\', '/');
				if (!path.Contains("Assets/Resources/")) continue;
				var resPath = path.Substring(AssetBundleConfig.ProjectPath.Length);
				var importer = AssetImporter.GetAtPath(resPath);
				if (!importer) continue;
				var abName = path.Substring(AssetBundleConfig.ProjectResourcespath.Length);
				FileList.Add(new AssetBundleData(importer, abName.Substring(0, abName.LastIndexOf('.')) + AssetBundleConfig.Suffix));
			}
		}
	}

	private void ClearAllAssetBundleName()
	{
		for (var i = 0; i < FileList.Count; ++i)
		{
			EditorUtility.DisplayProgressBar("清除名称", string.Format("清除:{0}", FileList[i].assetBundleName), 1f * i / FileList.Count);
			FileList[i].importer.assetBundleName = null;
		}
		AssetDatabase.RemoveUnusedAssetBundleNames();
		EditorUtility.ClearProgressBar();
	}

	private void SetAllAssetBundleName()
	{
		for (var i = 0; i < FileList.Count; ++i)
		{
			EditorUtility.DisplayProgressBar("设置名称", string.Format("设置:{0}", FileList[i].assetBundleName), 1f * i / FileList.Count);
			FileList[i].importer.assetBundleName = FileList[i].assetBundleName;
		}
		AssetDatabase.RemoveUnusedAssetBundleNames();
		EditorUtility.ClearProgressBar();
	}

	private void CreateAssetBundle()
	{
		FileList.Clear();
		var path = AssetBundleConfig.AssetbundlePath;
		var buildTarget = AssetBundleConfig.AssetbundleBuildTarget;
		var di = new DirectoryInfo(path);
		if (di.Exists) di.Delete(true);
		di.Create();
		var a = BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.ForceRebuildAssetBundle | BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.DeterministicAssetBundle, buildTarget);
		AssetDatabase.Refresh();

		if (delmanifest)
		{
			try
			{
				var names = a.GetAllAssetBundles();
				var nameList = new List<string>(names);
				for (var i = 0; i < nameList.Count; i++)
				{
					var name = nameList[i];
					var filePath = path + name + ".manifest";
					EditorUtility.DisplayProgressBar("删除.manifest", string.Format("删除:{0}", name + ".manifest"), 1f*i/nameList.Count);
					var fileInfo = new FileInfo(filePath);
					if (fileInfo.Exists) fileInfo.Delete();
				}
				AssetDatabase.RemoveUnusedAssetBundleNames();
				EditorUtility.ClearProgressBar();
			}
			catch (Exception e)
			{
				Debug.LogError(e);
			}
		}

	}

	public static void SetUnityAssetBundleName(string assetBundleName)
	{
		foreach (var data in FileList)
		{
			data.assetBundleName = assetBundleName;
		}
	}
}

public class EditorAssetBoudleSub : EditorWindow
{
	private string assetBundleName = "";
	public static void AssetBundleEditor()
	{
		var window = GetWindow(typeof(EditorAssetBoudleSub), true, "Input AssetBundle Name");
		window.ShowAuxWindow();
		window.Focus();
		window.maxSize = new Vector2(500,200);
	}

	void OnGUI()
	{
		GUILayout.BeginVertical();
		GUILayout.Space(10);
		GUILayout.Label("例如:'xx/xx/xx/xx' 注意不用加后缀");
		GUILayout.Space(1);
		assetBundleName = GUILayout.TextField(assetBundleName);
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("确定"))
		{
			EditorAssetBoudle.SetUnityAssetBundleName(assetBundleName + AssetBundleConfig.Suffix);
			this.Close();
		}
		if (GUILayout.Button("取消"))
		{
			this.Close();
		}
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
	}
}