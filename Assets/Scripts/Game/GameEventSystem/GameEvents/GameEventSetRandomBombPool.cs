using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct GameEventSetRandomBombPool : IGameEventActions
{
	public string[] OnGetProperties()
	{
		return new string[] { "BombPrefabs" };
	}

	public void OnAction(GameEventData gameEventData)
	{
		GameController.Instance.BombSpawnerInstance.SetAllowedBombs(gameEventData.BombPrefabs);
	}

	public void OnGizmos(GameEventData gameEventData)
	{

	}
}
