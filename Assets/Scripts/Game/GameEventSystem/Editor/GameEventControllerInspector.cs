using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameEventController))]
public class GameEventControllerInspector : Editor
{
	public override void OnInspectorGUI()
	{
		GUIGlobals.PrefabMenu<GameEventBase>("Create Game Event", OnClickedMenu, GUILayout.Height(50));
		if (GUILayout.Button(new GUIContent("Sanitize Game Events"), GUILayout.Height(50)) == true)
		{
			((GameEventController)target).ClearEvents();
			((GameEventController)target).FetchEvents();
		}

		DrawDefaultInspector();
	}

	private void OnClickedMenu(object aObject)
	{
		GameEventBase gameEvent = Instantiate((GameEventBase)aObject);
		gameEvent.transform.SetParent(((GameEventController)target).transform);
		gameEvent.transform.Reset(true);
		((GameEventController)target).AddEvent(gameEvent);
	}
}
