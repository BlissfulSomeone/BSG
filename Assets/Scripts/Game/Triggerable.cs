using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triggerable : MonoBehaviour
{
	public delegate void OnTriggeredHandle(Vector2 aExplosionSource, float aExplosionRadius);
	public OnTriggeredHandle OnTriggered;

	[SerializeField] private bool mHasPhysics;
	public bool HasPhysics { get { return mHasPhysics; } }

	public Rigidbody Rigidbody
	{
		get
		{
			Rigidbody body = GetComponent<Rigidbody>();
			if (body == null)
				body = gameObject.AddComponent<Rigidbody>();
			return body;
		}
	}

	public void Trigger(Vector2 aExplosionSource, float aExplosionRadius)
	{
		OnTriggered?.Invoke(aExplosionSource, aExplosionRadius);
	}
}
