using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class BSGFakePhysics : MonoBehaviour
{
	private const float GROUND_CHECK_DISTANCE = 0.01f;
	private const float STATIONARY_VELOCITY_EPSILON = 0.01f;

	private enum EAxisIndex
	{
		Horizontal = 0,
		Vertical = 1,
	}

	[System.Serializable]
	private class RaycastSettings
	{
		public int numberOfRaycasts = 4;
		public float raycastMargin = 0.01f;
	}

	[System.Serializable]
	private class Physics
	{
		public RaycastSettings horizontal;
		public RaycastSettings vertical;
		[Range(0.0f, 1.0f)] public float bounciness = 0.0f;
		public float gravityScale = 1.0f;
		[Range(0.0f, 1.0f)] public float friction = 0.5f;
	}

	[SerializeField] private Physics mPhysics;

	private int mCollisionMask = 0;
	private BoxCollider mBoxCollider;
	private bool mIsGrounded = false;
	private Vector2 mVelocity = Vector2.zero;

	public Vector2 Velocity { get { return mVelocity; } set { mVelocity = value; } }

	private void Awake()
	{
		int layer = gameObject.layer;
		for (int i = 0; i < 32; ++i)
		{
			if (UnityEngine.Physics.GetIgnoreLayerCollision(layer, i) == false)
			{
				mCollisionMask = mCollisionMask | (1 << i);
			}
		}

		mBoxCollider = GetComponent<BoxCollider>();
	}

	private void FixedUpdate()
	{
		if (mIsGrounded == false)
			mVelocity.y -= 20.0f * mPhysics.gravityScale * Time.fixedDeltaTime;
		DoRaycasts(Vector2.right, mPhysics.horizontal, EAxisIndex.Horizontal);
		DoRaycasts(Vector2.up, mPhysics.vertical, EAxisIndex.Vertical);
		DoGroundCheck();
	}

	private void DoRaycasts(Vector2 aDirection, RaycastSettings aRaycastSettings, EAxisIndex aAxisIndex)
	{
		int axisIndex = (int)aAxisIndex;
		float distanceThisFrame = mVelocity[axisIndex] * Time.fixedDeltaTime;
		float offset = mBoxCollider.size[1 - axisIndex] * 0.5f - aRaycastSettings.raycastMargin;
		Vector3 offsetVector0 = Vector3.zero;
		Vector3 offsetVector1 = Vector3.zero;
		offsetVector0[1 - axisIndex] = -offset;
		offsetVector1[1 - axisIndex] = offset;
		Vector3 start = transform.position + offsetVector0;
		Vector3 end = transform.position + offsetVector1;
		float absDistanceThisFrame = Mathf.Abs(distanceThisFrame);
		float distanceToMove = absDistanceThisFrame;
		float rayLength = mBoxCollider.size[axisIndex] * 0.5f + absDistanceThisFrame;
		Vector3 rayDirection = aDirection.ToVec3() * Mathf.Sign(distanceThisFrame);
		for (int i = 0; i < aRaycastSettings.numberOfRaycasts; ++i)
		{
			Vector3 rayOrigin = Vector3.Lerp(start, end, i / (aRaycastSettings.numberOfRaycasts - 1.0f));
			Ray ray = new Ray(rayOrigin, rayDirection);
			RaycastHit hitInfo;
			Debug.DrawLine(rayOrigin, rayOrigin + rayDirection * rayLength, Color.red, 0.0f);
			if (UnityEngine.Physics.Raycast(ray, out hitInfo, rayLength, mCollisionMask))
			{
				float distance = Mathf.Abs(start[axisIndex] - hitInfo.point[axisIndex]) - (mBoxCollider.size[axisIndex] * 0.5f);
				if (distance < distanceToMove)
				{
					distanceToMove = distance;
					mVelocity[axisIndex] = -mVelocity[axisIndex] * mPhysics.bounciness;
					mVelocity *= mPhysics.friction;
					if (Mathf.Abs(mVelocity.x) < STATIONARY_VELOCITY_EPSILON)
						mVelocity.x = 0.0f;
					if (Mathf.Abs(mVelocity.y) < STATIONARY_VELOCITY_EPSILON)
						mVelocity.y = 0.0f;
				}
			}
		}
		transform.position += rayDirection * distanceToMove;
	}

	private void DoGroundCheck()
	{
		Vector3 start = transform.position + new Vector3(-mBoxCollider.size.x * 0.5f + mPhysics.vertical.raycastMargin, 0.0f, 0.0f);
		Vector3 end = transform.position + new Vector3(mBoxCollider.size.x * 0.5f - mPhysics.vertical.raycastMargin, 0.0f, 0.0f);
		float rayLength = mBoxCollider.size.y * 0.5f + GROUND_CHECK_DISTANCE;
		for (int i = 0; i < mPhysics.vertical.numberOfRaycasts; ++i)
		{
			Vector3 rayOrigin = Vector3.Lerp(start, end, i / (mPhysics.vertical.numberOfRaycasts - 1.0f));
			Vector3 rayDirection = Vector3.down;
			Ray ray = new Ray(rayOrigin, rayDirection);
			RaycastHit hitInfo;
			Debug.DrawLine(rayOrigin, rayOrigin + rayDirection * rayLength, Color.red, 0.1f);
			if (UnityEngine.Physics.Raycast(ray, out hitInfo, rayLength, mCollisionMask))
			{
				mIsGrounded = true;
				break;
			}
		}
		mIsGrounded = false;
	}
	
	public void AddForce(Vector2 aForce)
	{
		mVelocity += aForce;
	}

	public void AddForce(Vector2 aNormalDirection, float aForce)
	{
		mVelocity += aNormalDirection * aForce;
	}

	public void AddExplosionForce(float aForce, Vector2 aExplosionSource, float aExplosionRadius, float aUpModifier = 1.0f)
	{
		Vector2 delta = transform.position.ToVec2() - aExplosionSource;
		float distance = delta.magnitude;
		if (distance <= aExplosionRadius)
		{
			float force = 1.0f - (distance / aExplosionRadius);
			mVelocity += delta.normalized * force + Vector2.up * aUpModifier;
		}
	}
}
