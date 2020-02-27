using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class WeightedObjectExtension
{
	public static MonoBehaviour GetRandom(this WeightedObject[] weightedObjects)
	{
		int totalWeight = 0;
		foreach (WeightedObject weightedObject in weightedObjects)
		{
			totalWeight += weightedObject.Weight;
		}
		int random = Random.Range(0, totalWeight);
		int buffer = 0;
		foreach (WeightedObject weightedObject in weightedObjects)
		{
			if (random < buffer + weightedObject.Weight)
			{
				return weightedObject.Object;
			}
			buffer += weightedObject.Weight;
		}
		return null;
	}
}
