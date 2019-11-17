using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepthMeter : MonoBehaviour
{
	[SerializeField] private TMPro.TextMeshProUGUI mDepthText;

	private void Update()
	{
		mDepthText.text = "Depth: " + Mathf.FloorToInt(GameController.Instance.FurthestDepth).ToString();
	}
}
