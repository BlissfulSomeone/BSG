using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ExplosionData
{
	public float Radius;
	public float Damage;
	public float Knockback;
	public bool Friendly;

	public ExplosionData(float radius, float damage, float knockback, bool friendly)
	{
		Radius = radius;
		Damage = damage;
		Knockback = knockback;
		Friendly = friendly;
	}
}

public struct ExplosionInstance
{
	public Vector3 Position;
	public ExplosionData ExplosionData;

	public ExplosionInstance(Vector3 position, ExplosionData explosionData)
	{
		Position = position;
		ExplosionData = explosionData;
	}
}

public class Explosion : MonoBehaviour
{
	[Tooltip("Total added screenshake is the explosion radius times the screen shake multiplier.")] [SerializeField] private float mScreenShakeMultiplier;
	[SerializeField] private ExplosionData mExplosionData;
	public ExplosionData ExplosionData
	{
		get
		{
			return mExplosionData;
		}
		set
		{
			mExplosionData = value;
		}
	}

	private void Start()
	{
		Destroy(gameObject, 3.0f);
		transform.localScale = Vector3.one * mExplosionData.Radius;
		GameController.Instance.Explode(new ExplosionInstance(transform.position, mExplosionData));
		GameController.Instance.CameraControllerInstance.AddScreenShake(ExplosionData.Radius * mScreenShakeMultiplier);
	}
}
