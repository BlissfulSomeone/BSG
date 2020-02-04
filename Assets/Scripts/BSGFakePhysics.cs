using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class BSGFakePhysics : MonoBehaviour
{
	private const float GROUND_CHECK_DISTANCE = 0.01f;
	private const float STATIONARY_VELOCITY_EPSILON = 0.1f;

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
		public float friction = 10.0f;
		[Range(0.0f, 1.0f)] public float airControl = 0.0f;
	}

	public delegate void OnImpactHandler();
	public OnImpactHandler OnImpact;

	[SerializeField] private Physics mPhysics;
	
	private int mCollisionMask = 0;
	private BoxCollider mBoxCollider;
	private bool mIsGrounded = false;
	private Vector2 mVelocity = Vector2.zero;

	public bool IsGrounded { get { return mIsGrounded; } }
	public Vector2 Velocity { get { return mVelocity; } set { mVelocity = value; } }

	private void Awake()
	{
		int layer = gameObject.layer;
		for (int i = 0; i < 16; ++i)
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
		if (mVelocity.x > 0.0f)
			mVelocity.x = Mathf.Max(mVelocity.x - mPhysics.friction * (mIsGrounded == true ? 1.0f : mPhysics.airControl) * Time.fixedDeltaTime, 0.0f);
		if (mVelocity.x < 0.0f)
			mVelocity.x = Mathf.Min(mVelocity.x + mPhysics.friction * (mIsGrounded == true ? 1.0f : mPhysics.airControl) * Time.fixedDeltaTime, 0.0f);
		if (mIsGrounded == false)
			mVelocity.y -= 20.0f * mPhysics.gravityScale * Time.fixedDeltaTime;
		bool hit = false;
		if (DoRaycasts(Vector2.right, mPhysics.horizontal, EAxisIndex.Horizontal))
			hit = true;
		if (DoRaycasts(Vector2.up, mPhysics.vertical, EAxisIndex.Vertical))
			hit = true;
		if (hit)
			OnImpact?.Invoke();
		DoGroundCheck();
	}

	private bool DoRaycasts(Vector2 aDirection, RaycastSettings aRaycastSettings, EAxisIndex aAxisIndex)
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
		bool hit = false;
		for (int i = 0; i < aRaycastSettings.numberOfRaycasts; ++i)
		{
			Vector3 rayOrigin = Vector3.Lerp(start, end, i / (aRaycastSettings.numberOfRaycasts - 1.0f));
			Ray ray = new Ray(rayOrigin, rayDirection);
			RaycastHit hitInfo;
			if (UnityEngine.Physics.Raycast(ray, out hitInfo, rayLength, mCollisionMask))
			{
				float distance = Mathf.Abs(start[axisIndex] - hitInfo.point[axisIndex]) - (mBoxCollider.size[axisIndex] * 0.5f);
				if (distance < distanceToMove)
				{
					hit = true;
					distanceToMove = distance;
					mVelocity[axisIndex] = -mVelocity[axisIndex] * mPhysics.bounciness;
					if (mVelocity[axisIndex] > 0.0f)
						mVelocity[axisIndex] = Mathf.Max(mVelocity[axisIndex] - STATIONARY_VELOCITY_EPSILON, 0.0f);
					if (mVelocity[axisIndex] < 0.0f)
						mVelocity[axisIndex] = Mathf.Min(mVelocity[axisIndex] + STATIONARY_VELOCITY_EPSILON, 0.0f);
				}
			}
		}
		transform.position += rayDirection * distanceToMove;
		return hit;
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
				return;
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
			float falloff = 1.0f - (distance / aExplosionRadius);
			float force = 10.0f * falloff;
			mVelocity += delta.normalized * force + Vector2.up * force * aUpModifier;
		}
	}
}
