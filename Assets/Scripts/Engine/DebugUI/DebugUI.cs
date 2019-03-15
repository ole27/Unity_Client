using System;
using UnityEngine;
using System.Collections.Generic;

public class DebugUI : MonoBehaviour
{
	public bool debugMode = true;

	public static DebugUI Instance = null;

	private class DebugLog
	{
		public string log;
		public LogType type;

		public DebugLog(string log, LogType type)
		{
			this.log = log;
			this.type = type;
		}
	}

	private readonly List<DebugLog> logList = new List<DebugLog>();

	private readonly GUILayoutOption[] options = new GUILayoutOption[]
	{
		GUILayout.Width(Screen.width),
		GUILayout.Height(Screen.height),
	};

	private Vector2 scrollPosition;
	private string gmCommand = "";
	private bool display = false;
	private bool showFps = false;

	private readonly Dictionary<LogType, Color> logColors = new Dictionary<LogType, Color>()
	{
		{ LogType.Assert, Color.cyan },
		{ LogType.Error, Color.red },
		{ LogType.Exception, Color.magenta },
		{ LogType.Log, Color.blue },
		{ LogType.Warning, Color.yellow },
	};

	void OnEnable()
	{
		if(!debugMode) return;
#if UNITY_5
		Application.logMessageReceived += LogReceived;
#else
		Application.RegisterLogCallback(HandleLog);
#endif
	}

	void OnDisable()
	{
		if (!debugMode) return;
#if UNITY_5
		Application.logMessageReceived -= LogReceived;
#else
		Application.RegisterLogCallback(null);
#endif
	}

	void Awake()
	{
		Instance = this;
	}

	void Update()
	{
		if (!debugMode) return;
		if (showFps) FrameRateCalculation();
		if (!display) CheckTouchs();
		if (display) ScrollViewTouch();
	}

	void OnGUI()
	{
		if (!debugMode) return;
		if (display) DrawDebugUI();
		if (showFps) DrawFpsUI();
	}

	private float _fps;
	private float _timeleft;
	private float _accum;
	private int _frames;
	private readonly float updateInterval = 0.5f;
	private void FrameRateCalculation()
	{
		_timeleft -= Time.deltaTime;
		_accum += Time.timeScale / Time.deltaTime;
		++_frames;
		if (_timeleft <= 0.0f)
		{
			_timeleft = updateInterval;
			_accum = 0.0f;
			_frames = 0;
		}
		else
		{
			_fps = _accum / _frames;
		}
	}

	private void CheckTouchs()
	{
#if UNITY_EDITOR || UNITY_EDITOR_64 || UNITY_STANDALONE_WIN || UNITY_STANDALONE || UNITY_EDITOR_WIN
		if (Input.GetMouseButtonDown(2) && !display)
#else
		if (Input.touchCount >= 5 && !display)
#endif
			display = true;
	}

	private Vector2 _lastDeltaPos;
	private int _scrollVelocity;
	private float _timeTouchPhaseEnded;
	private readonly float inertiaDuration = 0.1f;
	private void ScrollViewTouch()
	{
		if (Input.touchCount > 0)
		{
			var state = Input.GetTouch(0).phase;
			switch (state)
			{
				case TouchPhase.Moved:
					_lastDeltaPos = Input.GetTouch(0).deltaPosition*10;
					scrollPosition.y += _lastDeltaPos.y;
					break;
				case TouchPhase.Ended:
					if (Mathf.Abs(_lastDeltaPos.y) > 20.0f)
					{
						_scrollVelocity = (int) (_lastDeltaPos.y*0.5/Input.GetTouch(0).deltaTime);
					}
					_timeTouchPhaseEnded = Time.time;
					break;
			}
		}
		else
		{
			if (_scrollVelocity == 0) return;
			var t = (Time.time - _timeTouchPhaseEnded)/inertiaDuration;
			var frameVelocity = Mathf.Lerp(_scrollVelocity, 0, t);
			scrollPosition.y += frameVelocity*Time.deltaTime;
			if (t >= inertiaDuration)
				_scrollVelocity = 0;
		}
	}

	private int itemHeight = 0;
	private void DrawDebugUI()
	{
		InitGUISetting();

		GUILayout.BeginVertical(options);
		scrollPosition = GUILayout.BeginScrollView(scrollPosition);
		for (var i = 0; i < logList.Count; i++)
		{
			var log = logList[i];
			GUI.contentColor = logColors[log.type];
			GUILayout.Label(log.log);
		}
		GUILayout.EndScrollView();
		GUI.contentColor = Color.white;

		gmCommand = GUILayout.TextField(gmCommand, GUILayout.Height(itemHeight));
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Enter", GUILayout.Height(itemHeight)))
		{
			GameMasterCommand(gmCommand);
			gmCommand = "";
		}
		if (GUILayout.Button("Clear", GUILayout.Height(itemHeight)))
		{
			ClearDebugLog();
		}
		if (GUILayout.Button("Close", GUILayout.Height(itemHeight)))
		{
			CloseDebugUI();
		}
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
	}

	private readonly Rect _fpsLabelRect = new Rect(0, 0, 1000, 200);
	private readonly string _fpsLabelTextFormat = "FPS: {0}";
	private void DrawFpsUI()
	{
		GUI.contentColor = Color.red;
		GUI.Label(_fpsLabelRect, string.Format(_fpsLabelTextFormat, _fps));
	}

	private bool init = false;
	private void InitGUISetting()
	{
		if (init) return;

		itemHeight = Screen.height/10;

		var style = GUI.skin.label;
		style.fontSize = itemHeight/3;

		style = GUI.skin.textField;
		style.fontSize = itemHeight - 10;

		init = true;
	}

	private void CloseDebugUI()
	{
		display = false;
	}

	private void ClearDebugLog()
	{
		logList.Clear();
	}

	private void GameMasterCommand(string s)
	{
		if (string.IsNullOrEmpty(s)) return;
		var args = s.Split(',');
		GMManager.Instance.Execute(args);
	}

	private const string logHead = "--------------{0}--------------\n";
	private const string logCrLf = "\n";
	private const string logEnd = "-------------------------------\n";
	private void LogReceived(string condition, string stacktrace, LogType type)
	{
		var log = string.Format(logHead, type);
		log += condition + logCrLf;
		if (type != LogType.Log && type != LogType.Warning)
			log += stacktrace;
		log += logEnd;
		logList.Add(new DebugLog(log, type));
	}

	public void StopDebudMode(string[] args = null)
	{
		logList.Clear();
		display = false;
		showFps = false;
		debugMode = false;
	}

	public void StartFps(string[] args = null)
	{
		showFps = true;
	}
		
	public void StopFps(string[] args = null)
	{
		showFps = false;
	}

	public void ClosGame(string[] args)
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
	}
}
