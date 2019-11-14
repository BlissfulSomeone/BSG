﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombController : MonoBehaviour
{
	private List<Bomb> mSpawnedBombs;

	private void Awake()
	{
		mSpawnedBombs = new List<Bomb>();
	}

	public void SpawnBomb(Bomb aBomb)
	{
		mSpawnedBombs.Add(aBomb);
	}

	public void RemoveBomb(Bomb aBomb)
	{
		mSpawnedBombs.Remove(aBomb);
	}

	public void Explode(Vector2 aExplosionSource, float aExplosionRadius)
	{
		for (int i = 0; i < mSpawnedBombs.Count; ++i)
		{
			Bomb bomb = mSpawnedBombs[i];
			Vector2 delta = bomb.transform.position.ToVec2() - aExplosionSource;
			float distance = delta.magnitude;
			if (distance <= aExplosionRadius)
			{
				bomb.Rigidbody.AddExplosionForce(10.0f, aExplosionSource, aExplosionRadius, 5.0f, ForceMode.VelocityChange);
				bomb.Trigger();
			}
		}
	}
}
