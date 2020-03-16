using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Character), typeof(Animator))]
public class CharacterAnimatorController : MonoBehaviour
{
	private Character mOwner;
	public Character Owner { get { return mOwner; } }

	private Animator mAnimator;

	[SerializeField] [DisplayAs("Idle Threshold")] private float mIdleThreshold;
	[SerializeField] [DisplayAs("Transform to Flip")] private Transform mFlipTransform;

	private Vector3 mCachedFlipScale;
	private BlockAbility mCachedBlockAbility;
	private GrabAbility mCachedGrabAbility;

	private void Awake()
	{
		mOwner = GetComponent<Character>();
		mAnimator = GetComponent<Animator>();

		if (mFlipTransform != null)
		{
			mCachedFlipScale = mFlipTransform.localScale;
		}
		mCachedBlockAbility = Owner.AbilityController.GetAbility<BlockAbility>();
		mCachedGrabAbility = Owner.AbilityController.GetAbility<GrabAbility>();
	}

	private void FixedUpdate()
	{
		UpdateAnimator();
		UpdateTransforms();
	}

	private void UpdateAnimator()
	{
		if (mAnimator != null && mAnimator.runtimeAnimatorController != null)
		{
			mAnimator.SetFloat("Player_XVel", Owner.FakePhysics.Velocity.x);
			mAnimator.SetFloat("Player_YVel", Owner.FakePhysics.Velocity.y);
			mAnimator.SetBool("Grounded", Owner.FakePhysics.IsGrounded);
			mAnimator.SetBool("Throw", mCachedGrabAbility != null ? mCachedGrabAbility.HasPostThrowEffect : false);
			mAnimator.SetBool("Hurt", Owner.MovementController.IsStunned);
			mAnimator.SetBool("Block", mCachedBlockAbility != null ? mCachedBlockAbility.IsRunning : false);
			mAnimator.SetBool("Parry", false);
			mAnimator.SetBool("PickUp", mCachedGrabAbility != null ? mCachedGrabAbility.IsGrabbing : false);
			mAnimator.SetBool("Bomb", mCachedGrabAbility != null ? mCachedGrabAbility.GrabbedObject != null : false);
			mAnimator.SetBool("Idle", Mathf.Abs(Owner.FakePhysics.Velocity.x) <= mIdleThreshold);
			mAnimator.SetBool("JumpSquat", Owner.MovementController.IsJumpSquatting);
			mAnimator.SetBool("DoubleJump", false);
		}
	}

	private void UpdateTransforms()
	{
		if (mFlipTransform != null)
		{
			mFlipTransform.localScale = Vector3.Scale(mCachedFlipScale, new Vector3(Owner.MovementController.IsFacingRight ? 1.0f : -1.0f, 1.0f, 1.0f));
		}
	}
}
