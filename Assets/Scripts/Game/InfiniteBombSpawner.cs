using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteBombSpawner : MonoBehaviour
{
	private Bomb[] mBombPrefabs;
	private float mBombsPerSecond;
	private float mSpawnTimer;

	private Bomb[] BombPrefabs { get { if (mBombPrefabs == null) mBombPrefabs = new Bomb[0]; return mBombPrefabs; } set { mBombPrefabs = value; } }

	private void Awake()
	{
		mSpawnTimer = 0.0f;
	}

	private void Update()
	{
		if (BombPrefabs.Length > 0 && mBombsPerSecond > 0.0f)
		{
			mSpawnTimer += Time.deltaTime;
			float interval = 1.0f / mBombsPerSecond;
			if (mSpawnTimer >= interval)
			{
				mSpawnTimer -= interval;

				Vector2 force = new Vector2(Random.Range(-10.0f, 10.0f), 0.0f);

				Bomb bomb = Instantiate(BombPrefabs[Random.Range(0, BombPrefabs.Length)]);
				bomb.transform.Reset();
				bomb.transform.position = new Vector2(Random.Range(-8.0f, 8.0f), 10.0f);
				bomb.FakePhysics.AddForce(force);
			}
		}
	}

	public void SetBPS(float aBPS)
	{
		mBombsPerSecond = aBPS;
	}

	public void SetAllowedBombs(Bomb[] aBombPrefabs)
	{
		BombPrefabs = aBombPrefabs;
	}
}
