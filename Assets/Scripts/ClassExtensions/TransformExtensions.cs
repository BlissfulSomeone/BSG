using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransformExtensions
{
	public static void Reset(this Transform transform)
	{
		transform.position = Vector3.zero;
		transform.rotation = Quaternion.identity;
		transform.localScale = Vector3.one;
	}
}
