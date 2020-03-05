using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Character))]
public class CharacterAbilityController : MonoBehaviour
{
	[System.Serializable]
	private struct AbilitySet
	{
		public string InputName;
		public Ability Ability;
	}

	[SerializeField] [DisplayAs("Abilities")] private AbilitySet[] mAbilities;

	private Character mOwner;
	public Character Owner { get { return mOwner; } }

	private void Awake()
	{
		mOwner = GetComponent<Character>();

		for (int i = 0; i < mAbilities.Length; ++i)
		{
			mAbilities[i].Ability.Initialize(Owner);
		}
	}

	private void Update()
	{
		for (int i = 0; i < mAbilities.Length; ++i)
		{
			if (Input.GetButtonDown(mAbilities[i].InputName))
			{
				mAbilities[i].Ability.StartAbility();
			}
			if (Input.GetButtonUp(mAbilities[i].InputName))
			{
				mAbilities[i].Ability.StopAbility();
			}
			mAbilities[i].Ability.UpdateAbility();
		}
	}

	private void FixedUpdate()
	{
		for (int i = 0; i < mAbilities.Length; ++i)
		{
			mAbilities[i].Ability.FixedUpdateAbility();
		}
	}

	public T GetAbility<T>() where T : Ability
	{
		for (int i = 0; i < mAbilities.Length; ++i)
		{
			if (mAbilities[i].Ability is T)
				return (T)mAbilities[i].Ability;
		}
		return null;
	}
}
