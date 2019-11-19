using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombSpawner : MonoBehaviour
{
	[System.Serializable]
	public class BombSpawnSettings
	{
		public int fromDepth;
		public float spawnInterval;
		public Bomb[] bombsToSpawn;
	}

	[SerializeField] private List<BombSpawnSettings> mBombSpawnSettings;

	private BombSpawnSettings mCachedBombSettings;
	private float mSpawnTimer;

	private void Awake()
	{
		mSpawnTimer = 0.0f;
	}

	private void Update()
	{
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
			if (mSpawnTimer >= mCachedBombSettings.spawnInterval)
			{
				mSpawnTimer -= mCachedBombSettings.spawnInterval;

				Bomb bomb = Instantiate(mCachedBombSettings.bombsToSpawn[Random.Range(0, mCachedBombSettings.bombsToSpawn.Length)]);
				bomb.transform.position = new Vector2(Random.Range(-8.0f, 8.0f), 10.0f);
				bomb.Rigidbody.AddForce(Random.Range(-10.0f, 10.0f), 0.0f, 0.0f, ForceMode.VelocityChange);
			}
		}
	}
}
