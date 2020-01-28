using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct GameEventSetBombsPerSecond : IGameEventActions
{
	public string[] OnGetProperties()
	{
		return new string[] { "BombsPerSecond" };
	}

	public void OnAction(GameEventData gameEventData)
	{
		GameController.Instance.BombSpawnerInstance.SetBPS(gameEventData.BombsPerSecond);
	}

	public void OnGizmos(GameEventData gameEventData)
	{

	}
}
