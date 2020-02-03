using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
	[SerializeField] private TMPro.TextMeshProUGUI mHealthText;
	[SerializeField] private Image mHealthBar;
	[SerializeField] private Gradient mHealthBarColor;

	private void Update()
	{
		Character player = GameController.Instance.PlayerCharacterInstance;
		mHealthText.text = Mathf.CeilToInt(player.Health).ToString() + "/" + Mathf.CeilToInt(player.MaxHealth).ToString();
		float fraction = player.Health / player.MaxHealth;
		mHealthBar.rectTransform.anchorMax = new Vector2(fraction, 1.0f);
		mHealthBar.rectTransform.offsetMax = Vector2.zero;
		mHealthBar.color = mHealthBarColor.Evaluate(fraction);
	}
}
