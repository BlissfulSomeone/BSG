using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Explosion : MonoBehaviour
{
	private float mTimer = 0.0f;
	private SpriteRenderer mSpriteRenderer;

	private void Awake()
	{
		mSpriteRenderer = GetComponent<SpriteRenderer>();
	}

	private void Update()
	{
		const float ANIMATION_SPEED = 0.05f;
		mTimer += Time.deltaTime;
		if (mTimer < ANIMATION_SPEED)
			mSpriteRenderer.color = Color.white;
		else if (mTimer < ANIMATION_SPEED * 2)
			mSpriteRenderer.color = Color.black;
		else
			Destroy(gameObject);
	}
}
