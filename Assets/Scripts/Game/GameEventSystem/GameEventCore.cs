using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGameEventActions
{
	string[] OnGetProperties();
	void OnAction(GameEventData gameEventData);
	void OnGizmos(GameEventData gameEventData);
}

public class GameEventCore
{
	public enum EBombType
	{
		Normal,
		Timed,
		Big,
		Cluster,
		ClusterChild
	}

	public enum EGameEventType
	{
		None,
		BombsPerSecond,
		RandomBombEnabled,
		SpawnBomb,
		NestedEvent,
	}

	public static IGameEventActions[] GameEventActions = new IGameEventActions[]
	{
		new GameEventNone(),
		new GameEventSetBombsPerSecond(),
		new GameEventSetRandomBombPool(),
		new GameEventSpawnBomb(),
		new GameEventNested(),
	};
}
