using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(InfiniteBombSpawner))]
public class InfiniteBombSpawnerInspector : Editor
{
	private const float EPSILON = 0.01f;
	private float mPreviewLength = 10.0f;

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		InfiniteBombSpawner bombSpawner = (InfiniteBombSpawner)target;

		mPreviewLength = Mathf.Max(bombSpawner.BombSpawnSettings.Last().fromDepth, EditorGUILayout.FloatField(new GUIContent("Preview Length"), mPreviewLength));

		AnimationCurve curve = new AnimationCurve();
		curve.AddKey(0.0f, 0.0f);
		for (int i = 0; i < bombSpawner.BombSpawnSettings.Count; ++i)
		{
			InfiniteBombSpawner.BombSpawnSetting currentSetting = bombSpawner.BombSpawnSettings[i];
			curve.AddKey(currentSetting.fromDepth + EPSILON, currentSetting.bombsPerSecond);
			if (i < bombSpawner.BombSpawnSettings.Count - 1)
			{
				InfiniteBombSpawner.BombSpawnSetting nextSetting = bombSpawner.BombSpawnSettings[i + 1];
				curve.AddKey(nextSetting.fromDepth, 1.0f / currentSetting.EvaluateInterval(nextSetting.fromDepth));
			}
		}
		curve.AddKey(mPreviewLength, 1.0f / bombSpawner.BombSpawnSettings.Last().EvaluateInterval(mPreviewLength));
		for (int i = 0; i < curve.keys.Length; ++i)
		{
			AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
			AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
		}

		EditorGUILayout.CurveField(curve, GUILayout.Height(150));
	}
}
