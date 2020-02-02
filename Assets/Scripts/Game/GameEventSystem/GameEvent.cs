using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct GameEventData
{
	// For external use.
	[SerializeField] public bool HasBeenTriggered;
	[SerializeField] public bool IsFoldOut;

	// All game events include these.
	[SerializeField] public string Name;
	[SerializeField] public int TriggerAtDepth;
	[SerializeField] public float TriggerDelayTime;
	public float TriggerAtTime;
	[SerializeField] public GameEventCore.EGameEventType EventType;

	// Game event specific variables.
	[SerializeField] public float BombsPerSecond;
	[SerializeField] public float RampPerDepth;
	[SerializeField] public float MaxBombsPerSecond;
	[SerializeField] [EnumFlags] public GameEventCore.EBombType BombType;
	[SerializeField] public Bomb BombPrefab;
	[SerializeField] public Bomb[] BombPrefabs;
	[SerializeField] public Vector2 Position;
	[SerializeField] [Range(0.0f, 360.0f)] public float Rotation;
	[SerializeField] [Range(0.0f, 180.0f)] public float Spread;
	[SerializeField] public MinMaxFloat Speed;
	[SerializeField] public Vector2 Offset;
	[SerializeField] public int Number;
	[SerializeField] public GameEvent NestedEvent;
}

[CreateAssetMenu(fileName = "Game Event", menuName = "Bomb Survival Game/Game Event", order = 1)]
public class GameEvent : ScriptableObject
{
	public static GameEventData NO_EVENT = new GameEventData();

	[SerializeField] private List<GameEventData> mGameEventData;
	
	public int Count { get { if (mGameEventData == null) return -1; return mGameEventData.Count; } }

	public GameEventData GetGameEvent(int index)
	{
		if (mGameEventData == null)
			return NO_EVENT;

		if (index < 0 || index >= mGameEventData.Count)
		{
			Debug.LogError("Getting index " + index.ToString() + " out of range " + mGameEventData.Count.ToString() + ".");
			return NO_EVENT;
		}

		return mGameEventData[index];
	}

	public void UpdateGameEvent(GameEventData gameEventData, int index)
	{
		if (mGameEventData == null)
			return;

		if (index < 0 || index >= mGameEventData.Count)
		{
			Debug.LogError("Updating index " + index.ToString() + " out of range " + mGameEventData.Count.ToString() + ".");
			return;
		}

		mGameEventData[index] = gameEventData;
	}
}
