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
	private Vector3 mMovementInput = Vector3.zero;
	private bool mWantToJump = false;
	private float mJumpTime = -1.0f;
	private bool mIsFlipped = false;
	private CharacterOverrides mCurrentOverrides = new CharacterOverrides();
	private List<CharacterOverrides> mOverridesStack = new List<CharacterOverrides>();
	
    public Vector3 Velocity { get { return FakePhysics.Velocity; } }
	public bool IsFlipped { get { return mIsFlipped; } }
	public CharacterOverrides CurrentOverrides { get { return mCurrentOverrides; } }

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
		//CanMove = false;
	}

	private void OnPostTimedEvent()
	{
		//CanMove = true;
	}

	private void Update()
	{
		UpdateOverrides();
		UpdateInput();
		UpdateJump();
		UpdateAbilities();
	}

	private void UpdateOverrides()
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

	private void UpdateInput()
	{
		mMovementInput.x = Input.GetAxisRaw(mHorizontalMovementInputName);
		mWantToJump = Input.GetButton(mJumpInputName);
	}

	private void UpdateJump()
	{
		if (FakePhysics.IsGrounded && mWantToJump)
		{
			mJumpTime = mMovement.jumpTime;
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
		Vector3 velocity = FakePhysics.Velocity;

		if (IsOverrideEnabled(CurrentOverrides.CanRecieveInput))
		{
			if (mWantToJump && mJumpTime >= 0.0f)
			{
				velocity.y = mMovement.jumpForce;
				mJumpTime -= Time.fixedDeltaTime;
			}
			velocity.x += mMovementInput.x * mMovement.acceleration * Time.fixedDeltaTime;
		}
		
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
