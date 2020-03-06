using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Character), typeof(Animator))]
public class CharacterAnimatorController : MonoBehaviour
{
	private Character mOwner;
	public Character Owner { get { return mOwner; } }

	private Animator mAnimator;

	[SerializeField] private float mIdleThreshold;

	private void Awake()
	{
		mOwner = GetComponent<Character>();
		mAnimator = GetComponent<Animator>();
	}

	private void FixedUpdate()
	{
		if (mAnimator == null || mAnimator.runtimeAnimatorController == null)
			return;

		mAnimator.SetFloat("Player_XVel", mOwner.FakePhysics.Velocity.x);
		mAnimator.SetFloat("Player_YVel", mOwner.FakePhysics.Velocity.y);
		mAnimator.SetBool("Grounded", mOwner.FakePhysics.IsGrounded);
		mAnimator.SetBool("Throw", false);
		mAnimator.SetBool("Hurt", false);
		mAnimator.SetBool("Block", false);
		mAnimator.SetBool("Parry", false);
		mAnimator.SetBool("PickUp", false);
		mAnimator.SetBool("Bomb", false);
		mAnimator.SetBool("Idle", Mathf.Abs(mOwner.FakePhysics.Velocity.x) <= mIdleThreshold);
		mAnimator.SetBool("Landed", false);
	}
}
