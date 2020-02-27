using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(WeightedObject))]
public class WeightedObjectPropertyDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		float x = position.x;
		float y = position.y;
		float w = position.width;
		float h = position.height;
		float labelWidth = EditorGUIUtility.labelWidth;
		float widthWeight = 40;
		float spacing = 2;
		float arbitraryOffset = 15;	// to counter the fact that an empty label (from GUIContent.none) still has a bit of width...

		Rect rectLabel = new Rect
		(
			x,
			y,
			labelWidth,
			h
		);
		Rect rectWeight = new Rect
		(
			x + labelWidth - arbitraryOffset,
			y,
			widthWeight + arbitraryOffset,
			h
		);
		Rect rectObject = new Rect
		(
			x + labelWidth + widthWeight + spacing - arbitraryOffset,
			y,
			w - labelWidth - (widthWeight + spacing) + arbitraryOffset,
			h
		);

		EditorGUI.LabelField(rectLabel, label);
		EditorGUI.PropertyField(rectWeight, property.FindPropertyRelative("Weight"), GUIContent.none);
		EditorGUI.PropertyField(rectObject, property.FindPropertyRelative("Object"), GUIContent.none);
	}
}
