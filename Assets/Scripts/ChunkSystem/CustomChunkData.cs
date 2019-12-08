using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Custom Chunk", menuName = "Bomb Survival Game/Custom Chunk", order = 1)]
public class CustomChunkData : ScriptableObject
{
	public TextAsset mChunkDataAsset;
	public TileData[] mReflectedTileData;
}
