using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour
{
	public enum EMissileState
	{
		None,
		Tracking,
		Flying,
		Grabbed,
		Done,
	}

	private EMissileState mState = EMissileState.None;

	[Header("Tracking")]
	[SerializeField] private float mTrackingTime;
	[SerializeField] private Texture2D mLockOnTexture;

	private int mCollisionMask = 0;
	private Vector3 mLockOnPosition = Vector3.zero;
	private bool mHasLockOn = false;
	private Vector3 mTargetPosition = Vector3.zero;
	private bool mHasTarget = false;
	private float mCurrentTime = 0.0f;
	private float mFlashTimer = 0.0f;
	private bool mIsFlashing = false;

	[Header("Movement")]
	[SerializeField] private float mSpawnHeight;
	[SerializeField] private float mSpeed;
	
	[Header("Bomb")]
	[SerializeField] private float mExplosionRadius;
	[SerializeField] private float mDamage;
	[SerializeField] private Explosion mExplosionPrefab;

	private TrailRenderer mTrailRenderer;
	private BSGFakePhysics mFakePhysics;
	private Renderer[] mRenderers;
	
	private void Awake()
	{
		mCollisionMask = Globals.GetCollisionMask(gameObject);
		mCurrentTime = mTrackingTime;
		mTrailRenderer = GetComponent<TrailRenderer>();
		mFakePhysics = GetComponent<BSGFakePhysics>();
		mFakePhysics.OnImpact += OnImpact;
		mRenderers = GetComponentsInChildren<Renderer>();

		SetState(EMissileState.Tracking);

		transform.position = Vector3.up * 100.0f;
	}

	private void OnDestroy()
	{
		mFakePhysics.OnImpact -= OnImpact;
	}

	private void OnImpact()
	{
		SetState(EMissileState.Done);
	}

	private void FixedUpdate()
	{
		if (mState == EMissileState.Tracking)
		{
			Character player = GameController.Instance.PlayerCharacterInstance;
			if (player != null)
			{
				if (mCurrentTime >= 0.0f)
				{
					mLockOnPosition = player.transform.position;
					mHasLockOn = true;
					
				}
				else
				{
					SetState(EMissileState.Flying);
					transform.position = mTargetPosition + Vector3.up * mSpawnHeight;
				}
			}
			if (mHasLockOn)
			{
				Ray ray = new Ray(mLockOnPosition + Vector3.up * 0.1f, Vector3.down);
				RaycastHit hitInfo;
				if (Physics.Raycast(ray, out hitInfo, float.MaxValue, mCollisionMask))
				{
					mTargetPosition = hitInfo.point;
					mHasTarget = true;
				}
			}
		}
		else if (mState == EMissileState.Flying)
		{
			Vector2 forward = mFakePhysics.Velocity.normalized;
			mFakePhysics.Velocity = forward * mSpeed;
			transform.rotation = Quaternion.LookRotation(mFakePhysics.Velocity.normalized, Vector3.up);
		}

		if (mCurrentTime > 0.0f)
		{
			mCurrentTime -= Time.fixedDeltaTime;
		}

		if (mFlashTimer > 0.0f)
		{
			mFlashTimer -= Time.deltaTime;
		}
		else
		{
			mFlashTimer = 0.1f;
			mIsFlashing = !mIsFlashing;
		}
	}

	public void SetState(EMissileState state)
	{
		if (mState != state && mState != EMissileState.Done)
		{
			mState = state;

			switch (mState)
			{
				case EMissileState.Tracking:
					foreach (Renderer renderer in mRenderers) renderer.enabled = false;
					mFakePhysics.Velocity = Vector2.zero;
					break;

				case EMissileState.Flying:
					foreach (Renderer renderer in mRenderers) renderer.enabled = true;
					mFakePhysics.Velocity = Vector2.down * mSpeed;
					break;

				case EMissileState.Grabbed:
					foreach (Renderer renderer in mRenderers) renderer.enabled = true;
					mTrailRenderer.enabled = false;
					mFakePhysics.Velocity = Vector2.zero;
					break;

				case EMissileState.Done:
					foreach (Renderer renderer in mRenderers) renderer.enabled = false;
					mFakePhysics.enabled = false;
					mFakePhysics.Velocity = Vector2.zero;

					Explosion explosion = Instantiate(mExplosionPrefab);
					explosion.ExplosionData = new ExplosionData { Position = transform.position, Damage = mDamage, Radius = mExplosionRadius, Friendly = false };
					Destroy(gameObject, mTrailRenderer.time);
					break;
			}
		}
	}

	private void OnGUI()
	{
		if ((mState == EMissileState.Tracking || mState == EMissileState.Flying) && mHasTarget)
		{
			Vector2 screenPosition = Camera.main.WorldToScreenPoint(mTargetPosition);
			Rect rect = new Rect();
			float factor = 1.0f + (mCurrentTime / mTrackingTime) * 0.5f;
			float SIZE_X = 144.0f * factor;
			float SIZE_Y = 72.0f * factor;
			float HALF_SIZE_X = SIZE_X / 2.0f;
			float HALF_SIZE_Y = SIZE_Y / 2.0f;
			rect.x = screenPosition.x - HALF_SIZE_X;
			rect.y = Screen.height - screenPosition.y - HALF_SIZE_Y;
			rect.width = SIZE_X;
			rect.height = SIZE_Y;
			GUI.DrawTexture(rect, mLockOnTexture, ScaleMode.StretchToFill, true, 0.0f, mIsFlashing || mState != EMissileState.Tracking ? Color.red : Color.white, 0.0f, 0.0f);
		}
	}
}
