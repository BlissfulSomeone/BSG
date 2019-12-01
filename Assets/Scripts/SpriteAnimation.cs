using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Sprite Animation", menuName = "Bomb Survival Game/Sprite Animation", order = 1)]
public class SpriteAnimation : ScriptableObject
{
	public Sprite[] frames;

	public int Length { get { if (frames == null) return 0; return frames.Length; } }
}
