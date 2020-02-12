﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
	[System.Serializable]
	private struct Movement
	{
		public float acceleration;
		public float maxSpeed;
		public float jumpForce;
		public float jumpTime;
	}

	[System.Serializable]
	private struct AbilitySet
	{
		public string InputName;
		public Ability Ability;
	}

	public delegate void OnKilledHandler(Character aKilledCharacter);
	public OnKilledHandler OnKilled;

	private Renderer mRenderer;
	private BSGFakePhysics mFakePhysics;
	private BoxCollider mBoxCollider;
	private Triggerable mTriggerable;

	public Renderer Renderer { get { if (mRenderer == null) mRenderer = GetComponent<Renderer>(); return mRenderer; } }
	public BSGFakePhysics FakePhysics { get { if (mFakePhysics == null) mFakePhysics = GetComponent<BSGFakePhysics>(); return mFakePhysics; } }

	[Header("Generic Input Names")]
	[SerializeField] private string mHorizontalMovementInputName;
	[SerializeField] private string mJumpInputName;

	[Header("Movement")]
	[SerializeField] private Movement mMovement;

	[Header("Stats")]
	[SerializeField] private float mMaxHealth;
	[SerializeField] private AbilitySet[] mAbilities;
	
	private Vector3 mMovementInput = Vector3.zero;
	private bool mWantToJump = false;
	private float mJumpTime = -1.0f;
	private bool mIsFlipped = false;

	private float mHealth;
	public float Health { get { return mHealth; } }
	public float MaxHealth { get { return mMaxHealth; } }
    public Vector3 Velocity { get { return FakePhysics.Velocity; } }
	public bool IsFlipped { get { return mIsFlipped; } }
	
    private void Awake()
	{
		mRenderer = GetComponent<Renderer>();
		mFakePhysics = GetComponent<BSGFakePhysics>();
		mBoxCollider = GetComponent<BoxCollider>();
		mTriggerable = GetComponent<Triggerable>();
		mTriggerable.OnTriggered += Trigger;

		mHealth = mMaxHealth;

		for (int i = 0; i < mAbilities.Length; ++i)
		{
			mAbilities[i].Ability.Initialize(this);
		}
	}
	
	private void OnDestroy()
	{
		mTriggerable.OnTriggered -= Trigger;
	}

	private void Trigger(ExplosionData explosionData)
	{
		if (!explosionData.Friendly)
		{
			mHealth -= explosionData.Damage;
            if (mHealth <= 0.0f)
			{
				OnKilled?.Invoke(this);
				Destroy(gameObject);
			}
		}
	}

	private void Update()
	{
		mMovementInput.x = Input.GetAxisRaw(mHorizontalMovementInputName);
		mWantToJump = Input.GetButton(mJumpInputName);
		if (FakePhysics.IsGrounded && mWantToJump)
		{
			mJumpTime = mMovement.jumpTime;
		}
		else if (Input.GetButtonUp(mJumpInputName))
		{
			mJumpTime = -1.0f;
		}
		for (int i = 0; i < mAbilities.Length; ++i)
		{
			if (Input.GetButtonDown(mAbilities[i].InputName))
			{
				mAbilities[i].Ability.StartAbility();
			}
			if (Input.GetButtonUp(mAbilities[i].InputName))
			{
				mAbilities[i].Ability.StopAbility();
			}
			mAbilities[i].Ability.UpdateAbility();
		}
	}

	private float Damp(float aSource, float aSmoothing, float aDeltaTime)
	{
		return aSource * Mathf.Pow(aSmoothing, aDeltaTime);
	}

	private void FixedUpdate()
	{
		Vector3 velocity = FakePhysics.Velocity;

		if (mWantToJump && mJumpTime >= 0.0f)
		{
			velocity.y = mMovement.jumpForce;
			mJumpTime -= Time.fixedDeltaTime;
		}

		velocity.x += mMovementInput.x * mMovement.acceleration * Time.fixedDeltaTime;
		velocity.x = Mathf.Clamp(velocity.x, -mMovement.maxSpeed, mMovement.maxSpeed);
		if (velocity.x > 0.0f)
			mIsFlipped = false;
		if (velocity.x < 0.0f)
			mIsFlipped = true;
		FakePhysics.Velocity = velocity;
		
		for (int i = 0; i < mAbilities.Length; ++i)
		{
			mAbilities[i].Ability.FixedUpdateAbility();
		}
	}
}