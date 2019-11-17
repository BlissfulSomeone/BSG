using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bomb : MonoBehaviour
{
	[System.Serializable]
	private struct SpawnOnDestroy
	{
		public GameObject objectToSpawn;
		public Vector2 spawnOffset;
	}

	[SerializeField] private bool mHasTimer;
	[SerializeField] private float mTimer;
	[SerializeField] private float mExplosionRadius;
	[SerializeField] private bool mCanBeTriggeredByExplosion;

	[SerializeField] private Explosion mExplosionPrefab;

	[SerializeField] private SpawnOnDestroy[] mToSpawnOnDestroy;

	private Rigidbody mRigidbody;
	public Rigidbody Rigidbody { get { return mRigidbody; } }

	private float mCurrentTimer = 0.0f;
	private bool mIsTriggered = false;
	
	private void Awake()
	{
		mRigidbody = GetComponent<Rigidbody>();
	}
	
	private void OnDestroy()
	{
		GameController.Instance.BombControllerInstance.UnregisterBomb(this);
	}
	
	public void Trigger()
	{
		if (mCanBeTriggeredByExplosion == true && mIsTriggered == false)
		{
			mIsTriggered = true;
			mCurrentTimer = Random.Range(0.3f, 0.6f);
		}
	}

	private void Update()
	{
		if (mHasTimer == true)
		{
			if (mIsTriggered == false)
			{
				if (mRigidbody.velocity.IsZero() == true)
				{
					mIsTriggered = true;
					mCurrentTimer = mTimer;
				}
			}
		}

		if (mIsTriggered == true)
		{
			if (mCurrentTimer > 0.0f)
			{
				mCurrentTimer -= Time.deltaTime;
			}
			else
			{
				Explode();
			}
		}
	}

	private void Explode()
	{
		foreach (SpawnOnDestroy i in mToSpawnOnDestroy)
		{
			GameObject spawnedObject = Instantiate(i.objectToSpawn, transform.position + i.spawnOffset.ToVec3(), Quaternion.identity);
			Bomb bomb = spawnedObject.GetComponent<Bomb>();
			if (bomb != null)
			{
				GameController.Instance.BombControllerInstance.RegisterBomb(bomb);
			}
		}

		GameController.Instance.Explode(transform.position, 3.0f);
		Explosion explosionInstance = Instantiate(mExplosionPrefab);
		explosionInstance.transform.position = transform.position;
		explosionInstance.transform.localScale = Vector3.one * mExplosionRadius * 2.0f;
		Destroy(gameObject);
	}

	private void OnGUI()
	{
		if (mHasTimer == true)
		{
			Vector2 screenPosition = GameController.Instance.CameraControllerInstance.CameraComponent.WorldToScreenPoint(transform.position);
			float x = screenPosition.x - 16.0f;
			float y = Screen.height - (screenPosition.y + 16.0f);
			float w = 32.0f;
			float h = 32.0f;
			GUI.TextField(new Rect(x, y, w, h), mCurrentTimer.ToString());
		}
	}
}
