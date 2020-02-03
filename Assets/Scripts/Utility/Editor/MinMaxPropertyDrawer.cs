using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(MinMaxInt))]
public class MinMaxIntPropertyDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		float halfWidth = position.width / 2.0f;

		Rect minRect = new Rect(position.x, position.y, halfWidth, position.height);
		Rect maxRect = new Rect(position.x + halfWidth, position.y, halfWidth, position.height);

		EditorGUI.PropertyField(minRect, property.FindPropertyRelative("Min"));
		EditorGUI.PropertyField(maxRect, property.FindPropertyRelative("Max"));
	}
}

[CustomPropertyDrawer(typeof(MinMaxFloat))]
public class MinMaxFloatPropertyDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		float mainLabelWidth = EditorGUIUtility.labelWidth;
		float remainingWidth = position.width - mainLabelWidth;
		float halfWidth = remainingWidth / 2.0f;
		float minLabelWidth = 30.0f;
		float maxLabelWidth = 30.0f;
		float minPropertyWidth = halfWidth - minLabelWidth;
		float maxPropertyWidth = halfWidth - maxLabelWidth;

		Rect mainLabelRect = new Rect(position.x, position.y, mainLabelWidth, position.height);
		Rect minLabelRect = new Rect(position.x + mainLabelWidth, position.y, minLabelWidth, position.height);
		Rect maxLabelRect = new Rect(position.x + mainLabelWidth + halfWidth, position.y, maxLabelWidth, position.height);
		Rect minPropertyRect = new Rect(position.x + mainLabelWidth + minLabelWidth, position.y, minPropertyWidth, position.height);
		Rect maxPropertyRect = new Rect(position.x + mainLabelWidth + halfWidth + maxLabelWidth, position.y, maxPropertyWidth, position.height);

		GUI.Label(mainLabelRect, label);
		GUI.Label(minLabelRect, new GUIContent("Min"));
		GUI.Label(maxLabelRect, new GUIContent("Max"));
		EditorGUI.PropertyField(minPropertyRect, property.FindPropertyRelative("Min"), GUIContent.none);
		EditorGUI.PropertyField(maxPropertyRect, property.FindPropertyRelative("Max"), GUIContent.none);
	}
}
