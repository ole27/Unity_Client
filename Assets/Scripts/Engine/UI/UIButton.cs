using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIButton : Button
{
	[SerializeField]
	[FormerlySerializedAs("m_Label")]
	protected Text m_Label = null;

	public string ButtonLabel
	{
		get { return m_Label == null ? "" : m_Label.text; }
		set { if(m_Label != null) m_Label.text = value; }
	}
}
