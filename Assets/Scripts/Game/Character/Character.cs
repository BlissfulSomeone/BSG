using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BSGFakePhysics), typeof(BoxCollider), typeof(Triggerable))]
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

	// Movement stuff
	private bool mCanMove = true;
	private Vector3 mMovementInput = Vector3.zero;
	private bool mWantToJump = false;
	private float mJumpTime = -1.0f;
	private bool mIsFlipped = false;
	
    public Vector3 Velocity { get { return FakePhysics.Velocity; } }
	public bool IsFlipped { get { return mIsFlipped; } }
	public bool CanMove { get { return mCanMove; } set { mCanMove = value; } }

	// Health stuff
	private float mHealth;
	private bool mIsInvulnerable = false;

	public float Health { get { return mHealth; } }
	public float MaxHealth { get { return mMaxHealth; } }
	public bool IsInvulnerable { get { return mIsInvulnerable; } set { mIsInvulnerable = value; } }
	
    private void Awake()
	{
		mRenderer = GetComponent<Renderer>();
		mFakePhysics = GetComponent<BSGFakePhysics>();
		mBoxCollider = GetComponent<BoxCollider>();
		mTriggerable = GetComponent<Triggerable>();
		mTriggerable.OnTriggered += OnTriggered;
		mTriggerable.OnPreTimedEvent += OnPreTimedEvent;
		mTriggerable.OnPostTimedEvent += OnPostTimedEvent;

		mHealth = mMaxHealth;

		for (int i = 0; i < mAbilities.Length; ++i)
		{
			mAbilities[i].Ability.Initialize(this);
		}
	}
	
	private void OnDestroy()
	{
		mTriggerable.OnTriggered -= OnTriggered;
	}

	private void OnTriggered(ExplosionInstance explosionInstance)
	{
		if (!explosionInstance.ExplosionData.Friendly && !IsInvulnerable)
		{
			mHealth -= explosionInstance.ExplosionData.Damage;
            if (mHealth <= 0.0f)
			{
				OnKilled?.Invoke(this);
				Destroy(gameObject);
			}
		}
	}

	private void OnPreTimedEvent()
	{
		CanMove = false;
	}

	private void OnPostTimedEvent()
	{
		CanMove = true;
	}

	private void Update()
	{
		mMovementInput.x = CanMove ? Input.GetAxisRaw(mHorizontalMovementInputName) : 0.0f;
		mWantToJump = CanMove ? Input.GetButton(mJumpInputName) : false;
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
		velocity.x = FakePhysics.IsAffectedByFriction ? Mathf.Clamp(velocity.x, -mMovement.maxSpeed, mMovement.maxSpeed) : velocity.x;
		if (velocity.x > 0.0f)
			mIsFlipped = false;
		if (velocity.x < 0.0f)
			mIsFlipped = true;
		FakePhysics.Velocity = velocity;
		GetComponent<SpriteRenderer>().flipX = mIsFlipped;
		
		for (int i = 0; i < mAbilities.Length; ++i)
		{
			mAbilities[i].Ability.FixedUpdateAbility();
		}
	}

	public T GetAbility<T>() where T : Ability
	{
		for (int i = 0; i < mAbilities.Length; ++i)
		{
			if (mAbilities[i].Ability is T)
				return (T)mAbilities[i].Ability;
		}
		return null;
	}
}
