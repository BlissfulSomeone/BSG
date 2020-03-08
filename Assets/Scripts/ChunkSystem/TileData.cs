using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Tile Data", menuName = "Bomb Survival Game/Tile Data", order = 1)]
public class TileData : ScriptableObject
{
	[SerializeField] public Material Material;
	[SerializeField] public Material[] Depth1Materials;
	[SerializeField] public Material[] Depth2Materials;
	[SerializeField] public Material[] Depth3Materials;
	[SerializeField] public Color TileColor;
	[SerializeField] public bool IsIndestructible;
	[SerializeField] public bool IsCollision;
	[SerializeField] public float Health;
}
