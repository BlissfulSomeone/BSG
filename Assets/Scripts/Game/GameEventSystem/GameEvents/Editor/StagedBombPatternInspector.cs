using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(StagedBombPattern))]
public class StagedBombPatternInspector : Editor
{
	public override void OnInspectorGUI()
	{
		if (GUILayout.Button("Create Bomb Spawner") == true)
		{
			GameObject go = new GameObject("Bomb Spawner");
			go.transform.SetParent(((StagedBombPattern)target).transform);
			go.transform.Reset(true);
			go.AddComponent<StagedBombSpawner>();
		}
		DrawDefaultInspector();
	}
}
