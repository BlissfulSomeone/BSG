using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderQueue
{
	public struct RenderQueueTransform
	{
		public Vector3 position;
		public Vector3 scale;

		public RenderQueueTransform(Vector3 aPosition, Vector3 aScale)
		{
			position = aPosition;
			scale = aScale;
		}
	}

	private Dictionary<Sprite, List<RenderQueueTransform>> mQueue;
	public Dictionary<Sprite, List<RenderQueueTransform>> Queue { get { return mQueue; } }

	public RenderQueue()
	{
		mQueue = new Dictionary<Sprite, List<RenderQueueTransform>>();
	}

	public void AddSprite(Sprite aSprite, Vector3 aPosition, Vector3 aScale)
	{
		if (mQueue.ContainsKey(aSprite) == false)
		{
			mQueue.Add(aSprite, new List<RenderQueueTransform>());
		}
		mQueue[aSprite].Add(new RenderQueueTransform(aPosition, aScale));
	}
}

public class ChunkRenderer
{
	private Mesh mMesh;
	private Material mBaseMaterial;
	private Dictionary<Sprite, Material> mMaterials;

	public void Initialize(Mesh aMesh, Material aBaseMaterial)
	{
		mMesh = aMesh;
		mBaseMaterial = aBaseMaterial;

		mMaterials = new Dictionary<Sprite, Material>();
	}

	public void Render(RenderQueue aRenderQueue)
	{
		foreach (KeyValuePair<Sprite, List<RenderQueue.RenderQueueTransform>> kvp in aRenderQueue.Queue)
		{
			if (mMaterials.ContainsKey(kvp.Key) == false)
			{
				Material newMaterial = new Material(mBaseMaterial);
				newMaterial.mainTexture = kvp.Key.texture;
				mMaterials.Add(kvp.Key, newMaterial);
			}

			Material material = mMaterials[kvp.Key];

			foreach (RenderQueue.RenderQueueTransform transform in kvp.Value)
			{
				Graphics.DrawMesh(mMesh, Matrix4x4.TRS(transform.position, Quaternion.identity, transform.scale), material, 0);
			}
		}
	}
}
