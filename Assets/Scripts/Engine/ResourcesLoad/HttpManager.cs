using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Net;
using System.Text;

public class HttpManager : Singleton<HttpManager>
{
	private class HttpCallBack
	{
		private HttpHelperCallBack mHttpHelperCallBack;
		private object data;
		private HttpHelperRet ret;

		public HttpCallBack(HttpHelperCallBack mHttpHelperCallBack, object data = null, HttpHelperRet ret = null)
		{
			this.mHttpHelperCallBack = mHttpHelperCallBack;
			this.data = data;
			this.ret = ret;
		}

		public void Invoke()
		{
			if (mHttpHelperCallBack != null)
				mHttpHelperCallBack.Invoke(ret, data);
		}

		public void SetHttpHelperRet(HttpHelperRet ret)
		{
			this.ret = ret;
		}
	}

	private class HttpDownLoad
	{
		private HttpHelper httpHelper;
		private string url;
		private string filePath;

		public HttpDownLoad(HttpHelper httpHelper, string url, string filePath)
		{
			this.httpHelper = httpHelper;
			this.url = url;
			this.filePath = filePath;
		}

		public void Invoke()
		{
			if (httpHelper != null)
				httpHelper.AsyDownLoad(url, filePath);
		}

		public string GetDownFilePath()
		{
			return filePath;
		}

		public long GetCurSize()
		{
			return httpHelper == null ? 0 : httpHelper.m_curSize;
		}

		public long GetSize()
		{
			return httpHelper == null ? 0 : httpHelper.m_size;
		}
	}

	private class HttpUpLoad
	{
		private HttpHelper httpHelper;
		private string url;
		private string filepath;
		private byte[] bytes;
		private string userId;

		public HttpUpLoad(HttpHelper httpHelper, string url, byte[] bytes, string userId)
		{
			this.httpHelper = httpHelper;
			this.url = url;
			this.filepath = null;
			this.bytes = bytes;
			this.userId = userId;
		}

		public HttpUpLoad(HttpHelper httpHelper, string url, string filepath, string userId)
		{
			this.httpHelper = httpHelper;
			this.url = url;
			this.filepath = filepath;
			this.bytes = null;
			this.userId = userId;
		}

		public void Invoke()
		{
			if (httpHelper == null) return;
			if (bytes != null)
				httpHelper.AsyUpLoad(url, bytes, userId);
			else
				httpHelper.AsyUpLoad(url, filepath, userId);
		}
	}

	private List<HttpCallBack> HttpCallBackList = new List<HttpCallBack>();
	private List<HttpDownLoad> HttpDownLoadList = new List<HttpDownLoad>();
	private List<HttpUpLoad> HttpUpLoadList = new List<HttpUpLoad>();
	private bool downLoad = false;
	private bool upLoad = false;
	private HttpDownLoad curDownLoad = null;
	private HttpUpLoad curUpLoad = null;

	public void Init()
	{
		HttpCallBackList = new List<HttpCallBack>();
	}

	public void Update()
	{
		if (HttpCallBackList != null)
		{
			while (HttpCallBackList.Count > 0)
			{
				HttpCallBackList[0].Invoke();
				HttpCallBackList.RemoveAt(0);
			}
		}

		if (HttpDownLoadList != null)
		{
			while (HttpDownLoadList.Count > 0 && !downLoad)
			{
				downLoad = true;
				curDownLoad = HttpDownLoadList[0];
				HttpDownLoadList.RemoveAt(0);
				curDownLoad.Invoke();
			}
		}

		if (HttpUpLoadList != null)
		{
			while (HttpUpLoadList.Count > 0 && !upLoad)
			{
				upLoad = true;
				curUpLoad = HttpUpLoadList[0];
				HttpUpLoadList.RemoveAt(0);
				curUpLoad.Invoke();
			}
		}
	}

