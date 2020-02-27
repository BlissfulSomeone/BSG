using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{
	[System.Serializable]
	private class MeshGenerationData
	{
		public Mesh Mesh;
		public int SubmeshCount;
		public List<Vector3> Vertices;
		public List<Vector3> Normals;
		public List<Vector2> Uvs;
		public List<Color> Colors;
		public List<int>[] Triangles;
		public int TriangleIndex;
		
		public void Sanitize(int submeshCount)
		{
			SubmeshCount = submeshCount;

			if (Mesh == null)
				Mesh = new Mesh();
			if (Vertices == null)
				Vertices = new List<Vector3>();
			if (Normals == null)
				Normals = new List<Vector3>();
			if (Uvs == null)
				Uvs = new List<Vector2>();
			if (Colors == null)
				Colors = new List<Color>();
			if (Triangles == null || Triangles.Length != SubmeshCount)
			{
				Triangles = new List<int>[SubmeshCount];
				for (int i = 0; i < Triangles.Length; ++i)
				{
					Triangles[i] = new List<int>();
				}
			}
		}

		public void Reset()
		{
			Mesh.Clear();
			Vertices.Clear();
			Normals.Clear();
			Uvs.Clear();
			Colors.Clear();
			for (int i = 0; i < Triangles.Length; ++i)
			{
				Triangles[i].Clear();
			}
			TriangleIndex = 0;
		}

		public void Build()
		{
			Mesh.Clear();
			Mesh.vertices = Vertices.ToArray();
			Mesh.normals = Normals.ToArray();
			Mesh.uv = Uvs.ToArray();
			Mesh.colors = Colors.ToArray();
			Mesh.subMeshCount = SubmeshCount;
			for (int i = 0; i < Triangles.Length; ++i)
			{
				Mesh.SetTriangles(Triangles[i].ToArray(), i);
			}
		}

		public void AddQuad(int submeshIndex, Vector3 pos0, Vector3 pos1, Vector3 pos2, Vector3 pos3, Vector3 normal, Vector2 uv0, Vector2 uv1, Vector2 uv2, Vector2 uv3, Color color)
		{
			Vertices.Add(pos0);
			Vertices.Add(pos1);
			Vertices.Add(pos2);
			Vertices.Add(pos3);

			Normals.Add(normal);
			Normals.Add(normal);
			Normals.Add(normal);
			Normals.Add(normal);

			Uvs.Add(uv0);
			Uvs.Add(uv1);
			Uvs.Add(uv2);
			Uvs.Add(uv3);

			Colors.Add(color);
			Colors.Add(color);
			Colors.Add(color);
			Colors.Add(color);

			Triangles[submeshIndex].Add(TriangleIndex + 0);
			Triangles[submeshIndex].Add(TriangleIndex + 1);
			Triangles[submeshIndex].Add(TriangleIndex + 2);
			Triangles[submeshIndex].Add(TriangleIndex + 0);
			Triangles[submeshIndex].Add(TriangleIndex + 2);
			Triangles[submeshIndex].Add(TriangleIndex + 3);

			TriangleIndex += 4;
		}
	}

	public delegate void OnTileDestroyedHandler(Vector3 tilePosition, int tileId, ExplosionInstance explosionInstance);
	public OnTileDestroyedHandler OnTileDestroyed;
	
	private MeshFilter mMeshFilter;
	private MeshRenderer mMeshRenderer;
	private MeshCollider mMeshCollider;

	private ChunkController.ChunkSettings mChunkSettings;

	private MeshGenerationData mMeshGenerationData;
	private MeshGenerationData mColliderGenerationData;

	private int[] mTiles;
	private float[] mTileHealths;

	public void Generate(ChunkController.ChunkSettings chunkSettings, bool empty)
	{
		mChunkSettings = chunkSettings;

		Sanitize();
		GenerateTiles(empty);
		ResetData();
		GenerateMeshData();
		GenerateColliderData();
		Build();
	}

	private void Sanitize()
	{
		if (mMeshFilter == null)
			mMeshFilter = GetComponent<MeshFilter>();
		if (mMeshRenderer == null)
			mMeshRenderer = GetComponent<MeshRenderer>();
		if (mMeshCollider == null)
			mMeshCollider = GetComponent<MeshCollider>();

		if (mMeshGenerationData == null)
			mMeshGenerationData = new MeshGenerationData();
		if (mColliderGenerationData == null)
			mColliderGenerationData = new MeshGenerationData();

		mMeshGenerationData.Sanitize(mChunkSettings.TileData.Length);
		mColliderGenerationData.Sanitize(mChunkSettings.TileData.Length);
	}

	private void GenerateTiles(bool empty)
	{
		mTiles = new int[mChunkSettings.NumberOfTiles];
		mTileHealths = new float[mChunkSettings.NumberOfTiles];
		for (int y = 0; y < mChunkSettings.NumberOfRows; ++y)
		{
			for (int x = 0; x < mChunkSettings.NumberOfColumns; ++x)
			{
				for (int z = 0; z < mChunkSettings.NumberOfLayers; ++z)
				{
					if (x == 0 || x == mChunkSettings.NumberOfColumns - 1)
						SetTile(x, y, z, 1);
					else
						SetTile(x, y, z, empty ? 0 : GetGroundType(x, y, z));
				}
			}
		}
	}

	private int GetGroundType(int x, int y, int z)
	{
		float actualY = y - transform.position.y;
		float perlin = Mathf.PerlinNoise(x / mChunkSettings.GenerationPerlinSize, actualY / mChunkSettings.GenerationPerlinSize);
		float height = actualY * Mathf.Max(mChunkSettings.GenerationRampSpeed / 10000.0f, 0.000001f);
		float threshold = 0.5f;
		float value = perlin * height;
		
		return value < threshold ? 2 : 3;
	}

	private void ResetData()
	{
		mMeshGenerationData.Reset();
		mColliderGenerationData.Reset();
	}

	private void GenerateMeshData()
	{
		for (int y = 0; y < mChunkSettings.NumberOfRows; ++y)
		{
			for (int x = 0; x < mChunkSettings.NumberOfColumns; ++x)
			{
				for (int z = 0; z < mChunkSettings.NumberOfLayers; ++z)
				{
					int tileId = GetTile(x, y, z);
					if (tileId == 0)
						continue;
					
					Vector3 tilePosition = GetTileLocalPosition(x, y, z);
					Color layerColor = Color.Lerp(Color.white, mChunkSettings.BackLayerTint, z / (float)(mChunkSettings.NumberOfLayers - 1));

					mMeshGenerationData.AddQuad(
						tileId,
						tilePosition - Vector3.one / 2 + new Vector3(0, 1, 0) * mChunkSettings.TileSize,
						tilePosition - Vector3.one / 2 + new Vector3(1, 1, 0) * mChunkSettings.TileSize,
						tilePosition - Vector3.one / 2 + new Vector3(1, 0, 0) * mChunkSettings.TileSize,
						tilePosition - Vector3.one / 2 + new Vector3(0, 0, 0) * mChunkSettings.TileSize,
						Vector3.back,
						new Vector2(0, 0),
						new Vector2(1, 0),
						new Vector2(1, 1),
						new Vector2(0, 1),
						layerColor);

					if (tileId != GetTile(x + 1, y, z))
						mMeshGenerationData.AddQuad(
							tileId,
							tilePosition - Vector3.one / 2 + new Vector3(1, 1, 0) * mChunkSettings.TileSize,
							tilePosition - Vector3.one / 2 + new Vector3(1, 1, 1) * mChunkSettings.TileSize,
							tilePosition - Vector3.one / 2 + new Vector3(1, 0, 1) * mChunkSettings.TileSize,
							tilePosition - Vector3.one / 2 + new Vector3(1, 0, 0) * mChunkSettings.TileSize,
							Vector3.right,
							new Vector2(0, 0),
							new Vector2(1, 0),
							new Vector2(1, 1),
							new Vector2(0, 1),
							layerColor);

					if (tileId != GetTile(x - 1, y, z))
						mMeshGenerationData.AddQuad(
							tileId,
							tilePosition - Vector3.one / 2 + new Vector3(0, 1, 1) * mChunkSettings.TileSize,
							tilePosition - Vector3.one / 2 + new Vector3(0, 1, 0) * mChunkSettings.TileSize,
							tilePosition - Vector3.one / 2 + new Vector3(0, 0, 0) * mChunkSettings.TileSize,
							tilePosition - Vector3.one / 2 + new Vector3(0, 0, 1) * mChunkSettings.TileSize,
							Vector3.left,
							new Vector2(0, 0),
							new Vector2(1, 0),
							new Vector2(1, 1),
							new Vector2(0, 1),
							layerColor);

					if (tileId != GetTile(x, y + 1, z))
						mMeshGenerationData.AddQuad(
							tileId,
							tilePosition - Vector3.one / 2 + new Vector3(1, 0, 1) * mChunkSettings.TileSize,
							tilePosition - Vector3.one / 2 + new Vector3(0, 0, 1) * mChunkSettings.TileSize,
							tilePosition - Vector3.one / 2 + new Vector3(0, 0, 0) * mChunkSettings.TileSize,
							tilePosition - Vector3.one / 2 + new Vector3(1, 0, 0) * mChunkSettings.TileSize,
							Vector3.down,
							new Vector2(0, 0),
							new Vector2(1, 0),
							new Vector2(1, 1),
							new Vector2(0, 1),
							layerColor);

					if (tileId != GetTile(x, y - 1, z))
						mMeshGenerationData.AddQuad(
							tileId,
							tilePosition - Vector3.one / 2 + new Vector3(1, 1, 0) * mChunkSettings.TileSize,
							tilePosition - Vector3.one / 2 + new Vector3(0, 1, 0) * mChunkSettings.TileSize,
							tilePosition - Vector3.one / 2 + new Vector3(0, 1, 1) * mChunkSettings.TileSize,
							tilePosition - Vector3.one / 2 + new Vector3(1, 1, 1) * mChunkSettings.TileSize,
							Vector3.up,
							new Vector2(0, 0),
							new Vector2(1, 0),
							new Vector2(1, 1),
							new Vector2(0, 1),
							layerColor);
				}
			}
		}
	}
	
	private void GenerateColliderData()
	{
		for (int y = 0; y < mChunkSettings.NumberOfRows; ++y)
		{
			for (int x = 0; x < mChunkSettings.NumberOfColumns; ++x)
			{
				bool hasCollision = mChunkSettings.TileData[GetTile(x, y, 0)].IsCollision;
				if (!hasCollision)
					continue;
				
				Vector3 tilePosition = GetTileLocalPosition(x, y, 0);

				if (hasCollision != mChunkSettings.TileData[GetTile(x + 1, y, 0)].IsCollision)
					mColliderGenerationData.AddQuad(
						0,
						tilePosition - Vector3.one / 2 + new Vector3(1, 1, 0) * mChunkSettings.TileSize,
						tilePosition - Vector3.one / 2 + new Vector3(1, 1, 1) * mChunkSettings.TileSize,
						tilePosition - Vector3.one / 2 + new Vector3(1, 0, 1) * mChunkSettings.TileSize,
						tilePosition - Vector3.one / 2 + new Vector3(1, 0, 0) * mChunkSettings.TileSize,
						Vector3.right,
						new Vector2(0, 0),
						new Vector2(1, 0),
						new Vector2(1, 1),
						new Vector2(0, 1),
						Color.white);

				if (hasCollision != mChunkSettings.TileData[GetTile(x - 1, y, 0)].IsCollision)
					mColliderGenerationData.AddQuad(
						0,
						tilePosition - Vector3.one / 2 + new Vector3(0, 1, 1) * mChunkSettings.TileSize,
						tilePosition - Vector3.one / 2 + new Vector3(0, 1, 0) * mChunkSettings.TileSize,
						tilePosition - Vector3.one / 2 + new Vector3(0, 0, 0) * mChunkSettings.TileSize,
						tilePosition - Vector3.one / 2 + new Vector3(0, 0, 1) * mChunkSettings.TileSize,
						Vector3.left,
						new Vector2(0, 0),
						new Vector2(1, 0),
						new Vector2(1, 1),
						new Vector2(0, 1),
						Color.white);

				if (hasCollision != mChunkSettings.TileData[GetTile(x, y + 1, 0)].IsCollision)
					mColliderGenerationData.AddQuad(
						0,
						tilePosition - Vector3.one / 2 + new Vector3(1, 0, 1) * mChunkSettings.TileSize,
						tilePosition - Vector3.one / 2 + new Vector3(0, 0, 1) * mChunkSettings.TileSize,
						tilePosition - Vector3.one / 2 + new Vector3(0, 0, 0) * mChunkSettings.TileSize,
						tilePosition - Vector3.one / 2 + new Vector3(1, 0, 0) * mChunkSettings.TileSize,
						Vector3.down,
						new Vector2(0, 0),
						new Vector2(1, 0),
						new Vector2(1, 1),
						new Vector2(0, 1),
						Color.white);

				if (hasCollision != mChunkSettings.TileData[GetTile(x, y - 1, 0)].IsCollision)
					mColliderGenerationData.AddQuad(
						0,
						tilePosition - Vector3.one / 2 + new Vector3(1, 1, 0) * mChunkSettings.TileSize,
						tilePosition - Vector3.one / 2 + new Vector3(0, 1, 0) * mChunkSettings.TileSize,
						tilePosition - Vector3.one / 2 + new Vector3(0, 1, 1) * mChunkSettings.TileSize,
						tilePosition - Vector3.one / 2 + new Vector3(1, 1, 1) * mChunkSettings.TileSize,
						Vector3.up,
						new Vector2(0, 0),
						new Vector2(1, 0),
						new Vector2(1, 1),
						new Vector2(0, 1),
						Color.white);
			}
		}
	}

	private int GetTile(int x, int y, int z)
	{
		if (x < 0 || x >= mChunkSettings.NumberOfColumns || y < 0 || y >= mChunkSettings.NumberOfRows || z < 0 || z >= mChunkSettings.NumberOfLayers)
			return 0;
		int index = x + y * mChunkSettings.NumberOfColumns + z * mChunkSettings.NumberOfColumns * mChunkSettings.NumberOfRows;
		return mTiles[index];
	}

	private void SetTile(int x, int y, int z, int tileId)
	{
		if (x < 0 || x >= mChunkSettings.NumberOfColumns || y < 0 || y >= mChunkSettings.NumberOfRows || z < 0 || z >= mChunkSettings.NumberOfLayers)
			return;
		int index = x + y * mChunkSettings.NumberOfColumns + z * mChunkSettings.NumberOfColumns * mChunkSettings.NumberOfRows;
		mTiles[index] = tileId;
		mTileHealths[index] = mChunkSettings.TileData[tileId].Health;
	}

	private void Build()
	{
		mMeshGenerationData.Build();
		mColliderGenerationData.Build();

		mMeshFilter.mesh = mMeshGenerationData.Mesh;
		
		Material[] materials = new Material[mChunkSettings.TileData.Length];
		for (int i = 0; i < materials.Length; ++i)
		{
			materials[i] = mChunkSettings.TileData[i].Material;
		}
		mMeshRenderer.materials = materials;

		mMeshCollider.sharedMesh = mColliderGenerationData.Mesh;
		mMeshCollider.convex = false;
	}

	public void Explode(ExplosionInstance explosionInstance)
	{
		bool dirty = false;
		for (int y = 0; y < mChunkSettings.NumberOfRows; ++y)
		{
			for (int x = 0; x < mChunkSettings.NumberOfColumns; ++x)
			{
				for (int z = 0; z < mChunkSettings.NumberOfLayers; ++z)
				{
					int tileId = GetTile(x, y, z);
					bool isIndistructible = mChunkSettings.TileData[tileId].IsIndestructible;
					if (isIndistructible)
						continue;
					
					Vector3 tilePosition = GetTileWorldPosition(x, y, z);
					float distance = Vector3.Distance(tilePosition, explosionInstance.Position);

					if (distance <= explosionInstance.ExplosionData.Radius)
					{
						int index = x + y * mChunkSettings.NumberOfColumns + z * mChunkSettings.NumberOfColumns * mChunkSettings.NumberOfRows;
						mTileHealths[index] -= explosionInstance.ExplosionData.Damage;
						if (mTileHealths[index] <= 0)
						{
							SetTile(x, y, z, 0);
							OnTileDestroyed(tilePosition, tileId, explosionInstance);
							dirty = true;
						}
					}
				}
			}
		}
		
		if (dirty)
		{
			Sanitize();
			ResetData();
			GenerateMeshData();
			GenerateColliderData();
			Build();
		}
	}
	
	private Vector3 GetTileLocalPosition(int x, int y, int z)
	{
		Vector3 localCenterPosition = new Vector3(
			-mChunkSettings.ChunkWidth / 2 + x * mChunkSettings.TileSize + mChunkSettings.TileSize / 2,
			mChunkSettings.ChunkHeight - y * mChunkSettings.TileSize - mChunkSettings.TileSize / 2,
			z * mChunkSettings.TileSize);
		return localCenterPosition;
	}

	private Vector3 GetTileWorldPosition(int x, int y, int z)
	{
		return transform.position + GetTileLocalPosition(x, y, z);
	}
}
