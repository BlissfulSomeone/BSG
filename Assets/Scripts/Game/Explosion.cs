using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ExplosionData
{
	public Vector3 Position;
	public float Radius;
	public float Damage;
	public bool Friendly;

	public ExplosionData(Vector3 position, float radius, float damage, bool friendly)
	{
		Position = position;
		Radius = radius;
		Damage = damage;
		Friendly = friendly;
	}
}

public class Explosion : MonoBehaviour
{
	[SerializeField] private ExplosionData mExplosionData;
	[Tooltip("Total added screenshake is the explosion radius times the screen shake multiplier.")] [SerializeField] private float mScreenShakeMultiplier;
	public ExplosionData ExplosionData
	{
		get
		{
			return mExplosionData;
		}
		set
		{
			mExplosionData = value;
			transform.position = mExplosionData.Position;
			transform.localScale = Vector3.one * mExplosionData.Radius;
		}
	}

	private void Start()
	{
		Destroy(gameObject, 3.0f);
		GameController.Instance.Explode(ExplosionData);
		GameController.Instance.CameraControllerInstance.AddScreenShake(ExplosionData.Radius * mScreenShakeMultiplier);
	}
}
