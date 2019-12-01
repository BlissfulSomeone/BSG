using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
	public enum ELayer
	{
		HUD,
		Background,
		Menus
	}

	private Dictionary<ELayer, UILayer> mLayers;

	private void Awake()
	{
		mLayers = new Dictionary<ELayer, UILayer>();

		CreateLayer(ELayer.HUD);
		CreateLayer(ELayer.Background);
		CreateLayer(ELayer.Menus);
	}

	private UILayer CreateLayer(ELayer aLayer)
	{
		if (mLayers.ContainsKey(aLayer) == true)
			return mLayers[aLayer];

		GameObject layerObject = new GameObject("UI Layer [" + aLayer.ToString() + "]");
		layerObject.transform.SetParent(transform);
		layerObject.transform.Reset();

		UILayer layer = layerObject.AddComponent<UILayer>();
		mLayers.Add(aLayer, layer);

		return layer;
	}

	public UIMenu PushMenu(ELayer aLayer, UIMenu aMenuPrefab)
	{
		if (mLayers.ContainsKey(aLayer) == false)
		{
			Debug.LogError("Attempting to create a menu for a non-existent layer [" + aLayer.ToString() + "].");
			return null;
		}

		return mLayers[aLayer].PushMenu(aMenuPrefab);
	}

	public void PopMenu(ELayer aLayer)
	{
		if (mLayers.ContainsKey(aLayer) == false)
		{
			Debug.LogError("Attempting to pop a menu from a non-existent layer [" + aLayer.ToString() + "].");
			return;
		}

		mLayers[aLayer].PopMenu();
	}
}
