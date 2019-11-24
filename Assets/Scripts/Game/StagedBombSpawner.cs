using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StagedBombSpawner : MonoBehaviour
{
	[SerializeField] private Bomb[] bombPrefabs;
	[SerializeField] private int numberOfBombs = 1;
	[SerializeField] private Vector2 spawnOffset;
	[SerializeField] private float spawnForce;
	[SerializeField] private float spawnAngle = -90;
	[SerializeField] private float spawnAngleSpread;
	[SerializeField] private float spawnDelay;
	
	private Bomb[] BombPrefabs { get { if (bombPrefabs == null) bombPrefabs = new Bomb[0]; return bombPrefabs; } }

	private void Start()
	{
		StartCoroutine(Coroutine_SpawnBombs());
	}

	private IEnumerator Coroutine_SpawnBombs()
	{
		yield return new WaitForSeconds(spawnDelay);

		if (BombPrefabs.Length > 0)
		{
			for (int i = 0; i < numberOfBombs; ++i)
			{
				CameraController cameraController = GameController.Instance.CameraControllerInstance;
				Vector3 cameraPosition = cameraController.transform.position;
				float cameraTopBound = cameraPosition.y + cameraController.CameraComponent.orthographicSize;
				cameraTopBound += 2.0f;		// idk arbitrary thing so the bomb's center isn't right on the screen edge but slightly outside

				float x = Random.Range(-spawnOffset.x, spawnOffset.x);
				float y = Random.Range(-spawnOffset.y, spawnOffset.y);
				Vector3 offset = new Vector3(x, cameraTopBound + y, 0.0f);

				float spread = Random.Range(-spawnAngleSpread, spawnAngleSpread);
				float radians = (spawnAngle + spread) * Mathf.Deg2Rad;
				float cos = Mathf.Cos(radians);
				float sin = Mathf.Sin(radians);
				Vector3 force = new Vector3(cos, sin, 0.0f) * spawnForce;

				Bomb bomb = Instantiate(BombPrefabs.Random());
				bomb.transform.Reset();
				bomb.transform.position = transform.position + offset;
				bomb.Rigidbody.AddForce(force, ForceMode.VelocityChange);
			}
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = BombPrefabs.Length > 0 ? Color.green : Color.red;
		Gizmos.DrawSphere(transform.position, 0.5f);
	}

	private void OnDrawGizmosSelected()
	{
		if (spawnOffset.IsZero() == false)
		{
			Gizmos.color = Color.white;
			Gizmos.DrawWireCube(transform.position, spawnOffset.ToVec3() * 2.0f);
		}
		if (spawnForce > 0.0f)
		{
			Gizmos.color = Color.white;
			float radians = spawnAngle * Mathf.Deg2Rad;
			float cos = Mathf.Cos(radians);
			float sin = Mathf.Sin(radians);
			Vector3 from = transform.position;
			Vector3 to = from + new Vector3(cos, sin, 0.0f) * spawnForce;
			Gizmos.DrawLine(from, to);

			Gizmos.color = Color.grey;
			float offsetRadians = spawnAngleSpread * Mathf.Deg2Rad;
			float offsetCos0 = Mathf.Cos(radians + offsetRadians);
			float offsetSin0 = Mathf.Sin(radians + offsetRadians);
			float offsetCos1 = Mathf.Cos(radians - offsetRadians);
			float offsetSin1 = Mathf.Sin(radians - offsetRadians);
			Vector3 offsetTo0 = from + new Vector3(offsetCos0, offsetSin0, 0.0f) * spawnForce * 0.5f;
			Vector3 offsetTo1 = from + new Vector3(offsetCos1, offsetSin1, 0.0f) * spawnForce * 0.5f;
			Gizmos.DrawLine(from, offsetTo0);
			Gizmos.DrawLine(from, offsetTo1);
		}
	}
}
