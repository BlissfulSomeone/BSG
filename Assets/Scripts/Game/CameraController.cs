using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	[System.Serializable]
	private struct ViewInfo
	{
		public Vector3 position;
		public float viewWidth;
		public float fov;

		public static ViewInfo Lerp(ViewInfo from, ViewInfo to, float t)
		{
			ViewInfo result;
			result.position = Vector3.Lerp(from.position, to.position, t);
			result.viewWidth = Mathf.Lerp(from.viewWidth, to.viewWidth, t);
			result.fov = Mathf.Lerp(from.fov, to.fov, t);
			return result;
		}
	}

	[SerializeField] private ViewInfo mInitialViewInfo;
	private ViewInfo mCurrentViewInfo;
	private ViewInfo mTargetViewInfo;
	
	private Camera mCamera;
	public Camera CameraComponent { get { return mCamera; } }

	private void Awake()
	{
		mCamera = GetComponent<Camera>();

		mCurrentViewInfo = mInitialViewInfo;
		mTargetViewInfo = mInitialViewInfo;
	}

	private void FixedUpdate()
	{
		float frustumHeight = mTargetViewInfo.viewWidth / CameraComponent.aspect;
		float distance = frustumHeight * 0.5f / Mathf.Tan(CameraComponent.fieldOfView * 0.5f * Mathf.Deg2Rad);
		mTargetViewInfo.position.z = distance * -1;

		mCurrentViewInfo = ViewInfo.Lerp(mCurrentViewInfo, mTargetViewInfo, 0.05f);

		transform.position = mCurrentViewInfo.position;
		CameraComponent.fieldOfView = mCurrentViewInfo.fov;
	}

	public void SetTargetPosition(Vector2 targetPosition)
	{
		Vector3 position = mTargetViewInfo.position;
		position.x = targetPosition.x;
		position.y = targetPosition.y;
		mTargetViewInfo.position = position;
	}
}
