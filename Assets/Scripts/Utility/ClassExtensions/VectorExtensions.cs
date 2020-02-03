using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorExtensions
{
	private static float EPSILON = Mathf.Epsilon;
	
	//=============================================================
	// Vector3
	//=============================================================

	public static Vector2 ToVec2(this Vector3 vec)
	{
		return new Vector2(vec.x, vec.y);
	}

	public static bool IsZero(this Vector3 vec, float epsilon = 1.0E-6f)
	{
		return vec.sqrMagnitude < epsilon;
	}



	//=============================================================
	// Vector2
	//=============================================================

	public static Vector3 ToVec3(this Vector2 vec)
	{
		return new Vector3(vec.x, vec.y, 0.0f);
	}

	public static Vector3 ToVec3_1(this Vector2 vec)
	{
		return new Vector3(vec.x, vec.y, 1.0f);
	}

	public static bool IsZero(this Vector2 vec, float epsilon = 1.0E-6f)
	{
		return vec.sqrMagnitude < epsilon;
	}
}
