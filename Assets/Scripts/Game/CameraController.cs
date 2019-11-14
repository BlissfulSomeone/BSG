using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	[System.Serializable]
	private struct ViewInfo
	{
		public Vector3 position;
		public float blocksToFit;

		public static ViewInfo Lerp(ViewInfo from, ViewInfo to, float t)
		{
			ViewInfo result;
			result.position = Vector3.Lerp(from.position, to.position, t);
			result.blocksToFit = Mathf.Lerp(from.blocksToFit, to.blocksToFit, t);
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
		mCurrentViewInfo = ViewInfo.Lerp(mCurrentViewInfo, mTargetViewInfo, 0.05f);

		transform.position = new Vector3(mCurrentViewInfo.position.x, mCurrentViewInfo.position.y, -10.0f);
		mCamera.orthographicSize = (mCurrentViewInfo.blocksToFit * 0.5f) / mCamera.aspect;
	}

	public void SetTargetPosition(Vector3 position)
	{
		mTargetViewInfo.position = position;
	}
}