	public void DownLoad(string url, string filePath, HttpHelperCallBack callBack, object data = null)
	{
		if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(filePath))
		{
			if (callBack != null)
				callBack(new HttpHelperRet(false, "'url' or 'filePath' is null"), data);
			return;
		}
		HttpCallBack cb = new HttpCallBack(callBack, data);
		HttpHelper httpHelper = new HttpHelper(OnDownLoadCallBack, cb);
		HttpDownLoad dl = new HttpDownLoad(httpHelper, url, filePath);
		HttpDownLoadList.Add(dl);
	}

	private void OnDownLoadCallBack(HttpHelperRet ret, object data)
	{
		downLoad = false;
		var callBack = data as HttpCallBack;
		if (callBack != null)
		{
			callBack.SetHttpHelperRet(ret);
			HttpCallBackList.Add(callBack);
		}
	}

	public bool DownLoadEnd()
	{
		return HttpDownLoadList.Count == 0 && !downLoad;
	}

	public void UpLoad(string url, byte[] bytes, HttpHelperCallBack callBack, object data = null, string userId = "")
	{
		if (bytes == null || string.IsNullOrEmpty(url))
		{
			if (callBack != null) callBack(new HttpHelperRet(false, "'url' or 'bytes' is null"), data);
			return;
		}

		HttpCallBack cb = new HttpCallBack(callBack, data);
		HttpHelper httpHelper = new HttpHelper(OnUpLoadCallBack, cb);
		HttpUpLoad ul = new HttpUpLoad(httpHelper, url, bytes, userId);
		HttpUpLoadList.Add(ul);
	}

	public void UpLoad(string url, string filePath, HttpHelperCallBack callBack, object data = null, string userId = "")
	{
		if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(url))
		{
			if (callBack != null) callBack(new HttpHelperRet(false, "'url' or 'filePath' is null"), data);
			return;
		}

		HttpCallBack cb = new HttpCallBack(callBack, data);
		HttpHelper httpHelper = new HttpHelper(OnUpLoadCallBack, cb);
		HttpUpLoad ul = new HttpUpLoad(httpHelper, url, filePath, userId);
		HttpUpLoadList.Add(ul);
	}

	private void OnUpLoadCallBack(HttpHelperRet ret, object data)
	{
		upLoad = false;
		var callBack = data as HttpCallBack;
		if (callBack != null)
		{
			callBack.SetHttpHelperRet(ret);
			HttpCallBackList.Add(callBack);
		}
	}

	public bool UpLoadEnd()
	{
		return HttpUpLoadList.Count == 0 && !upLoad;
	}

	public string GetDownLoadInfo()
	{
		if (curDownLoad == null || DownLoadEnd()) return "";
		return string.Format("当前下载(进度:{2:F0}KB/{3:F0}KB 剩余:{0}):{1}",
			HttpDownLoadList.Count+1,
			curDownLoad.GetDownFilePath().Replace(ResManager.LocalAssetbundlePath,""),
			curDownLoad.GetCurSize()/1024f,
			curDownLoad.GetSize()/1024f
			);
	}

	public int GetDownLoadListCount()
	{
		return HttpDownLoadList.Count + 1;
	}
}

public class HttpHelperRet
{
	public bool isOk;
	public string log;

	public HttpHelperRet(bool isOk, string log)
	{
		this.isOk = isOk;
		this.log = log;
		if (log.Contains("<br />"))
			this.isOk = false;
		//if (GMManager.IsInEditor) Debug.LogError(log);
	}
}

public class WebState
{
	private const int BufferSize = 1024;
	private readonly byte[] mBuffer = new byte[BufferSize];
	private readonly FileStream mFileStream = null;
	private MemoryStream mMemoryStream = null;
	private Stream mStream = null;
	private HttpWebResponse mHttpWebResponse = null;
	private HttpWebRequest mHttpWebRequest = null;

	public WebState(string path = null, FileMode fileMode = FileMode.Create, FileAccess fileAccess = FileAccess.ReadWrite)
	{
		if (string.IsNullOrEmpty(path)) return;
		try
		{
			if (fileMode == FileMode.Create)
			{
				var fi = new FileInfo(path);
				if (fi.Exists) fi.Delete();
				var di = fi.Directory;
				if (di != null && !di.Exists) di.Create();
			}
			mFileStream = new FileStream(path, fileMode, fileAccess);
		}
		catch (Exception e)
		{
			Debug.LogError(string.Format("in new WebState()\npath: {0}\ne: {1}", path, e));
		}
	}

	public void Close()
	{
		if (mFileStream != null) mFileStream.Close();
		if (mMemoryStream != null) mMemoryStream.Close();
		if (mStream != null) mStream.Close();
		if (mHttpWebResponse != null) mHttpWebResponse.Close();
		if (mHttpWebRequest != null) mHttpWebRequest.Abort();
	}

	public void WriteFile(int count)
	{
		mFileStream.Write(mBuffer, 0, count);
	}

	public void FlushFile()
	{
		mFileStream.Flush();
	}

	public void WriteMemory(byte[] datas, int length)
	{
		if (mMemoryStream == null)
			mMemoryStream = new MemoryStream(datas, 0, length, false);
		else
			mMemoryStream.Write(datas, 0, length);
	}

	public int ReadMemory()
	{
		return mMemoryStream.Read(mBuffer, 0, BufferSize);
	}

	public MemoryStream GetMemory()
	{
		return mMemoryStream;
	}

	public long GetMemorySize()
	{
		return mMemoryStream.Length;
	}

