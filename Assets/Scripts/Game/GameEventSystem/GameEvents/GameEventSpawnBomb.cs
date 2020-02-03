using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct GameEventSpawnBomb : IGameEventActions
{
	public string[] OnGetProperties()
	{
		return new string[] { "BombPrefab", "Position", "Rotation", "Spread", "Speed", "Offset", "Number" };
	}

	public void OnAction(GameEventData gameEventData)
	{
		CameraController cameraController = GameController.Instance.CameraControllerInstance;
		Vector3 cameraPosition = cameraController.transform.position;
		float frustumHalfHeight = Mathf.Abs(cameraPosition.z) * Mathf.Tan(cameraController.CameraComponent.fieldOfView * 0.5f * Mathf.Deg2Rad);
		float cameraTopBound = cameraPosition.y + frustumHalfHeight;
		cameraTopBound += 2.0f;     // idk arbitrary thing so the bomb's center isn't right on the screen edge but slightly outside
		
		for (int i = 0; i < gameEventData.Number; ++i)
		{
			float x = Random.Range(-gameEventData.Offset.x, gameEventData.Offset.x);
			float y = Random.Range(-gameEventData.Offset.y, gameEventData.Offset.y);
			Vector3 offset = new Vector2(x, y).ToVec3();
			Vector3 spawnPosition = new Vector2(gameEventData.Position.x, cameraTopBound + gameEventData.Position.y).ToVec3();
		
			float spread = Random.Range(-gameEventData.Spread, gameEventData.Spread);
			float radians = (gameEventData.Rotation + spread) * Mathf.Deg2Rad;
			float cos = Mathf.Cos(radians);
			float sin = Mathf.Sin(radians);
			Vector2 force = new Vector3(cos, sin) * gameEventData.Speed.Get();
		
			Bomb bomb = Object.Instantiate(gameEventData.BombPrefab);
			bomb.transform.Reset();
			bomb.transform.position = spawnPosition + offset;
			bomb.FakePhysics.AddForce(force);
		}
	}

	public void OnGizmos(GameEventData gameEventData)
	{

	}
}
