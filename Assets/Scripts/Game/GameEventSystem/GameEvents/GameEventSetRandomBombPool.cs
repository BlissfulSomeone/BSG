using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct GameEventSetRandomBombPool : IGameEventActions
{
	public string[] OnGetProperties()
	{
		return new string[] { "Objects" };
	}

	public void OnAction(GameEventData gameEventData)
	{
		GameController.Instance.BombSpawnerInstance.SetAllowedBombs(gameEventData.Objects);
	}

	public void OnGizmos(GameEventData gameEventData)
	{

	}
}
