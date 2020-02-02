using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Explosion : MonoBehaviour
{
	private void Start()
	{
		GameController.Instance.Explode(transform.position, transform.localScale.x);
	}
}
