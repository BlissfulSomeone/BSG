using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BSGFakePhysics))]
public class Triggerable : MonoBehaviour
{
	public delegate void OnTriggeredHandle(ExplosionData explosionData);
	public OnTriggeredHandle OnTriggered;

	[SerializeField] private bool mHasPhysics = true;
	public bool HasPhysics { get { return mHasPhysics; } }

	[SerializeField] private float mUpForceMultiplier = 1.25f;
	public float UpForceMultiplier { get { return mUpForceMultiplier; } }

	private BSGFakePhysics mFakePhysics;
	public BSGFakePhysics FakePhysics
	{
		get
		{
			if (mFakePhysics == null)
				mFakePhysics = GetComponent<BSGFakePhysics>();
			return mFakePhysics;
		}
	}

	public void Trigger(ExplosionData explosionData)
	{
		OnTriggered?.Invoke(explosionData);
	}
}
