using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CameraController : MonoBehaviour
{
	[System.Serializable]
	public struct ViewInfo
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

		public static ViewInfo operator +(ViewInfo a, ViewInfo b) =>
			new ViewInfo
			{ 
				position = a.position + b.position,
				rotation = a.rotation + b.rotation,
				viewWidth = a.viewWidth + b.viewWidth,
				fov = a.fov + b.fov
			};
	}

	[Header("Basics")]
	[SerializeField] private ViewInfo mInitialViewInfo;
    [SerializeField] [Range(0.0f, 1.0f)] private float mLerpSpeed;
	[SerializeField] private float mCameraLookAhead;

	[Header("Dead Zone")]
	[Tooltip("The dead zone is in screen space. Put to 0 to disable.")] [SerializeField] [Range(0.0f, 1.0f)] private float mDeadZone;
	[Tooltip("The dead zone is in screen space.")] [SerializeField] [Range(-0.5f, 0.5f)] private float mDeadZoneOffset;

	[Header("Screen Shake")]
	[SerializeField] private float mScreenShakeFrequency;
	[SerializeField] private float mScreenShakeDecay;
	[Tooltip("Max cachecd screenshake")] [SerializeField] private float mMaxScreenShake;
	[Tooltip("Max shown screenshake")] [SerializeField] private float mCapScreenShake;

	[Header("Debug")]
	[SerializeField] private bool mDrawDebug;

	private ViewInfo mCurrentViewInfo;
	private ViewInfo mTargetViewInfo;
	private Dictionary<Guid, ViewInfo> mAdditiveViewInfos;
#if UNITY_EDITOR
	private Vector3 mCachedTargetPosition;
#endif
	
	private Camera mCamera;
	public Camera CameraComponent { get { return mCamera; } }
	public float CameraLookAhead { get { return mCameraLookAhead; } }

	private float mScreenShake;

    private void Awake()
	{
		mCamera = GetComponent<Camera>();

		mCurrentViewInfo = mInitialViewInfo;
		mTargetViewInfo = new ViewInfo();

		mAdditiveViewInfos = new Dictionary<Guid, ViewInfo>();
		PushAdditiveViewInfo(mInitialViewInfo);
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
		ViewInfo finalTargetViewInfo = mTargetViewInfo;
		foreach (ViewInfo viewInfo in mAdditiveViewInfos.Values)
		{
			finalTargetViewInfo += viewInfo;
		}

		float frustumHeight = finalTargetViewInfo.viewWidth / CameraComponent.aspect;
		float distance = frustumHeight * 0.5f / Mathf.Tan(CameraComponent.fieldOfView * 0.5f * Mathf.Deg2Rad);
		finalTargetViewInfo.position.z = distance * -1;

		mCurrentViewInfo = ViewInfo.Lerp(mCurrentViewInfo, finalTargetViewInfo, mLerpSpeed);

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
#if UNITY_EDITOR
		mCachedTargetPosition = targetPosition.ToVec3();
#endif

		if (IsInDeadZone(targetPosition))
			return;

		Vector3 position = mTargetViewInfo.position;
		position.x = targetPosition.x;
		position.y = targetPosition.y;
		mTargetViewInfo.position = position;
	}

	private bool IsInDeadZone(Vector2 point)
	{
		float deadZoneSize = mDeadZone * Screen.height;
		float offset = mDeadZoneOffset * Screen.height;
		float top = (Screen.height / 2 - deadZoneSize) + offset;
		float bottom = (Screen.height / 2 + deadZoneSize) + offset;
		Vector2 target = mCamera.WorldToScreenPoint(point).ToVec2();
		target.y = Screen.height - target.y;
		bool isInDeadZone = target.y > top && target.y < bottom;
		
		return target.y > top && target.y < bottom;
	}

	public void SetTargetRotation(Vector3 targetRotation)
	{
		mTargetViewInfo.rotation = targetRotation;
	}
	
	public Guid PushAdditiveViewInfo(ViewInfo additiveViewInfo)
	{
		Guid guid = Guid.NewGuid();
		return PushAdditiveViewInfo(additiveViewInfo, guid);
	}

	public Guid PushAdditiveViewInfo(ViewInfo additiveViewInfo, Guid guid)
	{
		if (mAdditiveViewInfos.ContainsKey(guid))
		{
			mAdditiveViewInfos[guid] = additiveViewInfo;
		}
		else
		{
			mAdditiveViewInfos.Add(guid, additiveViewInfo);
		}
		return guid;
	}

	public void RemoveAdditiveViewInfo(Guid guid)
	{
		if (mAdditiveViewInfos.ContainsKey(guid))
		{
			mAdditiveViewInfos.Remove(guid);
		}
	}

	public void AddScreenShake(float amount)
	{
		mScreenShake = Mathf.Min(mScreenShake + amount, mMaxScreenShake);
	}

#if UNITY_EDITOR
	private void OnGUI()
	{
		if (!mDrawDebug)
			return;

		float deadZoneSize = mDeadZone * Screen.height;
		float offset = mDeadZoneOffset * Screen.height;

		float top = (Screen.height / 2 - deadZoneSize) + offset;
		float bottom = (Screen.height / 2 + deadZoneSize) + offset;
		float cachedTarget = Screen.height - mCamera.WorldToScreenPoint(mCachedTargetPosition.ToVec2()).y;
		float target = Screen.height - mCamera.WorldToScreenPoint(mTargetViewInfo.position.ToVec2()).y;
		float current = Screen.height - mCamera.WorldToScreenPoint(transform.position.ToVec2()).y;
			
		Handles.BeginGUI();
		DrawLine(top, 3, Color.red);
		DrawLine(bottom, 3, Color.red);
		DrawLine(cachedTarget, 3, new Color(0.0f, 1.0f, 0.0f, 0.33f));
		DrawLine(target, 3, Color.green);
		DrawLine(current, 3, Color.magenta);
		Handles.EndGUI();
	}

	private void DrawLine(float y, int width, Color color)
	{
		Color c = Handles.color;
		Handles.color = color;
		for (int i = 0; i < width; ++i)
		{
			Handles.DrawLine(new Vector3(0.0f, y - width / 2, 0.0f), new Vector3(Screen.width, y - width / 2));
			Handles.DrawLine(new Vector3(0.0f, y + width / 2, 0.0f), new Vector3(Screen.width, y + width / 2));
		}
		Handles.color = c;
	}
#endif
}