	public void SetStream(bool isDownLoad/*Stream stream*/)
	{
		mStream = isDownLoad ? mHttpWebResponse.GetResponseStream() : mHttpWebRequest.GetRequestStream(); // stream;
	}

	public Stream GetStream()
	{
		return mStream;
	}

	public void StreamBeginRead(Action<IAsyncResult> readDataCallback)
	{
		mStream.BeginRead(mBuffer, 0, BufferSize, new AsyncCallback(readDataCallback), this);
	}

	public int StreamEndRead(IAsyncResult ar)
	{
		return mStream.EndRead(ar);
	}

	public void StreamBeginWrite(int length, Action<IAsyncResult> upLoadWriteDataCallback)
	{
		mStream.BeginWrite(mBuffer, 0, length, new AsyncCallback(upLoadWriteDataCallback), this);
	}

	public void CreateRequest(string url)
	{
		mHttpWebRequest = WebRequest.Create(url) as HttpWebRequest;
	}

	public void SetRequestInfo(string Method, bool AllowWriteStreamBuffering, int Timeout, string ContentType, bool KeepAlive, long ContentLength)
	{
		mHttpWebRequest.Method = Method;
		mHttpWebRequest.AllowWriteStreamBuffering = AllowWriteStreamBuffering;
		mHttpWebRequest.Timeout = Timeout;
		mHttpWebRequest.ContentType = ContentType;
		mHttpWebRequest.KeepAlive = KeepAlive;
		mHttpWebRequest.ContentLength = ContentLength;
	}

	public void BeginGetResponse(Action<IAsyncResult> responseCallback)
	{
		mHttpWebRequest.BeginGetResponse(new AsyncCallback(responseCallback), this);
	}

	public void EndGetResponse(IAsyncResult ar)
	{
		mHttpWebResponse = mHttpWebRequest.EndGetResponse(ar) as HttpWebResponse;
	}

	public void GetResponse()
	{
		mHttpWebResponse = mHttpWebRequest.GetResponse() as HttpWebResponse;
	}

	public HttpStatusCode ResponseStatusCode()
	{
		return mHttpWebResponse.StatusCode;
	}

	public long ResponseContentLength()
	{
		return mHttpWebResponse.ContentLength;
	}

	public void SetBuffer(byte[] bytes)
	{
		bytes.CopyTo(mBuffer, 0);
	}

	public byte[] GetBuffer()
	{
		return mBuffer;
	}

	public string SaveUrl()
	{
		if (mFileStream != null)
			return mFileStream.Name;
		return "";
	}

	public string GetRequestUrl()
	{
		return mHttpWebRequest.Address.AbsoluteUri;
	}
}

public delegate void HttpHelperCallBack(HttpHelperRet ret, object data);

public class HttpHelper
{
	#region 字段
	public long m_size;
	public long m_curSize;
	private HttpHelperCallBack m_callBack;
	private object data;

	private const string headerTemplate =
		"--{0}\r\n" + "Content-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\n" +
		"Content-Type: application/octet-stream\r\n\r\n";

	private const string DownLoadErrFormat = "DL Utl: {0}\nDL Msg: {1}\nDL State: {2}";
	private const string UpLoadErrFormat = "UL Utl: {0}\nUL Msg: {1}\nUL State: {2}";
	private string strBoundary = "";
	private byte[] boundaryBytes = null;
	#endregion

	public HttpHelper(HttpHelperCallBack m_callBack = null, object data = null)
	{
		this.m_callBack = m_callBack;
		this.data = data;
		m_size = 0;
		m_curSize = 0;
	}

	public void HttpHelperErr(string log, object data)
	{
		if (m_callBack != null) m_callBack(new HttpHelperRet(false, log), data);
	}

	#region 下载
	public void AsyDownLoad(string url, string saveFilePath)
	{
		try
		{
			WebState webState = new WebState(saveFilePath);
			webState.CreateRequest(url);
			webState.BeginGetResponse(ResponseCallback);
		}
		catch (WebException e)
		{
			HttpHelperErr(string.Format(DownLoadErrFormat, url, e.Message, e.Status), data);
		}
	}

	void ResponseCallback(IAsyncResult ar)
	{
		var webState = ar.AsyncState as WebState;
		if (webState == null)
		{
			HttpHelperErr("DL ResponseCallback 'webState' is not", data);
			return;
		}

		try
		{
			webState.EndGetResponse(ar);
		}
		catch (WebException e)
		{
			HttpHelperErr(string.Format(DownLoadErrFormat, webState.GetRequestUrl(), e.Message, e.Status), data);
		}
		if (webState.ResponseStatusCode() != HttpStatusCode.OK)
		{
			HttpHelperErr("DL ResponseCallback ResponseStatusCode is " + webState.ResponseStatusCode(), data);
			webState.Close();
			return;
		}

		m_size = webState.ResponseContentLength();
		try
		{
			webState.SetStream(true);
			webState.StreamBeginRead(ReadDataCallback);
		}
		catch (WebException e)
		{
			HttpHelperErr(string.Format(DownLoadErrFormat, webState.GetRequestUrl(), e.Message, e.Status), data);
		}
	}

