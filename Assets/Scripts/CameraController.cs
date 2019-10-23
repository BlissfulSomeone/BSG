using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	[SerializeField] private int mHorizontalBlocksToFit = 20;

	private Camera mCamera;

	private void Awake()
	{
		mCamera = GetComponent<Camera>();
	}

	private void Update()
	{
		mCamera.orthographicSize = (mHorizontalBlocksToFit * 0.5f) / mCamera.aspect;
	}
}
