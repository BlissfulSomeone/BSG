using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(InfiniteBombPattern))]
[CanEditMultipleObjects]
public class InfiniteBombPatternInspector : Editor
{
	public override void OnInspectorGUI()
	{
		GUIGlobals.PrefabMenu<Bomb>("Add Bomb Type", OnClickedMenu);
		DrawDefaultInspector();
	}

	private void OnClickedMenu(object aObject)
	{
		((InfiniteBombPattern)target).AddBombPrefab((Bomb)aObject);
	}

	private void SomeOtherCallback(object aObject)
	{
		Debug.Log(aObject.ToString());
	}
}
