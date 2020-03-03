using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBar : MonoBehaviour
{
	[SerializeField] [DisplayAs("Health Text Object")] private TextMeshProUGUI mHealthText;
	[SerializeField] [DisplayAs("Health Bar Image Object")] private Image mHealthBar;
	[SerializeField] [DisplayAs("Health Bar Color")] private Gradient mHealthBarColor;

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
