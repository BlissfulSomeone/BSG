using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteBombSpawner : MonoBehaviour
{
	[System.Serializable]
	public class BombSpawnSetting
	{
		public int fromDepth;
		public float bombsPerSecond;
		public float extraBombsPerSecondPerDepth;
		public float maxBombsPerSecond;
		public Bomb[] bombsToSpawn;

		public float EvaluateInterval(float aDepth)
		{
			float delta = Mathf.Max(aDepth - fromDepth, 0);
			float extra = extraBombsPerSecondPerDepth * delta;
			float bps = bombsPerSecond + extra;
			if (maxBombsPerSecond != 0.0f)
				bps = Mathf.Min(bps, maxBombsPerSecond);
			float interval = 1.0f / bps;
			return interval;
		}
	}

	[SerializeField] private List<BombSpawnSetting> mBombSpawnSettings;
	public List<BombSpawnSetting> BombSpawnSettings { get { return mBombSpawnSettings; } }

	private BombSpawnSetting mCachedBombSettings;
	private float mSpawnTimer;

	private void Awake()
	{
		mSpawnTimer = 0.0f;
	}

	private void Update()
	{
		float furthestDepth = GameController.Instance.FurthestDepth;
		for (int i = mBombSpawnSettings.Count - 1; i >= 0; --i)
		{
			if (GameController.Instance.FurthestDepth > mBombSpawnSettings[i].fromDepth)
			{
				mCachedBombSettings = mBombSpawnSettings[i];
				break;
			}
		}

		if (mCachedBombSettings != null)
		{
			mSpawnTimer += Time.deltaTime;
			float interval = mCachedBombSettings.EvaluateInterval(furthestDepth);
			if (mSpawnTimer >= interval)
			{
				mSpawnTimer -= interval;

				Bomb bomb = Instantiate(mCachedBombSettings.bombsToSpawn[Random.Range(0, mCachedBombSettings.bombsToSpawn.Length)]);
				bomb.transform.position = new Vector2(Random.Range(-8.0f, 8.0f), 10.0f);
				bomb.Rigidbody.AddForce(Random.Range(-10.0f, 10.0f), 0.0f, 0.0f, ForceMode.VelocityChange);
			}
		}
	}
}
