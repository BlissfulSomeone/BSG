using System;
using UnityEngine;

[Serializable]
public struct WeightedObject
{
	[SerializeField] public int Weight;
	[SerializeField] public MonoBehaviour Object;
}
