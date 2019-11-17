using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
}
