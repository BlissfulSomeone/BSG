using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Character))]
public class CharacterMovementController : MonoBehaviour
{
	[System.Serializable]
	private struct Movement
	{
		public float acceleration;
		public float maxMoveSpeed;
		public float maxFallSpeed;
		public float jumpSquat;
		public float jumpForce;
		public float jumpTime;
		public int maxJumps;
	}

	[Header("Input Names")]
	[SerializeField] [DisplayAs("Horizontal Movement Input Name")] private string mHorizontalMovementInputName;
	[SerializeField] [DisplayAs("Jump Input Name")] private string mJumpInputName;

	[Header("Movement")]
	[SerializeField] [DisplayAs("Movement Settings")] private Movement mMovement;
	[SerializeField] [DisplayAs("Stunned Character State")] private CharacterOverrides mStunnedCharacterState;
	[SerializeField] [DisplayAs("Jump Squat Character State")] private CharacterOverrides mJumpSquatCharacterState;

	private Character mOwner;
	public Character Owner { get { return mOwner; } }

	private BSGFakePhysics mFakePhysics;
	private Triggerable mTriggerable;

	public BSGFakePhysics FakePhysics { get { if (mFakePhysics == null) mFakePhysics = GetComponent<BSGFakePhysics>(); return mFakePhysics; } }
	public Triggerable Triggerable { get { if (mTriggerable == null) mTriggerable = GetComponent<Triggerable>(); return mTriggerable; } }
	
	private Vector3 mMovementInput = Vector3.zero;
	private float mJumpSquatTime = -1.0f;
	private float mJumpTime = -1.0f;
	private int mJumpsRemaining = 0;
	private bool mIsFacingRight = false;

	public bool IsFacingRight { get { return mIsFacingRight; } }

	private void Awake()
	{
		mOwner = GetComponent<Character>();
		mFakePhysics = GetComponent<BSGFakePhysics>();
		mTriggerable = GetComponent<Triggerable>();

		mTriggerable.OnPreTimedEvent += OnPreTimedEvent;
		mTriggerable.OnPostTimedEvent += OnPostTimedEvent;
	}

	private void OnDestroy()
	{
		mTriggerable.OnPreTimedEvent -= OnPreTimedEvent;
		mTriggerable.OnPostTimedEvent -= OnPostTimedEvent;
	}

	private void OnPreTimedEvent()
	{
		Owner.OverrideController.ApplyCharacterOverrides(mStunnedCharacterState);
	}

	private void OnPostTimedEvent()
	{
		Owner.OverrideController.RemoveCharacterOverrides(mStunnedCharacterState);
	}

	private void Update()
	{
		UpdateMovement();
		UpdateJump();
	}

	private void UpdateMovement()
	{
		mMovementInput.x = Input.GetAxisRaw(mHorizontalMovementInputName);
	}

	private void UpdateJump()
	{
		if (FakePhysics.IsGrounded && !FakePhysics.WasGrounded)
		{
			mJumpsRemaining = mMovement.maxJumps;
		}
		if (Input.GetButtonDown(mJumpInputName) &&
			!Owner.OverrideController.IsCharacterOverrideApplied(mJumpSquatCharacterState) &&
			(FakePhysics.IsGrounded || mJumpsRemaining > 0))
		{
			Owner.OverrideController.ApplyCharacterOverrides(mJumpSquatCharacterState, mMovement.jumpSquat, OnPerformJump);
			FakePhysics.Velocity = Vector2.zero;
		}

		if (mJumpTime >= 0.0f)
		{
			mJumpTime -= Time.deltaTime;
			if (!Input.GetButton(mJumpInputName) && mJumpTime > 0.0f)
			{
				mJumpTime = 0.0f;
			}
		}
	}

	private void OnPerformJump()
	{
		mJumpTime = mMovement.jumpTime;
		--mJumpsRemaining;
	}

	private void FixedUpdate()
	{
		FixedUpdateMovement();
		FixedUpdateClampSpeed();
		FixedUpdateFlipping();
	}

	private void FixedUpdateMovement()
	{
		Vector3 velocity = FakePhysics.Velocity;
		if (Owner.OverrideController.IsOverrideEnabled(Owner.OverrideController.CurrentOverrides.CanRecieveInput))
		{
			if (mJumpTime >= 0.0f)
			{
				velocity.y = mMovement.jumpForce;
			}
			velocity.x += mMovementInput.x * mMovement.acceleration * Time.fixedDeltaTime;
		}
		FakePhysics.Velocity = velocity;
	}

	private void FixedUpdateClampSpeed()
	{
		Vector3 velocity = FakePhysics.Velocity;
		velocity.x = Owner.OverrideController.IsOverrideEnabled(Owner.OverrideController.CurrentOverrides.ClampMaxSpeed) ? Mathf.Clamp(velocity.x, -mMovement.maxMoveSpeed, mMovement.maxMoveSpeed) : velocity.x;
		velocity.y = Owner.OverrideController.IsOverrideEnabled(Owner.OverrideController.CurrentOverrides.ClampMaxSpeed) ? Mathf.Clamp(velocity.y, -mMovement.maxFallSpeed, mMovement.maxFallSpeed) : velocity.y;
		FakePhysics.Velocity = velocity;
	}

	private void FixedUpdateFlipping()
	{
		if (FakePhysics.Velocity.x > 0.0f)
			mIsFacingRight = false;
		if (FakePhysics.Velocity.x < 0.0f)
			mIsFacingRight = true;
		GetComponent<SpriteRenderer>().flipX = mIsFacingRight;
	}
}
