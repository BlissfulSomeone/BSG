using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameEventController))]
public class GameEventControllerInspector : Editor
{
	public override void OnInspectorGUI()
	{
		if (GUILayout.Button(new GUIContent("Create Game Event"), GUILayout.Height(50)) == true)
		{
			GenericMenu menu = new GenericMenu();
			menu.AddItem(new GUIContent("Infinite Bomb Pattern"), false, OnClickedMenu, typeof(InfiniteBombPattern));
			menu.AddItem(new GUIContent("Staged Bomb Pattern"), false, OnClickedMenu, typeof(StagedBombPattern));
			menu.AddItem(new GUIContent("Nested Game Events"), false, OnClickedMenu, typeof(NestedGameEvent));
			menu.AddItem(new GUIContent("Remove Bomb Event"), false, OnClickedMenu, typeof(RemoveBombsEvent));
			menu.ShowAsContext();
		}
		if (GUILayout.Button(new GUIContent("Sanitize Game Events"), GUILayout.Height(50)) == true)
		{
			((GameEventController)target).ClearEvents();
			((GameEventController)target).FetchEvents();
		}

		DrawDefaultInspector();
	}

	private void OnClickedMenu(object aObject)
	{
		GameObject go = new GameObject(((Type)aObject).ToString());
		go.transform.SetParent(((GameEventController)target).transform);
		go.transform.Reset(true);
		Component component = go.AddComponent((Type)aObject);
		((GameEventController)target).AddEvent((GameEventBase)component);
	}
}
