using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Tile Data", menuName = "Bomb Survival Game/Tile Data", order = 1)]
public class TileData : ScriptableObject
{
	public Sprite[] sprites;
	public bool indestructible;
	public bool hasCollision;
}
