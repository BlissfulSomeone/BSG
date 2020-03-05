using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Globals
{
	public static void DestroyAllOfType<T>() where T : MonoBehaviour
	{
		T[] i = Object.FindObjectsOfType<T>();
		foreach (T j in i)
		{
			Object.Destroy(j.gameObject);
		}
	}
	
	public static List<T> GetAllPrefabsOfType<T>() where T : MonoBehaviour
	{
		List<T> result = new List<T>();
		Resources.LoadAll<T>("Assets/");
		T[] potentialPrefabs = Resources.FindObjectsOfTypeAll<T>();
		foreach (T potentialPrefab in potentialPrefabs)
		{
			if (potentialPrefab.transform.parent == null)
			{
				result.Add(potentialPrefab);
			}
		}
		return result;
	}

	public static void GetAllPrefabsOfTypeNoAlloc<T>(List<T> list) where T : MonoBehaviour
	{
		T[] potentialPrefabs = Resources.FindObjectsOfTypeAll<T>();
		foreach (T potentialPrefab in potentialPrefabs)
		{
			if (potentialPrefab.transform.parent == null)
			{
				list.Add(potentialPrefab);
			}
		}
	}

	public static int GetCollisionMask(GameObject gameObject)
	{
		int collisionMask = 0;
		int layer = gameObject.layer;
		for (int i = 0; i < 16; ++i)
		{
			if (!Physics.GetIgnoreLayerCollision(layer, i))
			{
				collisionMask = collisionMask | (1 << i);
			}
		}
		return collisionMask;
	}
}

public class GUIGlobals
{
	public static void PrefabMenu<T>(string buttonLabel, GenericMenu.MenuFunction2 callback, params GUILayoutOption[] options) where T : MonoBehaviour
	{
		if (GUILayout.Button(buttonLabel, options) == true)
		{
			GenericMenu menu = new GenericMenu();
			List<T> prefabs = Globals.GetAllPrefabsOfType<T>();
			foreach (T prefab in prefabs)
			{
				menu.AddItem(new GUIContent(prefab.name), false, callback, prefab);
			}
			menu.ShowAsContext();
		}
	}
}
