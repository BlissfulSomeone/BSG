using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Character))]
public class CharacterHealthController : MonoBehaviour
{
	public delegate void OnKilledHandler(Character aKilledCharacter);
	public OnKilledHandler OnKilled;

	[SerializeField] [DisplayAs("Max Health")] private float mMaxHealth;

	private Character mOwner;
	public Character Owner { get { return mOwner; } }

	private Triggerable mTriggerable;

	private float mHealth;

	public float Health { get { return mHealth; } }
	public float MaxHealth { get { return mMaxHealth; } }

	private void Awake()
	{
		mOwner = GetComponent<Character>();
		mTriggerable = GetComponent<Triggerable>();

		mTriggerable.OnTriggered += OnTriggered;

		mHealth = mMaxHealth;
	}

	private void OnDestroy()
	{
		mTriggerable.OnTriggered -= OnTriggered;
	}

	private void OnTriggered(ExplosionInstance explosionInstance)
	{
		if (!explosionInstance.ExplosionData.Friendly && Owner.OverrideController.IsOverrideEnabled(Owner.OverrideController.CurrentOverrides.CanBeHurt))
		{
			mHealth -= explosionInstance.ExplosionData.Damage;
			if (mHealth <= 0.0f)
			{
				OnKilled?.Invoke(Owner);
				Destroy(gameObject);
			}
		}
	}
}
