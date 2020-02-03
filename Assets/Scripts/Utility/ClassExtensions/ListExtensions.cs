using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ListExtensions
{
	public static T Last<T>(this List<T> list)
	{
		return list[list.Count - 1];
	}

	public static void RemoveLast<T>(this List<T> list)
	{
		list.RemoveAt(list.Count - 1);
	}

	public static T Random<T>(this List<T> list)
	{
		return list[UnityEngine.Random.Range(0, list.Count - 1)];
	}

	public static int LastIndex<T>(this List<T> list)
	{
		return list.Count - 1;
	}
}
