using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
	[System.Serializable]
	private struct Physics
	{
		public int numberOfHorizontalRaycasts;
		public float horizontalRaycastMargin;
		public int numberOfVerticalRaycasts;
		public float verticalRaycastMargin;
		public float gravityScale;
	}

	[System.Serializable]
	private struct Movement
	{
		public float acceleration;
		public float maxSpeed;
		public float jumpForce;
	}

	private Rigidbody mRigidbody;
	private BoxCollider mBoxCollider;

	[SerializeField] private Physics mPhysics;
	[SerializeField] private Movement mMovement;

	[Header("Physics")]
	[SerializeField] private float mGroundCheckDistance = 0.01f;
	[SerializeField] private int mNumberOfGruondCheckRaycasts = 4;
	[SerializeField] private float mGroundCheckRaycastsMargin = 0.025f;
	
	private bool mIsGrounded = false;
	private Vector3 mMovementInput = Vector3.zero;
	private bool mWantToJump = false;
	private Vector3 mVelocity = Vector3.zero;

	private void Awake()
	{
		mRigidbody = GetComponent<Rigidbody>();
		mBoxCollider = GetComponent<BoxCollider>();
	}

	private void Update()
	{
		mMovementInput.x = 0.0f;
		mMovementInput.x += (Input.GetKey(KeyCode.RightArrow) == true) ? 1.0f : 0.0f;
		mMovementInput.x += (Input.GetKey(KeyCode.LeftArrow) == true) ? -1.0f : 0.0f;
		mWantToJump = Input.GetKey(KeyCode.UpArrow) == true || Input.GetKey(KeyCode.Z) == true;
	}

	private float Damp(float aSource, float aSmoothing, float aDeltaTime)
	{
		return aSource * Mathf.Pow(aSmoothing, aDeltaTime);
	}

	private void FixedUpdate()
	{
		mIsGrounded = IsGrounded();
		
		if (mIsGrounded == false)
		{
			mVelocity.y -= 9.82f * mPhysics.gravityScale * Time.fixedDeltaTime;
		}
		else
		{
			if (mWantToJump == true)
			{
				mVelocity.y = mMovement.jumpForce;
			}
		}
		
		if (mMovementInput.x != 1.0f && mVelocity.x > 0.0f)
			mVelocity.x = Mathf.Max(mVelocity.x - mMovement.acceleration * Time.fixedDeltaTime, 0.0f);
		if (mMovementInput.x != -1.0f && mVelocity.x < 0.0f)
			mVelocity.x = Mathf.Min(mVelocity.x + mMovement.acceleration * Time.fixedDeltaTime, 0.0f);
		mVelocity.x += mMovementInput.x * mMovement.acceleration * Time.fixedDeltaTime;
		mVelocity.x = Mathf.Clamp(mVelocity.x, -mMovement.maxSpeed, mMovement.maxSpeed);

		DoHorizontalRaycasts();
		DoVerticalRaycasts();
	}

	private bool IsGrounded()
	{
		Vector3 start = transform.position + new Vector3(-mBoxCollider.size.x * 0.5f + mGroundCheckRaycastsMargin, 0.0f, 0.0f);
		Vector3 end = transform.position + new Vector3(mBoxCollider.size.x * 0.5f - mGroundCheckRaycastsMargin, 0.0f, 0.0f);
		float rayLength = mBoxCollider.size.y * 0.5f + mGroundCheckDistance;
		for (int i = 0; i < mNumberOfGruondCheckRaycasts; ++i)
		{
			Vector3 rayOrigin = Vector3.Lerp(start, end, i / (mNumberOfGruondCheckRaycasts - 1.0f));
			Vector3 rayDirection = Vector3.down;
			Ray ray = new Ray(rayOrigin, rayDirection);
			RaycastHit hitInfo;
			Debug.DrawLine(rayOrigin, rayOrigin + rayDirection * rayLength, Color.red, 0.1f);
			if (UnityEngine.Physics.Raycast(ray, out hitInfo, rayLength, 1 << LayerMask.NameToLayer("Default")))
			{
				return true;
			}
		}
		return false;
	}

	private void DoHorizontalRaycasts()
	{
		float distanceThisFrame = mVelocity.x * Time.fixedDeltaTime;
		Vector3 start = transform.position + new Vector3(0.0f, -mBoxCollider.size.y * 0.5f + mPhysics.horizontalRaycastMargin, 0.0f);
		Vector3 end = transform.position + new Vector3(0.0f, mBoxCollider.size.y * 0.5f - mPhysics.horizontalRaycastMargin, 0.0f);
		float absDistanceThisFrame = Mathf.Abs(distanceThisFrame);
		float distanceToMove = absDistanceThisFrame;
		float rayLength = mBoxCollider.size.x * 0.5f + absDistanceThisFrame;
		Vector3 rayDirection = Vector3.right * Mathf.Sign(distanceThisFrame);
		for (int i = 0; i < mPhysics.numberOfHorizontalRaycasts; ++i)
		{
			Vector3 rayOrigin = Vector3.Lerp(start, end, i / (mPhysics.numberOfHorizontalRaycasts - 1.0f));
			Ray ray = new Ray(rayOrigin, rayDirection);
			RaycastHit hitInfo;
			Debug.DrawLine(rayOrigin, rayOrigin + rayDirection * rayLength, Color.red, 0.0f);
			if (UnityEngine.Physics.Raycast(ray, out hitInfo, rayLength, 1 << LayerMask.NameToLayer("Default")))
			{
				float distance = Mathf.Abs(start.x - hitInfo.point.x) - (mBoxCollider.size.x * 0.5f);
				if (distance < distanceToMove)
				{
					distanceToMove = distance;
					mVelocity.x = 0.0f;
				}
			}
		}
		transform.position += rayDirection * distanceToMove;
	}

	private void DoVerticalRaycasts()
	{
		float distanceThisFrame = mVelocity.y * Time.fixedDeltaTime;
		Vector3 start = transform.position + new Vector3(-mBoxCollider.size.x * 0.5f + mPhysics.verticalRaycastMargin, 0.0f, 0.0f);
		Vector3 end = transform.position + new Vector3(mBoxCollider.size.x * 0.5f - mPhysics.verticalRaycastMargin, 0.0f, 0.0f);
		float absDistanceThisFrame = Mathf.Abs(distanceThisFrame);
		float distanceToMove = absDistanceThisFrame;
		float rayLength = mBoxCollider.size.y * 0.5f + absDistanceThisFrame;
		Vector3 rayDirection = Vector3.up * Mathf.Sign(distanceThisFrame);
		for (int i = 0; i < mPhysics.numberOfVerticalRaycasts; ++i)
		{
			Vector3 rayOrigin = Vector3.Lerp(start, end, i / (mPhysics.numberOfVerticalRaycasts - 1.0f));
			Ray ray = new Ray(rayOrigin, rayDirection);
			RaycastHit hitInfo;
			Debug.DrawLine(rayOrigin, rayOrigin + rayDirection * rayLength, Color.red, 0.0f);
			if (UnityEngine.Physics.Raycast(ray, out hitInfo, rayLength, 1 << LayerMask.NameToLayer("Default")))
			{
				float distance = Mathf.Abs(start.y - hitInfo.point.y) - (mBoxCollider.size.y * 0.5f);
				if (distance < distanceToMove)
				{
					distanceToMove = distance;
					mVelocity.y = 0.0f;
				}
			}
		}
		transform.position += rayDirection * distanceToMove;
	}
}
