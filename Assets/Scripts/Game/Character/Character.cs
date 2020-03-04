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
		public int maxJumps;
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
	private Animator mAnimator;

	public Renderer Renderer { get { if (mRenderer == null) mRenderer = GetComponent<Renderer>(); return mRenderer; } }
	public BSGFakePhysics FakePhysics { get { if (mFakePhysics == null) mFakePhysics = GetComponent<BSGFakePhysics>(); return mFakePhysics; } }

	[Header("Generic Input Names")]
	[SerializeField] [DisplayAs("Horizontal Movement Input Name")] private string mHorizontalMovementInputName;
	[SerializeField] [DisplayAs("Jump Input Name")] private string mJumpInputName;

	[Header("Movement")]
	[SerializeField] [DisplayAs("Movement Settings")] private Movement mMovement;
	[SerializeField] [DisplayAs("Stunned Character State")] private CharacterOverrides mStunnedCharacterState;

	[Header("Stats")]
	[SerializeField] [DisplayAs("Max Health")] private float mMaxHealth;
	[SerializeField] [DisplayAs("Abilities")] private AbilitySet[] mAbilities;

	// Movement stuff
	private Vector3 mMovementInput = Vector3.zero;
	private bool mWantToJump = false;
	private float mJumpTime = -1.0f;
	private int mJumpsRemaining = 0;
	private bool mIsFlipped = false;
	private CharacterOverrides mCurrentOverrides = new CharacterOverrides();
	private List<CharacterOverrides> mOverridesStack = new List<CharacterOverrides>();
	
    public Vector3 Velocity { get { return FakePhysics.Velocity; } }
	public bool IsFlipped { get { return mIsFlipped; } }
	public CharacterOverrides CurrentOverrides { get { return mCurrentOverrides; } }

	// Health stuff
	private float mHealth;

	public float Health { get { return mHealth; } }
	public float MaxHealth { get { return mMaxHealth; } }
	
    private void Awake()
	{
		mRenderer = GetComponent<Renderer>();
		mFakePhysics = GetComponent<BSGFakePhysics>();
		mBoxCollider = GetComponent<BoxCollider>();
		mTriggerable = GetComponent<Triggerable>();
		mAnimator = GetComponent<Animator>();

		mTriggerable.OnTriggered += OnTriggered;
		mTriggerable.OnPreTimedEvent += OnPreTimedEvent;
		mTriggerable.OnPostTimedEvent += OnPostTimedEvent;

		mHealth = mMaxHealth;
		mJumpsRemaining = mMovement.maxJumps;

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
		if (!explosionInstance.ExplosionData.Friendly && IsOverrideEnabled(CurrentOverrides.CanBeHurt))
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
		ApplyCharacterOverrides(mStunnedCharacterState);
	}

	private void OnPostTimedEvent()
	{
		RemoveCharacterOverrides(mStunnedCharacterState);
	}

	private void Update()
	{
		UpdateInput();
		UpdateJump();
		UpdateAbilities();
	}

	private void UpdateInput()
	{
		mMovementInput.x = Input.GetAxisRaw(mHorizontalMovementInputName);
		mWantToJump = Input.GetButtonDown(mJumpInputName);
	}

	private void UpdateJump()
	{
		if (FakePhysics.IsGrounded && !FakePhysics.WasGrounded)
		{
			mJumpsRemaining = mMovement.maxJumps;
		}
		if (mWantToJump && mJumpsRemaining > 0)
		{
			mJumpTime = mMovement.jumpTime;
			--mJumpsRemaining;
		}
		else if (Input.GetButtonUp(mJumpInputName))
		{
			mJumpTime = -1.0f;
		}
	}

	private void UpdateAbilities()
	{
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

	private void FixedUpdate()
	{
		FixedUpdateOverrides();
		FixedUpdateJump();
		FixedUpdateMovement();
		FixedUpdateFlipping();
		FixedUpdateAbilities();
		FixedUpdateAnimator();
	}

	private void FixedUpdateOverrides()
	{
		mCurrentOverrides.Reset(ECharacterOverrideState.Enable);
		foreach (CharacterOverrides overrides in mOverridesStack)
		{
			mCurrentOverrides = CharacterOverrides.TransferOverrides(mCurrentOverrides, overrides);
		}

		mTriggerable.HasPhysics = IsOverrideEnabled(CurrentOverrides.ReceiveKnockbackFromExplosions);
		if (IsOverrideEnabled(CurrentOverrides.AirControl))
		{
			mFakePhysics.SetAirControl(CurrentOverrides.AirControl.Value);
		}
		else
		{
			mFakePhysics.ResetAirControl();
		}
	}

	private void FixedUpdateJump()
	{
		Vector3 velocity = FakePhysics.Velocity;
		if (IsOverrideEnabled(CurrentOverrides.CanRecieveInput))
		{
			if (mJumpTime >= 0.0f)
			{
				velocity.y = mMovement.jumpForce;
				mJumpTime -= Time.fixedDeltaTime;
			}
			velocity.x += mMovementInput.x * mMovement.acceleration * Time.fixedDeltaTime;
		}
		FakePhysics.Velocity = velocity;
	}

	private void FixedUpdateMovement()
	{
		Vector3 velocity = FakePhysics.Velocity;
		velocity.x = IsOverrideEnabled(CurrentOverrides.ClampMaxSpeed) ? Mathf.Clamp(velocity.x, -mMovement.maxSpeed, mMovement.maxSpeed) : velocity.x;
		FakePhysics.Velocity = velocity;
	}

	private void FixedUpdateFlipping()
	{
		if (FakePhysics.Velocity.x > 0.0f)
			mIsFlipped = false;
		if (FakePhysics.Velocity.x < 0.0f)
			mIsFlipped = true;
		GetComponent<SpriteRenderer>().flipX = mIsFlipped;
	}

	private void FixedUpdateAbilities()
	{
		for (int i = 0; i < mAbilities.Length; ++i)
		{
			mAbilities[i].Ability.FixedUpdateAbility();
		}
	}

	private void FixedUpdateAnimator()
	{
		if (mAnimator == null)
			return;

		mAnimator.SetFloat("Player_XVel", FakePhysics.Velocity.x);
		mAnimator.SetFloat("Player_YVel", FakePhysics.Velocity.y);
		mAnimator.SetBool("Grounded", FakePhysics.IsGrounded);
		mAnimator.SetBool("Throw", false);
		mAnimator.SetBool("Hurt", false);
		mAnimator.SetBool("Block", false);
		mAnimator.SetBool("Parry", false);
		mAnimator.SetBool("PickUp", false);
		mAnimator.SetBool("Bomb", false);
		mAnimator.SetBool("Idle", false);
		mAnimator.SetBool("Landed", false);
	}

	public bool IsOverrideEnabled(ECharacterOverrideState overrideState)
	{
		return overrideState == ECharacterOverrideState.Enable;
	}

	public bool IsOverrideEnabled(FloatOverride floatOverride)
	{
		return floatOverride.State == ECharacterOverrideState.Enable;
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

	public void ApplyCharacterOverrides(CharacterOverrides characterOverrides)
	{
		if (mOverridesStack.Contains(characterOverrides))
		{
			Debug.LogError("The same instance of character overrides cannot be applied twice.");
			return;
		}
		mOverridesStack.Add(characterOverrides);
	}

	public void RemoveCharacterOverrides(CharacterOverrides characterOverrides)
	{
		mOverridesStack.Remove(characterOverrides);
	}

	public void ApplyCharacterOverridesOverTime(CharacterOverrides characterOverrides, float duration)
	{
		StartCoroutine(Coroutine_ApplyCharacterOverrideOverTime(characterOverrides, duration));
	}

	private IEnumerator Coroutine_ApplyCharacterOverrideOverTime(CharacterOverrides characterOverrides, float duration)
	{
		ApplyCharacterOverrides(characterOverrides);
		yield return new WaitForSeconds(duration);
		RemoveCharacterOverrides(characterOverrides);
	}
}
