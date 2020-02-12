using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabAbility : Ability
{
	[SerializeField] private Vector3 mGrabSocket;
	[SerializeField] private float mGrabRadius;
	[SerializeField] private float mThrowAngle;
	[SerializeField] private float mThrowForce;
	[SerializeField] private bool mThrowTriggerExplosion;
    private BSGFakePhysics mGrabbedObject;

	protected override void StartAbility_Internal()
	{
		Collider[] colliders = Physics.OverlapSphere(GetCurrentGrabSocketPosition(Space.World), mGrabRadius);
		for (int i = 0; i < colliders.Length; ++i)
		{
			if (colliders[i].gameObject == Owner.gameObject)
				continue;

			BSGFakePhysics fakePhysics = colliders[i].gameObject.GetComponent<BSGFakePhysics>();
			if (fakePhysics != null)
			{
				mGrabbedObject = fakePhysics;
				fakePhysics.enabled = false;
				fakePhysics.transform.localScale = Vector3.one * 0.5f;

                Bomb bomb = fakePhysics.gameObject.GetComponent<Bomb>();
                if(bomb !=null)
                {
                    if (mThrowTriggerExplosion == true)
                    {
                        bomb.CanBeTriggerByImpact = mThrowTriggerExplosion;
                    }
                    
                }

                break;
			}
		}
	}

	protected override void StopAbility_Internal()
	{
		if (mGrabbedObject != null)
		{
			float radians = mThrowAngle * Mathf.Deg2Rad;
			float forceX = Mathf.Cos(radians) * mThrowForce;
			float forceY = Mathf.Sin(radians) * mThrowForce;
			Vector2 force = new Vector2(Owner.IsFlipped ? -forceX : forceX, forceY);

			mGrabbedObject.enabled = true;
			mGrabbedObject.Velocity = force;
			mGrabbedObject.transform.localScale = Vector3.one;
			mGrabbedObject = null;
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

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(GetCurrentGrabSocketPosition(Space.World), mGrabRadius);

		Vector3 pos = GetCurrentGrabSocketPosition(Space.World);
		float radians = mThrowAngle * Mathf.Deg2Rad;
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