	void ReadDataCallback(IAsyncResult ar)
	{
		var webState = ar.AsyncState as WebState;
		if (webState == null)
		{
			HttpHelperErr("DL ReadDataCallback 'webState' is not", data);
			return;
		}

		try
		{
			var read = webState.StreamEndRead(ar);
			m_curSize += read;
			if (read > 0)
			{
				webState.WriteFile(read);
				webState.FlushFile();
				webState.StreamBeginRead(ReadDataCallback);
			}
			else
			{
				if (m_callBack != null) m_callBack(new HttpHelperRet(true, webState.SaveUrl()), data);
				webState.Close();
			}
		}
		catch (WebException e)
		{
			HttpHelperErr(string.Format(DownLoadErrFormat, webState.GetRequestUrl(), e.Message, e.Status), data);
		}
	}
	#endregion

	#region 上传
	public void AsyUpLoad(string url, string filePath, string userId)
	{
		try
		{
			FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
			BinaryReader br = new BinaryReader(fs);
			var filebyte = new byte[fs.Length];
			filebyte = br.ReadBytes(filebyte.Length);
			fs.Close();
			fs = null;
			br.Close();
			br = null;
			AsyUpLoad(url, filebyte, userId);
		}
		catch (Exception e)
		{
			HttpHelperErr(string.Format("UL AsyUpLoad Url:{0}\nMsg:{1}", url, e.Message), data);
		}
	}

	public void AsyUpLoad(string url, byte[] bytes, string userId)
	{
		strBoundary = "----------" + DateTime.Now.Ticks.ToString("x");
		boundaryBytes = Encoding.ASCII.GetBytes("\r\n--" + strBoundary + "\r\n");

		var webState = new WebState();
		try
		{
			var strPostHeader = string.Format(headerTemplate, strBoundary, "file", "temp");
			var postHeaderBytes = Encoding.UTF8.GetBytes(strPostHeader);

			webState.WriteMemory(bytes, bytes.Length);
			webState.CreateRequest(url + "?file_path=" + userId);
			webState.SetRequestInfo("POST", false, 300000, "multipart/form-data; boundary=" + strBoundary, true, webState.GetMemorySize() + postHeaderBytes.Length + boundaryBytes.Length);

			m_size = webState.GetMemorySize();
			m_curSize = 0;

			webState.SetStream(false);
			webState.SetBuffer(postHeaderBytes);
			webState.StreamBeginWrite(postHeaderBytes.Length, UpLoadWriteDataCallback);
		}
		catch (WebException e)
		{
			HttpHelperErr(string.Format(UpLoadErrFormat, webState.GetRequestUrl(), e.Message, e.Status), data);
		}
	}

	private void UpLoadWriteDataCallback(IAsyncResult ar)
	{
		var webState = ar.AsyncState as WebState;
		if (webState == null)
		{
			HttpHelperErr("UL UpLoadWriteDataCallback 'webState' is not", data);
			return;
		}

		try
		{
			var size = webState.ReadMemory();
			if (size != 0)
			{
				m_curSize += size;
				webState.StreamBeginWrite(size, UpLoadWriteDataCallback);
			}
			else
			{
				webState.SetBuffer(boundaryBytes);
				webState.StreamBeginWrite(boundaryBytes.Length, UpLoadWriteBoundaryCallback);
			}
		}
		catch (WebException e)
		{
			HttpHelperErr(string.Format(UpLoadErrFormat, webState.GetRequestUrl(), e.Message, e.Status), data);
		}
	}

	private void UpLoadWriteBoundaryCallback(IAsyncResult ar)
	{
		var webState = ar.AsyncState as WebState;
		if (webState == null)
		{
			HttpHelperErr("UL UpLoadWriteDataCallback 'webState' is not", data);
			return;
		}

		try
		{
			webState.GetResponse();
			webState.SetStream(true);
			StreamReader sr = new StreamReader(webState.GetStream());
			var log = sr.ReadToEnd();
			sr.Close();
			webState.Close();
			if (m_callBack != null) m_callBack(new HttpHelperRet(true, log), data);
		}
		catch (WebException e)
		{
			HttpHelperErr(string.Format(UpLoadErrFormat, webState.GetRequestUrl(), e.Message, e.Status), data);
		}
	}
	#endregion
}