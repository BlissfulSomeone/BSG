using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
	[System.Serializable]
	private struct InputNames
	{
		[Header("Actions")]
		public string Jump;
		public string Grab;

		[Header("Axis")]
		public string Horizontal;
	}

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
		public float groundCheckDistance;
		public float acceleration;
		public float maxSpeed;
		public float jumpForce;
		public float jumpTime;
	}

	public delegate void OnKilledHandler(Character aKilledCharacter);
	public OnKilledHandler OnKilled;

	private Rigidbody mRigidbody;
	private BoxCollider mBoxCollider;
	private Triggerable mTriggerable;

	[SerializeField] private InputNames mInputNames;
	[SerializeField] private Physics mPhysics;
	[SerializeField] private Movement mMovement;
	[SerializeField] private float mMaxHealth;
	[SerializeField] private Transform mGrabSocket;
	
	private bool mIsGrounded = false;
	private Vector3 mMovementInput = Vector3.zero;
	private bool mWantToJump = false;
	private float mJumpTime = 0.0f;
	private Vector3 mVelocity = Vector3.zero;
	private Vector3 mDirection = Vector3.right;

	private float mHealth;
	public float Health { get { return mHealth; } }
	public float MaxHealth { get { return mMaxHealth; } }

	private bool mIsGrabbing = false;
	private BSGFakePhysics mGrabbedObject = null;

	private void Awake()
	{
		mRigidbody = GetComponent<Rigidbody>();
		mBoxCollider = GetComponent<BoxCollider>();
		mTriggerable = GetComponent<Triggerable>();
		mTriggerable.OnTriggered += Trigger;

		mHealth = mMaxHealth;
	}

	private void OnDestroy()
	{
		mTriggerable.OnTriggered -= Trigger;
	}

	private void Trigger(ExplosionData explosionData)
	{
		if (!explosionData.Friendly)
		{
			mHealth -= explosionData.Damage;
			if (mHealth <= 0.0f)
			{
				OnKilled?.Invoke(this);
				Destroy(gameObject);
			}
		}
	}

	private void Update()
	{
		mMovementInput.x = Input.GetAxisRaw(mInputNames.Horizontal);
		mWantToJump = Input.GetButton(mInputNames.Jump);
		if (mIsGrounded && mWantToJump)
		{
			mJumpTime = mMovement.jumpTime;
		}
		else if (Input.GetButtonUp(mInputNames.Jump))
		{
			mJumpTime = -1.0f;
		}
		mIsGrabbing = Input.GetButton(mInputNames.Grab);
	}

	private float Damp(float aSource, float aSmoothing, float aDeltaTime)
	{
		return aSource * Mathf.Pow(aSmoothing, aDeltaTime);
	}

	private void FixedUpdate()
	{
		mIsGrounded = IsGrounded();
		
		if (!mIsGrounded)
		{
			mVelocity.y -= 9.82f * mPhysics.gravityScale * Time.fixedDeltaTime;
		}

		if (mWantToJump && mJumpTime >= 0.0f)
		{
			mVelocity.y = mMovement.jumpForce;
			mJumpTime -= Time.fixedDeltaTime;
		}
		
		if (mMovementInput.x != 1.0f && mVelocity.x > 0.0f)
			mVelocity.x = Mathf.Max(mVelocity.x - mMovement.acceleration * Time.fixedDeltaTime, 0.0f);
		if (mMovementInput.x != -1.0f && mVelocity.x < 0.0f)
			mVelocity.x = Mathf.Min(mVelocity.x + mMovement.acceleration * Time.fixedDeltaTime, 0.0f);
		mVelocity.x += mMovementInput.x * mMovement.acceleration * Time.fixedDeltaTime;
		mVelocity.x = Mathf.Clamp(mVelocity.x, -mMovement.maxSpeed, mMovement.maxSpeed);
		if (mVelocity.x > 0.0f)
			mDirection = Vector3.right;
		if (mVelocity.x < 0.0f)
			mDirection = Vector3.left;

		DoHorizontalRaycasts();
		DoVerticalRaycasts();

		if (mIsGrabbing)
		{
			if (mGrabbedObject == null)
			{
				Collider[] colliders = UnityEngine.Physics.OverlapSphere(transform.position + Vector3.Scale(mGrabSocket.localPosition, mDirection), 0.5f);
				for (int i = 0; i < colliders.Length; ++i)
				{
					if (colliders[i].gameObject == this)
						continue;

					BSGFakePhysics fakePhysics = colliders[i].gameObject.GetComponent<BSGFakePhysics>();
					if (fakePhysics != null)
					{
						mGrabbedObject = fakePhysics;
						fakePhysics.enabled = false;
						fakePhysics.transform.localScale = Vector3.one * 0.5f;
					}
				}
			}
			else
			{
				mGrabbedObject.transform.position = transform.position + Vector3.Scale(mGrabSocket.localPosition, mDirection);
			}
		}
		else
		{
			if (mGrabbedObject != null)
			{
				mGrabbedObject.enabled = true;
				mGrabbedObject.Velocity = new Vector2(10.0f * mDirection.x, 5.0f);
				mGrabbedObject.transform.localScale = Vector3.one;
				mGrabbedObject = null;
			}
		}
	}

	private bool IsGrounded()
	{
		Vector3 start = transform.position + new Vector3(-mBoxCollider.size.x * 0.5f + mPhysics.verticalRaycastMargin, 0.0f, 0.0f);
		Vector3 end = transform.position + new Vector3(mBoxCollider.size.x * 0.5f - mPhysics.verticalRaycastMargin, 0.0f, 0.0f);
		float rayLength = mBoxCollider.size.y * 0.5f + mMovement.groundCheckDistance;
		for (int i = 0; i < mPhysics.numberOfVerticalRaycasts; ++i)
		{
			Vector3 rayOrigin = Vector3.Lerp(start, end, i / (mPhysics.numberOfVerticalRaycasts - 1.0f));
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
