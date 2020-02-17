using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabAbility : Ability
{
	[SerializeField] private Vector3 mGrabSocket;
	[SerializeField] private float mGrabRadius;
	[SerializeField] private float mThrowForce;
	[SerializeField] private float mKnockbackForce;
	[SerializeField] private bool mThrowTriggerExplosion;
	[SerializeField] private float mLookAheadDistance;
	[SerializeField] private Texture2D mThrowArrowTexture;
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
		if (mGrabbedObject == null)
		{
			TryPickupObject();
		}
		else
		{
			mIsThrowing = true;
			Owner.CanMove = false;
			Owner.FakePhysics.SetAirControl(0.0f);
		}
	}

	protected override void StopAbility_Internal()
	{
		if (mIsThrowing)
		{
			mIsThrowing = false;
			ThrowObject();
			Owner.CanMove = true;
			Owner.FakePhysics.SetAirControl(mDefaultAirControl);
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

	private void TryPickupObject()
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

			break;
		}
	}

	private void ThrowObject()
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
			return;
			
		Vector2 force = mThrowDirection * mThrowForce;

		mGrabbedObject.enabled = true;
		mGrabbedObject.Velocity = force;
		mGrabbedObject.transform.localScale = Vector3.one;
		mGrabbedObject = null;

		Owner.FakePhysics.AddForce(-mThrowDirection * mKnockbackForce);
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
