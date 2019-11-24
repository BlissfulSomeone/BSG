using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransformExtensions
{
	public static void Reset(this Transform transform, bool local = false)
	{
		if (local == false || transform.parent == null)
		{
			transform.position = Vector3.zero;
			transform.rotation = Quaternion.identity;
		}
		else
		{
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
		}
		transform.localScale = Vector3.one;
	}
}
