using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockAbility : Ability
{
	[SerializeField] [DisplayAs("Max Block Time")] private float mMaxBlockTime;
	[SerializeField] [DisplayAs("Character Block State")] private CharacterOverrides mCharacterBlockState;

	private float mCurrentBlockTime;

	protected override void StartAbility_Internal()
	{
		Owner.ApplyCharacterOverrides(mCharacterBlockState);

		mCurrentBlockTime = mMaxBlockTime;
	}

	protected override void StopAbility_Internal()
	{
		Owner.RemoveCharacterOverrides(mCharacterBlockState);

		mCurrentBlockTime = 0.0f;
	}

	protected override void UpdateAbility_Internal()
	{
		if (mCurrentBlockTime > 0.0f)
		{
			mCurrentBlockTime -= Time.deltaTime;
		}
		else
		{
			StopAbility();
		}
	}

	private void OnGUI()
	{
		if (IsRunning)
		{
			Vector2 screenPosition = Camera.main.WorldToScreenPoint(Owner.transform.position);
			Rect rect = new Rect();
			const float SIZE = 96.0f;
			const float HALF_SIZE = SIZE / 2.0f;
			rect.x = screenPosition.x - HALF_SIZE;
			rect.y = Screen.height - screenPosition.y - HALF_SIZE;
			rect.width = SIZE;
			rect.height = SIZE;
			GUI.DrawTexture(rect, UnityEditor.EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill, true, 0.0f, new Color(1.0f, 0.9f, 0.0f, 0.35f), 0.0f, 0.0f);
			GUI.Label(rect, mCurrentBlockTime.ToString());
		}
	}
}
