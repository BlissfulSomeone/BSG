using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CustomChunkData))]
public class CustomChunkDataInspector : Editor
{
	public override void OnInspectorGUI()
	{
		if (GUILayout.Button("Test Render Chunk") == true)
		{

		}
		if (GUILayout.Button("Stop Render Chunk") == true)
		{
			
		}

		DrawDefaultInspector();
	}
}
