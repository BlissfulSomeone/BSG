using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteBombSpawner : MonoBehaviour
{
	private WeightedObject[] mBombPrefabs;
	private float mBombsPerSecond;
	private float mRampPerDepth;
	private float mMaxBombsPerSecond;
	private float mSpawnTimer;

	private WeightedObject[] BombPrefabs { get { if (mBombPrefabs == null) mBombPrefabs = new WeightedObject[0]; return mBombPrefabs; } set { mBombPrefabs = value; } }

	private void Awake()
	{
		mSpawnTimer = 0.0f;
	}

	private void Update()
	{
		if (BombPrefabs.Length > 0 && mBombsPerSecond > 0.0f)
		{
			mSpawnTimer += Time.deltaTime;
			float depth = GameController.Instance.FurthestDepth;
			float interval = 1.0f / Mathf.Min(mBombsPerSecond + mRampPerDepth * depth, mMaxBombsPerSecond != 0.0f ? mMaxBombsPerSecond : float.MaxValue);
			if (mSpawnTimer >= interval)
			{
				mSpawnTimer -= interval;

				Vector2 force = new Vector2(Random.Range(0.0f, 0.0f), 0.0f);

				MonoBehaviour bomb = Instantiate(BombPrefabs.GetRandom());
				bomb.transform.Reset();
				bomb.transform.position = new Vector2(Random.Range(-8.0f, 8.0f), 10.0f);
				BSGFakePhysics fakePhysics = bomb.GetComponent<BSGFakePhysics>();
				if (fakePhysics != null)
				{
					fakePhysics.AddForce(force);
				}
			}
		}
	}

	public void SetBPS(float bps, float ramp, float maxBps)
	{
		mBombsPerSecond = bps;
		mRampPerDepth = ramp;
		mMaxBombsPerSecond = maxBps;
	}

	public void SetAllowedBombs(MonoBehaviour[] bombPrefabs)
	{
		BombPrefabs = new WeightedObject[bombPrefabs.Length];
		for (int i = 0; i < bombPrefabs.Length; ++i)
		{
			BombPrefabs[i].Weight = 1;
			BombPrefabs[i].Object = bombPrefabs[i];
		}
	}

	public void SetAllowedBombs(WeightedObject[] weightedBombPrefabs)
	{
		BombPrefabs = weightedBombPrefabs;
	}
}
