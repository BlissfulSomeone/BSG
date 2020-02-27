using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct GameEventSetRandomBombPool : IGameEventActions
{
	public string[] OnGetProperties()
	{
		return new string[] { "WeightedObjects" };
	}

	public void OnAction(GameEventData gameEventData)
	{
		GameController.Instance.BombSpawnerInstance.SetAllowedBombs(gameEventData.WeightedObjects);
	}

	public void OnGizmos(GameEventData gameEventData)
	{

	}
}
