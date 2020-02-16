using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BSGFakePhysics), typeof(Triggerable))]
public class Bomb : MonoBehaviour
{
	private enum EBombState
	{
		Idle,
		Timer,
		Ignited
	}

	[System.Serializable]
	private struct SpawnOnDestroy
	{
		public GameObject objectToSpawn;
		public Vector2 spawnOffset;
	}

	private const string ANIMATION_IDLE = "Idle";
	private const string ANIMATION_IGNITE = "Ignite";

	[SerializeField] private AnimationClip mIdleAnimation;
	[SerializeField] private AnimationClip mIgniteAnimation;
	[SerializeField] private bool mHasTimer;
	[SerializeField] private float mIdleTime;
	[SerializeField] private float mIgniteTime;
	[SerializeField] private float mExplosionRadius;
	[SerializeField] private float mDamage;
	[SerializeField] private bool mCanBeTriggeredByExplosion;
	[SerializeField] private bool mCanBeTriggeredByImpact;

	[SerializeField] private Explosion mExplosionPrefab;

	[SerializeField] private SpawnOnDestroy[] mToSpawnOnDestroy;

	private BSGFakePhysics mFakePhysics;
	public BSGFakePhysics FakePhysics { get { if (mFakePhysics == null) mFakePhysics = GetComponent<BSGFakePhysics>(); return mFakePhysics; } }
    public bool CanBeTriggerByImpact { get { return mCanBeTriggeredByImpact; } set { mCanBeTriggeredByImpact = value; } }

	private Triggerable mTriggerable;

	private EBombState mState = EBombState.Idle;
	private Animation mAnimationComponent;

	private float mCurrentTimer = 0.0f;
	private bool mIsTriggered = false;
	private bool mHasTimerStarted = false;
	
	private void Awake()
	{
		mFakePhysics = GetComponent<BSGFakePhysics>();
		mFakePhysics.OnImpact += OnImpact;
		mTriggerable = GetComponent<Triggerable>();
		mTriggerable.OnTriggered += Trigger;

		mAnimationComponent = GetComponentInChildren<Animation>();
		mAnimationComponent.AddClip(mIdleAnimation, ANIMATION_IDLE);
		mAnimationComponent.AddClip(mIgniteAnimation, ANIMATION_IGNITE);
		mAnimationComponent.Play(ANIMATION_IDLE);
	}

	private void OnDestroy()
	{
		mTriggerable.OnTriggered -= Trigger;
	}

	private void Trigger(ExplosionData explosionData)
	{
		if (mCanBeTriggeredByExplosion)
		{
			Ignite();
		}
	}

	private void Ignite()
	{
		if (mState == EBombState.Ignited)
			return;

		mCurrentTimer = mIgniteTime;
		mState = EBombState.Ignited;
		mAnimationComponent.Play(ANIMATION_IGNITE);
		foreach (AnimationState animationState in mAnimationComponent)
		{
			if (animationState.name == ANIMATION_IGNITE)
			{
				animationState.speed = 1.0f / (mIgniteTime / animationState.length);
				Debug.Log(animationState.name + " - Ignite time: " + mIgniteTime.ToString() + ", Anim length: " + animationState.length.ToString() + ", Speed: " + animationState.speed.ToString());
			}
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
		if (mHasTimer && mState == EBombState.Idle)
		{
			if (mFakePhysics.Velocity.IsZero() == true)
			{
				mState = EBombState.Timer;
				mCurrentTimer = mIdleTime;
			}
		}
	}

	private void Update()
	{
		if (mCurrentTimer > 0.0f)
		{
			mCurrentTimer -= Time.deltaTime;
		}
		else
		{
			if (mState == EBombState.Timer)
			{
				Ignite();
			}
			else if (mState == EBombState.Ignited)
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

	//private void OnGUI()
	//{
	//	if (mHasTimer == true)
	//	{
	//		Vector2 screenPosition = GameController.Instance.CameraControllerInstance.CameraComponent.WorldToScreenPoint(transform.position);
	//		float w = 64.0f;
	//		float h = 24.0f;
	//		float x = screenPosition.x - w * 0.5f;
	//		float y = Screen.height - (screenPosition.y + h * 0.5f);
	//		GUI.TextField(new Rect(x, y, w, h), mCurrentTimer.ToString());
	//	}
	//}
}
