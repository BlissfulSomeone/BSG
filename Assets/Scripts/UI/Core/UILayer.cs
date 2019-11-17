using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILayer : MonoBehaviour
{
	private List<UIMenu> mMenus;

	private void Awake()
	{
		mMenus = new List<UIMenu>();
	}

	public UIMenu PushMenu(UIMenu aMenuPrefab)
	{
		UIMenu menu = Instantiate(aMenuPrefab, transform);
		menu.transform.Reset();

		if (mMenus.Count > 0)
		{
			UIMenu topMenu = mMenus.Last();
			topMenu.Alpha = 0.0f;
			topMenu.Interactable = false;
		}

		mMenus.Add(menu);
		menu.Alpha = 1.0f;
		menu.Interactable = true;

		return menu;
	}

	public void PopMenu()
	{
		if (mMenus.Count > 0)
		{
			Destroy(mMenus.Last().gameObject);
			mMenus.RemoveLast();
			if (mMenus.Count > 0)
			{
				UIMenu topMenu = mMenus.Last();
				topMenu.Alpha = 1.0f;
				topMenu.Interactable = true;
			}
		}
	}
}
