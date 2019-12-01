using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BSGFakePhysics))]
public class Triggerable : MonoBehaviour
{
	public delegate void OnTriggeredHandle(Vector2 aExplosionSource, float aExplosionRadius);
	public OnTriggeredHandle OnTriggered;

	[SerializeField] private bool mHasPhysics;
	public bool HasPhysics { get { return mHasPhysics; } }

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

	public void Trigger(Vector2 aExplosionSource, float aExplosionRadius)
	{
		OnTriggered?.Invoke(aExplosionSource, aExplosionRadius);
	}
}
