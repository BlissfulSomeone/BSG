using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BSGFakePhysics), typeof(Triggerable))]
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
	[SerializeField] private float mDamage;
	[SerializeField] private bool mCanBeTriggeredByExplosion;
	[SerializeField] private bool mCanBeTriggeredByImpact;

	[SerializeField] private Explosion mExplosionPrefab;

	[SerializeField] private SpawnOnDestroy[] mToSpawnOnDestroy;

	private BSGFakePhysics mFakePhysics;
	public BSGFakePhysics FakePhysics { get { if (mFakePhysics == null) mFakePhysics = GetComponent<BSGFakePhysics>(); return mFakePhysics; } }

	private Triggerable mTriggerable;

	private float mCurrentTimer = 0.0f;
	private bool mIsTriggered = false;
	
	private void Awake()
	{
		mFakePhysics = GetComponent<BSGFakePhysics>();
		mFakePhysics.OnImpact += OnImpact;
		mTriggerable = GetComponent<Triggerable>();
		mTriggerable.OnTriggered += Trigger;
	}

	private void OnDestroy()
	{
		mTriggerable.OnTriggered -= Trigger;
	}

	private void Trigger(ExplosionData explosionData)
	{
		if (mCanBeTriggeredByExplosion && !mIsTriggered)
		{
			mIsTriggered = true;
			mCurrentTimer = Random.Range(0.3f, 0.6f);
		}
	}

	private void OnImpact()
	{
		if (mCanBeTriggeredByImpact)
		{
			Explode();
		}
	}

	private void FixedUpdate()
	{
		if (mHasTimer == true)
		{
			if (mIsTriggered == false)
			{
				if (mFakePhysics.Velocity.IsZero() == true)
				{
					mIsTriggered = true;
					mCurrentTimer = mTimer;
				}
			}
		}
	}

	private void Update()
	{
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

	public void Explode()
	{
		foreach (SpawnOnDestroy i in mToSpawnOnDestroy)
		{
			Instantiate(i.objectToSpawn, transform.position + i.spawnOffset.ToVec3(), Quaternion.identity);
		}
		
		Explosion explosionInstance = Instantiate(mExplosionPrefab);
		explosionInstance.ExplosionData = new ExplosionData(transform.position, mExplosionRadius, mDamage, false);
		Destroy(gameObject);
	}

	private void OnGUI()
	{
		if (mHasTimer == true)
		{
			Vector2 screenPosition = GameController.Instance.CameraControllerInstance.CameraComponent.WorldToScreenPoint(transform.position);
			float w = 64.0f;
			float h = 24.0f;
			float x = screenPosition.x - w * 0.5f;
			float y = Screen.height - (screenPosition.y + h * 0.5f);
			GUI.TextField(new Rect(x, y, w, h), mCurrentTimer.ToString());
		}
	}
}
