using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct GameEventSpawnBomb : IGameEventActions
{
	public string[] OnGetProperties()
	{
		return new string[] { "BombPrefab", "Position", "Rotation", "Spread", "Speed" };
	}

	public void OnAction(GameEventData gameEventData)
	{
		CameraController cameraController = GameController.Instance.CameraControllerInstance;
		Vector3 cameraPosition = cameraController.transform.position;
		float cameraTopBound = cameraPosition.y + cameraController.CameraComponent.orthographicSize;
		cameraTopBound += 2.0f;     // idk arbitrary thing so the bomb's center isn't right on the screen edge but slightly outside
		
		//float x = Random.Range(-spawnOffset.x, spawnOffset.x);
		//float y = Random.Range(-spawnOffset.y, spawnOffset.y);
		Vector3 spawnPosition = new Vector2(gameEventData.Position.x, cameraTopBound + gameEventData.Position.y).ToVec3();
		
		//float spread = Random.Range(-spawnAngleSpread, spawnAngleSpread);
		//float radians = (spawnAngle + spread) * Mathf.Deg2Rad;
		//float cos = Mathf.Cos(radians);
		//float sin = Mathf.Sin(radians);
		//Vector2 force = new Vector3(cos, sin) * spawnForce;
		
		Bomb bomb = Object.Instantiate(gameEventData.BombPrefab);
		bomb.transform.Reset();
		bomb.transform.position = spawnPosition;
		//bomb.FakePhysics.AddForce(force);
	}

	public void OnGizmos(GameEventData gameEventData)
	{

	}
}
