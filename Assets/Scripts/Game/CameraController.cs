using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	[System.Serializable]
	private struct ViewInfo
	{
		public Vector3 position;
		public Vector3 rotation;
		public float viewWidth;
		public float fov;

		public static ViewInfo Lerp(ViewInfo from, ViewInfo to, float t)
		{
			ViewInfo result;
			result.position = Vector3.Lerp(from.position, to.position, t);
			result.rotation = Quaternion.Slerp(Quaternion.Euler(from.rotation), Quaternion.Euler(to.rotation), t).eulerAngles;
			result.viewWidth = Mathf.Lerp(from.viewWidth, to.viewWidth, t);
			result.fov = Mathf.Lerp(from.fov, to.fov, t);
			return result;
		}
	}

	[SerializeField] private ViewInfo mInitialViewInfo;
    [SerializeField] private float mCameraYOffset;
    [SerializeField] private float mLerpSpeed;
    [SerializeField] private float mCameraLookAhead;
	[SerializeField] private float mScreenShakeFrequency;
	[SerializeField] private float mScreenShakeDecay;
	[Tooltip("Max cachecd screenshake")] [SerializeField] private float mMaxScreenShake;
	[Tooltip("Max shown screenshake")] [SerializeField] private float mCapScreenShake;
	private ViewInfo mCurrentViewInfo;
	private ViewInfo mTargetViewInfo;
	
	private Camera mCamera;
	public Camera CameraComponent { get { return mCamera; } }
	public float CameraLookAhead { get { return mCameraLookAhead; } }

	private float mScreenShake;

    private void Awake()
	{
		mCamera = GetComponent<Camera>();

		mCurrentViewInfo = mInitialViewInfo;
		mTargetViewInfo = mInitialViewInfo;
	}

	private void Update()
	{
		if (mScreenShake > 0.0f)
		{
			mScreenShake = Mathf.Max(mScreenShake - mScreenShakeDecay * Time.deltaTime, 0.0f);
		}
	}

	private void FixedUpdate()
	{
		float frustumHeight = mTargetViewInfo.viewWidth / CameraComponent.aspect;
		float distance = frustumHeight * 0.5f / Mathf.Tan(CameraComponent.fieldOfView * 0.5f * Mathf.Deg2Rad);
		mTargetViewInfo.position.z = distance * -1;

		mCurrentViewInfo = ViewInfo.Lerp(mCurrentViewInfo, mTargetViewInfo, mLerpSpeed);

		transform.position = mCurrentViewInfo.position;
		transform.eulerAngles = mCurrentViewInfo.rotation;
		CameraComponent.fieldOfView = mCurrentViewInfo.fov;

		if (mScreenShake > 0.0f)
		{
			const float TAU = Mathf.PI * 2;
			float radius = Time.timeSinceLevelLoad * mScreenShakeFrequency * TAU;
			float power = Mathf.Min(mScreenShake, mCapScreenShake) / 100.0f;
			float offset = Mathf.Sin(radius) * power;
			transform.position += new Vector3(0.0f, offset, 0.0f);
		}
	}

	public void SetTargetPosition(Vector2 targetPosition)
	{
		Vector3 position = mTargetViewInfo.position;
		position.x = targetPosition.x;
		position.y = targetPosition.y + mCameraYOffset;
		mTargetViewInfo.position = position;
	}

	public void AddScreenShake(float amount)
	{
		mScreenShake = Mathf.Min(mScreenShake + amount, mMaxScreenShake);
	}
}
