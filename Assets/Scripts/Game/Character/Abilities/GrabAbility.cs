using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabAbility : Ability
{
	[SerializeField] [DisplayAs("Grab Socket")] private Vector3 mGrabSocket;
	[SerializeField] [DisplayAs("Grab Radius")] private float mGrabRadius;
	[SerializeField] [DisplayAs("Throw Force")] private float mThrowForce;
	[SerializeField] [DisplayAs("Knockback Force")] private float mKnockbackForce;
	[SerializeField] [DisplayAs("Has Knockback On Ground")] private bool mHasKnockbackOnGround;
	[SerializeField] [DisplayAs("Throw Trigger Explosion")] private bool mThrowTriggerExplosion;
	[SerializeField] [DisplayAs("Look-Ahead Distance")] private float mLookAheadDistance;
	[SerializeField] [DisplayAs("Throw Arrow Texture")] private Texture2D mThrowArrowTexture;
	[SerializeField] [DisplayAs("Character Post Throw State")] private CharacterOverrides mCharacterPostThrowState;
	[SerializeField] [DisplayAs("Character Post Throw State Duration")] private float mCharacterPostThrowStateDuration;
	private BSGFakePhysics mGrabbedObject;
	private bool mIsThrowing;
	private Vector2 mThrowDirection;
	private float mDefaultAirControl;
	private System.Guid mCameraOffsetGuid;

	public override void Initialize(Character owner)
	{
		base.Initialize(owner);

		mIsPassiveUpdate = true;

		mDefaultAirControl = Owner.FakePhysics.PhysicsSettings.airControl;
		mCameraOffsetGuid = System.Guid.NewGuid();
	}

	protected override void StartAbility_Internal()
	{
		if (TryPickupObject())
		{
			mIsThrowing = true;
		}
	}

	protected override void StopAbility_Internal()
	{
		if (TryThrowObject())
		{
			mIsThrowing = false;
			GameController.Instance.CameraControllerInstance.RemoveAdditiveViewInfo(mCameraOffsetGuid);
		}
	}

	protected override void UpdateAbility_Internal()
	{
		if (mIsThrowing)
		{
			float horizontalAxis = Input.GetAxisRaw("Horizontal");
			float verticalAxis = Input.GetAxisRaw("Vertical");
			if (horizontalAxis == 0.0f && verticalAxis == 0.0f)
			{
				horizontalAxis = Owner.IsFlipped ? -1.0f : 1.0f;
			}
			mThrowDirection = new Vector2(horizontalAxis, verticalAxis).normalized;
			GameController.Instance.CameraControllerInstance.PushAdditiveViewInfo(new CameraController.ViewInfo { position = mThrowDirection * mLookAheadDistance }, mCameraOffsetGuid);
		}
	}

	protected override void FixedUpdateAbility_Internal()
	{
		if (mGrabbedObject != null)
		{
			mGrabbedObject.transform.position = GetCurrentGrabSocketPosition(Space.World);
		}
		else
		{
			mIsThrowing = false;
			GameController.Instance.CameraControllerInstance.RemoveAdditiveViewInfo(mCameraOffsetGuid);
		}
	}

	private Vector3 GetCurrentGrabSocketPosition(Space space)
	{
		Vector3 offset = mGrabSocket;
		offset.x *= (Owner && Owner.IsFlipped ? -1.0f : 1.0f);
		if (space == Space.World)
		{
			offset += transform.position;
		}
		return offset;
	}

	private bool TryPickupObject()
	{
		Collider[] colliders = Physics.OverlapSphere(GetCurrentGrabSocketPosition(Space.World), mGrabRadius);
		for (int i = 0; i < colliders.Length; ++i)
		{
			if (colliders[i].gameObject == Owner.gameObject)
				continue;

			BSGFakePhysics fakePhysics = colliders[i].gameObject.GetComponent<BSGFakePhysics>();
			if (fakePhysics == null || !fakePhysics.enabled)
				continue;

			mGrabbedObject = fakePhysics;
			fakePhysics.enabled = false;
			fakePhysics.transform.localScale = Vector3.one * 0.5f;

			// Do type-specific actions here.
			Bomb bomb = fakePhysics.gameObject.GetComponent<Bomb>();
			if (bomb != null)
			{
				if (mThrowTriggerExplosion == true)
				{
					bomb.CanBeTriggerByImpact = mThrowTriggerExplosion;
				}
			}
			Missile missile = fakePhysics.gameObject.GetComponent<Missile>();
			if (missile != null)
			{
				missile.SetState(Missile.EMissileState.Grabbed);
			}

			return true;
		}

		return false;
	}

	private bool TryThrowObject()
	{
		if (mGrabbedObject != null)
		{
			// Do type-specific actions here.
			Missile missile = mGrabbedObject.gameObject.GetComponent<Missile>();
			if (missile != null)
			{
				missile.SetState(Missile.EMissileState.Flying);
			}
		}

		if (mGrabbedObject == null)
			return false;
			
		Vector2 force = mThrowDirection * mThrowForce;

		mGrabbedObject.enabled = true;
		mGrabbedObject.Velocity = force;
		mGrabbedObject.transform.localScale = Vector3.one;
		mGrabbedObject = null;

		if (!Owner.FakePhysics.IsGrounded || mHasKnockbackOnGround)
		{
			Owner.FakePhysics.Velocity = -mThrowDirection * mKnockbackForce;
			Owner.ApplyCharacterOverridesOverTime(mCharacterPostThrowState, mCharacterPostThrowStateDuration);
		}

		return true;
	}

	private void OnGUI()
	{
		if (mIsThrowing)
		{
			Vector3 screenPosition = Camera.main.WorldToScreenPoint(GetCurrentGrabSocketPosition(Space.World));
			screenPosition.y = Screen.height - screenPosition.y;
			float angle = Mathf.Atan2(-mThrowDirection.y, mThrowDirection.x) * Mathf.Rad2Deg;
			GUIUtility.RotateAroundPivot(angle, screenPosition);
			GUI.DrawTexture(new Rect(screenPosition.x, screenPosition.y - mThrowArrowTexture.height / 2, mThrowArrowTexture.width, mThrowArrowTexture.height), mThrowArrowTexture);
		}
	}
}
