using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct GameEventSetBombsPerSecond : IGameEventActions
{
	public string[] OnGetProperties()
	{
		return new string[] { "BombsPerSecond", "RampPerDepth", "MaxBombsPerSecond" };
	}

	public void OnAction(GameEventData gameEventData)
	{
		GameController.Instance.BombSpawnerInstance.SetBPS(gameEventData.BombsPerSecond, gameEventData.RampPerDepth, gameEventData.MaxBombsPerSecond);
	}

	public void OnGizmos(GameEventData gameEventData)
	{

	}
}
