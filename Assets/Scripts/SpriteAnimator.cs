using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteAnimator : MonoBehaviour
{
	[SerializeField] private SpriteAnimation mSpriteAnimation;
	[SerializeField] private float mFrameRate = 8.0f;
	[SerializeField] private bool mPlay = true;
	[SerializeField] private bool mLoop = true;

	public bool IsPlaying { get { return mPlay; } set { mPlay = value; } }
	public bool IsLooping { get { return mLoop; } set { mLoop = value; } }

	private float mCurrentFrame = 0.0f;
	private int mPreviousFrameIndex = -1;

	private SpriteRenderer mSpriteRenderer;
	private SpriteRenderer SpriteRenderer { get { if (mSpriteRenderer == null) mSpriteRenderer = GetComponent<SpriteRenderer>(); return mSpriteRenderer; } }

	private void OnValidate()
	{
		if (mSpriteAnimation == null)
		{
			SpriteRenderer.sprite = null;
			return;
		}

		if (mSpriteAnimation.frames == null || mSpriteAnimation.frames.Length == 0)
		{
			Debug.LogWarning("Assigned a Sprite Animation [" + mSpriteAnimation.name + "] with no frames.");
			SpriteRenderer.sprite = null;
			return;
		}
		
		SpriteRenderer.sprite = mSpriteAnimation.frames[0];
	}

	private void Awake()
	{
		mSpriteRenderer = GetComponent<SpriteRenderer>();
	}

	private void Update()
	{
		if (mPlay == true && Mathf.Abs(mFrameRate) > 0.0f)
		{
			mCurrentFrame += Time.deltaTime * mFrameRate;
			if (mCurrentFrame >= mSpriteAnimation.Length)
			{
				if (IsLooping == true)
					mCurrentFrame -= mSpriteAnimation.Length;
				else
					IsPlaying = false;
			}
			if (mCurrentFrame < 0.0f)
			{
				if (IsLooping == true)
					mCurrentFrame += mSpriteAnimation.Length;
				else
					IsPlaying = false;
			}

			int frameIndex = Mathf.FloorToInt(mCurrentFrame);
			if (frameIndex != mPreviousFrameIndex)
				SpriteRenderer.sprite = mSpriteAnimation.frames[frameIndex];
		}
	}
}
