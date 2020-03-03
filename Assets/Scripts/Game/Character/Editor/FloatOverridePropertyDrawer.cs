using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(FloatOverride))]
public class FloatOverridePropertyDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		float x = position.x;
		float y = position.y;
		float w = position.width;
		float h = position.height;
		float widthLabel = EditorGUIUtility.labelWidth;
		float widthEnum = (position.width - widthLabel) * (2.0f / 3.0f);
		float spacing = 2;
		float arbitraryOffset = 15; // to counter the fact that an empty label (from GUIContent.none) still has a bit of width...

		Rect rectLabel = new Rect
		(
			x,
			y,
			widthLabel,
			h
		);
		Rect rectEnum = new Rect
		(
			x + widthLabel - arbitraryOffset,
			y,
			widthEnum + arbitraryOffset,
			h
		);
		Rect rectObject = new Rect
		(
			x + widthLabel + widthEnum + spacing - arbitraryOffset,
			y,
			w - widthLabel - (widthEnum + spacing) + arbitraryOffset,
			h
		);

		EditorGUI.LabelField(rectLabel, label);
		EditorGUI.PropertyField(rectEnum, property.FindPropertyRelative("State"), GUIContent.none);
		GUI.enabled = (ECharacterOverrideState)property.FindPropertyRelative("State").enumValueIndex == ECharacterOverrideState.Enable;
		EditorGUI.PropertyField(rectObject, property.FindPropertyRelative("Value"), GUIContent.none);
		GUI.enabled = true;
	}
}
