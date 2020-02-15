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
	[SerializeField] private bool mThrowOnRelease;
    private BSGFakePhysics mGrabbedObject;

	public override void Initialize(Character owner)
	{
		base.Initialize(owner);

		mIsPassiveUpdate = true;
	}

	protected override void StartAbility_Internal()
	{
		if (mThrowOnRelease || mGrabbedObject == null)
		{
			TryPickupObject();
		}
		else
		{
			ThrowObject();
		}
	}

	protected override void StopAbility_Internal()
	{
		if (mThrowOnRelease)
		{
			ThrowObject();
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
			if (fakePhysics == null)
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

			break;
		}
	}

	private void ThrowObject()
	{
		if (mGrabbedObject == null)
			return;

		float horizontalAxis = Input.GetAxisRaw("Horizontal");
		float verticalAxis = Input.GetAxisRaw("Vertical");
		if (horizontalAxis == 0.0f && verticalAxis == 0.0f)
		{
			horizontalAxis = Owner.IsFlipped ? -1.0f : 1.0f;
		}
		Vector2 throwForward = new Vector2(horizontalAxis, verticalAxis).normalized;
		Vector2 force = throwForward * mThrowForce + Vector2.up;

		mGrabbedObject.enabled = true;
		mGrabbedObject.Velocity = force;
		mGrabbedObject.transform.localScale = Vector3.one;
		mGrabbedObject = null;

		Owner.FakePhysics.AddForce(-throwForward * mKnockbackForce);
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(GetCurrentGrabSocketPosition(Space.World), mGrabRadius);

		Vector3 pos = GetCurrentGrabSocketPosition(Space.World);
		float radians = System.DateTime.Now.Second + System.DateTime.Now.Millisecond / 1000.0f;
		float forceX = Mathf.Cos(radians) * mThrowForce;
		float forceY = Mathf.Sin(radians) * mThrowForce;
		Vector3 vel = new Vector2(forceX, forceY).ToVec3();
		const float DELTA_TIME = 0.1f;
		for (int i = 0; i < 20; ++i)
		{
			Gizmos.color = Color.Lerp(Color.red, new Color(1.0f, 0.0f, 0.0f, 0.0f), (float)i / 19);
			Vector3 movement = vel * DELTA_TIME;
			Gizmos.DrawLine(pos, pos + movement);
			pos += movement;
			vel.y -= 20.0f * DELTA_TIME;
		}
	}
}
