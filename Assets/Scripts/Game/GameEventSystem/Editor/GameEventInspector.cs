using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(GameEvent))]
public class GameEventInspector : Editor
{
	private float LINE_HEIGHT;
	private float LINE_SPACING;
	private const float ELEMENT_SPACING = 48.0f;

	private ReorderableList mList;

	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		mList.DoLayoutList();
		serializedObject.ApplyModifiedProperties();
	}

	private void OnEnable()
	{
		LINE_HEIGHT = EditorGUIUtility.singleLineHeight;
		LINE_SPACING = EditorGUIUtility.standardVerticalSpacing;

		SerializedProperty property = serializedObject.FindProperty("mGameEventData");
		mList = new ReorderableList(property.serializedObject, property, true, true, true, true);
		mList.drawHeaderCallback = OnDrawHeader;
		mList.drawElementCallback = OnDrawElement;
		mList.elementHeightCallback = OnElementHeight;
	}

	private void OnDisable()
	{
		mList = null;
	}

	private void OnDrawHeader(Rect rect)
	{
		GUI.Label(rect, new GUIContent("Game Events"));
	}

	private void OnDrawElement(Rect rect, int index, bool isActive, bool isFocused)
	{
		SerializedProperty foldoutProperty = GetPropertyFromIndexAndName(index, "IsFoldOut");
		SerializedProperty depthProperty = GetPropertyFromIndexAndName(index, "TriggerAtDepth");
		SerializedProperty typeProperty = GetPropertyFromIndexAndName(index, "EventType");

		Rect depthRect = new Rect(rect.x, rect.y, EditorGUIUtility.labelWidth - 4.0f, LINE_HEIGHT);
		Rect typeRect = new Rect(rect.x + EditorGUIUtility.labelWidth, rect.y, rect.width - EditorGUIUtility.labelWidth, LINE_HEIGHT);
		EditorGUI.PropertyField(depthRect, depthProperty, GUIContent.none);
		EditorGUI.PropertyField(typeRect, typeProperty, GUIContent.none);

		GameEventCore.EGameEventType type = (GameEventCore.EGameEventType)typeProperty.enumValueIndex;

		Rect propertyRect = new Rect(rect.x, rect.y + LINE_HEIGHT + LINE_SPACING, rect.width, LINE_HEIGHT);
		Rect tempRect = propertyRect;

		DrawProperty(ref propertyRect, index, "Name", false);
		foldoutProperty.isExpanded = foldoutProperty.boolValue = EditorGUI.Foldout(tempRect, foldoutProperty.boolValue, GUIContent.none);
		
		if (foldoutProperty.isExpanded)
		{
			propertyRect.x += LINE_HEIGHT + LINE_SPACING;
			propertyRect.width -= LINE_HEIGHT + LINE_SPACING;
			string[] gameEventProperties = GameEventCore.GameEventActions[(int)type].OnGetProperties();
			for (int i = 0; i < gameEventProperties.Length; ++i)
			{
				DrawProperty(ref propertyRect, index, gameEventProperties[i]);
			}
		}
	}

	private void DrawProperty(ref Rect rect, int index, string propertyName, bool hasLabel = true)
	{
		float height = LINE_HEIGHT;

		Rect fieldRect = new Rect();
		fieldRect.x = rect.x;
		fieldRect.y = rect.y;
		fieldRect.width = rect.width;
		fieldRect.height = height;

		SerializedProperty property = GetPropertyFromIndexAndName(index, propertyName);
		if (property != null)
		{
			height = GetPropertyHeight(property);
			fieldRect.height = height;
			EditorGUI.PropertyField(fieldRect, property, hasLabel ? new GUIContent(property.name) : GUIContent.none, true);
		}
		else
		{
			EditorGUI.LabelField(rect, "Property \"" + propertyName + "\" does not exist in the GameEventData class.", EditorStyles.boldLabel);
		}
		rect.y += height + LINE_SPACING;
	}

	private float OnElementHeight(int index)
	{
		float height = 0.0f;
		SerializedProperty foldoutProperty = GetPropertyFromIndexAndName(index, "IsFoldOut");
		if (foldoutProperty.isExpanded)
		{
			GameEventCore.EGameEventType type = (GameEventCore.EGameEventType)GetPropertyFromIndexAndName(index, "EventType").enumValueIndex;
			string[] gameEventProperties = GameEventCore.GameEventActions[(int)type].OnGetProperties();
			for (int i = 0; i < gameEventProperties.Length; ++i)
			{
				SerializedProperty property = GetPropertyFromIndexAndName(index, gameEventProperties[i]);
				if (property != null)
				{
					height += GetPropertyHeight(property) + LINE_SPACING;
				}
				else
				{
					height += LINE_HEIGHT + LINE_SPACING;
				}
			}
		}
		return ELEMENT_SPACING + LINE_HEIGHT + LINE_SPACING + height;
	}

	private SerializedProperty GetPropertyFromIndexAndName(int index, string name)
	{
		SerializedProperty indexProperty = mList.serializedProperty.GetArrayElementAtIndex(index);
		if (indexProperty == null)
			return null;

		SerializedProperty relativeProperty = indexProperty.FindPropertyRelative(name);
		return relativeProperty;
	}

	private float GetPropertyHeight(SerializedProperty property)
	{
		if (property.isArray && property.isExpanded)
		{
			return (LINE_HEIGHT + LINE_SPACING) * (property.arraySize + 2);
		}
		return LINE_HEIGHT;
	}
}
