using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEventChunkGeneration : IGameEventActions
{
	public string[] OnGetProperties()
	{
		return new string[] { "RockGenerationChaos", "RockGenerationAmount" };
	}

	public void OnAction(GameEventData gameEventData)
	{
		GameController.Instance.ChunkControllerInstance.SetGenerationSettings(gameEventData.RockGenerationChaos, gameEventData.RockGenerationAmount);
	}

	public void OnGizmos(GameEventData gameEventData)
	{

	}
}
