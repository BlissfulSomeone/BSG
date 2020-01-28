using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct MinMaxInt
{
	[SerializeField] public int Min;
	[SerializeField] public int Max;

	public MinMaxInt(int min, int max)
	{
		Min = min;
		Max = max;
	}

	public int Get()
	{
		return Random.Range(Min, Max);
	}
}

[System.Serializable]
public struct MinMaxFloat
{
	[SerializeField] public float Min;
	[SerializeField] public float Max;

	public MinMaxFloat(float min, float max)
	{
		Min = min;
		Max = max;
	}

	public float Get()
	{
		return Random.Range(Min, Max);
	}
}
