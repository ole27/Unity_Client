using UnityEditor;
using UnityEditor.UI;
using UnityEngine.UI;

[CustomEditor(typeof(UIButton), true)]
[CanEditMultipleObjects]
public class UIButtonEditor : ButtonEditor
{
	SerializedProperty m_Label;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_Label = serializedObject.FindProperty("m_Label");
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		EditorGUILayout.LabelField("Add--------------------");
		EditorGUILayout.PropertyField(m_Label);
		EditorGUILayout.LabelField("-----------------------");
		serializedObject.ApplyModifiedProperties();
		base.OnInspectorGUI();

	}
}
