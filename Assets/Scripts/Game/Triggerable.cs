using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BSGFakePhysics))]
public class Triggerable : MonoBehaviour
{
	public delegate void OnTriggeredHandle(ExplosionInstance explosionInstance);
	public OnTriggeredHandle OnTriggered;

	public delegate void OnPreTimedEventHandler();
	public OnPreTimedEventHandler OnPreTimedEvent;

	public delegate void OnPostTimedEventHandler();
	public OnPostTimedEventHandler OnPostTimedEvent;

	[SerializeField] private bool mHasPhysics = true;
	public bool HasPhysics { get { return mHasPhysics; } }

	[SerializeField] private float mUpForceMultiplier = 1.25f;
	public float UpForceMultiplier { get { return mUpForceMultiplier; } }

	private Coroutine mCurrentCoroutine = null;
	private Vector2 mCurrentVelocity = Vector2.zero;

	private BSGFakePhysics mFakePhysics;
	public BSGFakePhysics FakePhysics
	{
		get
		{
			if (mFakePhysics == null)
				mFakePhysics = GetComponent<BSGFakePhysics>();
			return mFakePhysics;
		}
	}

	private void Awake()
	{
		FakePhysics.OnImpact += OnImpact;
	}

	private void OnDestroy()
	{
		FakePhysics.OnImpact -= OnImpact;
	}

	private void OnImpact()
	{
		StopConstantVelocityOverTime();
	}

	public void Trigger(ExplosionInstance explosionInstance)
	{
		OnTriggered?.Invoke(explosionInstance);
	}

	public void ApplyConstantVelocityOverTime(float speed, Vector2 direction, float time)
	{
		StopConstantVelocityOverTime();
		mCurrentVelocity = direction * speed;
		mCurrentCoroutine = StartCoroutine(Coroutine_ApplyConstantVelocityOverTime(speed, direction, time));
	}

	private IEnumerator Coroutine_ApplyConstantVelocityOverTime(float speed, Vector2 direction, float time)
	{
		DisablePhysics();
		yield return new WaitForSeconds(time);
		StopConstantVelocityOverTime();
	}

	private void StopConstantVelocityOverTime()
	{
		if (mCurrentCoroutine != null)
		{
			EnablePhysics();
			StopCoroutine(mCurrentCoroutine);
			mCurrentCoroutine = null;
			mCurrentVelocity = Vector2.zero;
		}
	}

	private void DisablePhysics()
	{
		FakePhysics.IsAffectedByGravity = false;
		FakePhysics.IsAffectedByFriction = false;
		FakePhysics.Velocity = mCurrentVelocity;
		OnPreTimedEvent?.Invoke();
	}

	private void EnablePhysics()
	{
		FakePhysics.IsAffectedByGravity = true;
		FakePhysics.IsAffectedByFriction = true;
		FakePhysics.Velocity = mCurrentVelocity / 2;
		OnPostTimedEvent?.Invoke();
	}
}
