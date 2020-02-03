using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILayer : MonoBehaviour
{
	public enum ELayerType
	{
		Single,
		Multi
	}

	private List<UIMenu> mMenus;
	private ELayerType mType;

	public ELayerType LayerType { get { return mType; } set { mType = value; UpdateMenuVisibilities(); } }

	private void Awake()
	{
		mMenus = new List<UIMenu>();
	}

	public UIMenu PushMenu(UIMenu aMenuPrefab)
	{
		if (mMenus.Count > 0 && LayerType == ELayerType.Single)
		{
			UIMenu topMenu = mMenus.Last();
			topMenu.Alpha = 0.0f;
			topMenu.Interactable = false;
		}

		UIMenu menu = CreateMenu(aMenuPrefab);
		menu.Alpha = 1.0f;
		menu.Interactable = true;
		mMenus.Add(menu);

		return menu;
	}

	private UIMenu CreateMenu(UIMenu aMenuPrefab)
	{
		UIMenu menu = Instantiate(aMenuPrefab, transform);
		menu.transform.SetParent(transform);
		menu.transform.Reset();
		menu.Initialize(this);
		return menu;
	}

	public void PopMenu()
	{
		if (mMenus.Count > 0 && LayerType == ELayerType.Single)
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

	private void UpdateMenuVisibilities()
	{ 
		for (int i = 0; i < mMenus.Count; ++i)
		{
			bool isVisible = i == mMenus.LastIndex() || mType == ELayerType.Multi;
			mMenus[i].Alpha = isVisible ? 1.0f : 0.0f;
			mMenus[i].Interactable = isVisible;
		}
	}
}
