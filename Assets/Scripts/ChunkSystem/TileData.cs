using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Tile Data", menuName = "Bomb Survival Game/Tile Data", order = 1)]
public class TileData : ScriptableObject
{
	[System.Serializable]
	public struct SpriteList
	{
		public Sprite[] sprites;
	}

	public SpriteList[] sprites;
	public bool indestructible;
	public bool hasCollision;

	public Sprite GetRandomSpriteFromHealth(int aHealth)
	{
		return sprites[aHealth].sprites.Random();
	}
}
