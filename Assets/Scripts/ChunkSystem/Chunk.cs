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
			Mesh.subMeshCount = SubmeshCount;
			for (int i = 0; i < Triangles.Length; ++i)
			{
				Mesh.SetTriangles(Triangles[i].ToArray(), i);
			}
		}

		public void AddQuad(int submeshIndex, Vector3 pos0, Vector3 pos1, Vector3 pos2, Vector3 pos3, Vector3 normal, Vector2 uv0, Vector2 uv1, Vector2 uv2, Vector2 uv3)
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

			Triangles[submeshIndex].Add(TriangleIndex + 0);
			Triangles[submeshIndex].Add(TriangleIndex + 1);
			Triangles[submeshIndex].Add(TriangleIndex + 2);
			Triangles[submeshIndex].Add(TriangleIndex + 0);
			Triangles[submeshIndex].Add(TriangleIndex + 2);
			Triangles[submeshIndex].Add(TriangleIndex + 3);

			TriangleIndex += 4;
		}
	}

	public delegate void OnTileDestroyedHandler(Vector3 tilePosition, int tileId, Vector3 explosionSource, float explosionRadius);
	public OnTileDestroyedHandler OnTileDestroyed;
	
	private MeshFilter mMeshFilter;
	private MeshRenderer mMeshRenderer;
	private MeshCollider mMeshCollider;

	private ChunkController.ChunkSettings mChunkSettings;

	private MeshGenerationData mMeshGenerationData;
	private MeshGenerationData mColliderGenerationData;

	private int[] mTiles;

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
		for (int y = 0; y < mChunkSettings.NumberOfRows; ++y)
		{
			for (int x = 0; x < mChunkSettings.NumberOfColumns; ++x)
			{
				if (x == 0 || x == mChunkSettings.NumberOfColumns - 1)
					SetTile(x, y, 1);
				else
					SetTile(x, y, empty ? 0 : 2);
			}
		}
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
				int tileId = GetTile(x, y);
				if (tileId == 0)
					continue;

				Vector3 tilePosition = new Vector2(-mChunkSettings.ChunkWidth / 2 + x * mChunkSettings.TileSize, mChunkSettings.ChunkHeight - y * mChunkSettings.TileSize).ToVec3();

				mMeshGenerationData.AddQuad(
					tileId,
					tilePosition + new Vector3(0, 1, 0) * mChunkSettings.TileSize,
					tilePosition + new Vector3(1, 1, 0) * mChunkSettings.TileSize,
					tilePosition + new Vector3(1, 0, 0) * mChunkSettings.TileSize,
					tilePosition + new Vector3(0, 0, 0) * mChunkSettings.TileSize,
					Vector3.back,
					new Vector2(0, 0),
					new Vector2(1, 0),
					new Vector2(1, 1),
					new Vector2(0, 1));

				if (tileId != GetTile(x + 1, y))
					mMeshGenerationData.AddQuad(
						tileId,
						tilePosition + new Vector3(1, 1, 0) * mChunkSettings.TileSize,
						tilePosition + new Vector3(1, 1, 1) * mChunkSettings.TileSize,
						tilePosition + new Vector3(1, 0, 1) * mChunkSettings.TileSize,
						tilePosition + new Vector3(1, 0, 0) * mChunkSettings.TileSize,
						Vector3.right,
						new Vector2(0, 0),
						new Vector2(1, 0),
						new Vector2(1, 1),
						new Vector2(0, 1));

				if (tileId != GetTile(x - 1, y))
					mMeshGenerationData.AddQuad(
						tileId,
						tilePosition + new Vector3(0, 1, 1) * mChunkSettings.TileSize,
						tilePosition + new Vector3(0, 1, 0) * mChunkSettings.TileSize,
						tilePosition + new Vector3(0, 0, 0) * mChunkSettings.TileSize,
						tilePosition + new Vector3(0, 0, 1) * mChunkSettings.TileSize,
						Vector3.left,
						new Vector2(0, 0),
						new Vector2(1, 0),
						new Vector2(1, 1),
						new Vector2(0, 1));

				if (tileId != GetTile(x, y + 1))
					mMeshGenerationData.AddQuad(
						tileId,
						tilePosition + new Vector3(1, 0, 1) * mChunkSettings.TileSize,
						tilePosition + new Vector3(0, 0, 1) * mChunkSettings.TileSize,
						tilePosition + new Vector3(0, 0, 0) * mChunkSettings.TileSize,
						tilePosition + new Vector3(1, 0, 0) * mChunkSettings.TileSize,
						Vector3.down,
						new Vector2(0, 0),
						new Vector2(1, 0),
						new Vector2(1, 1),
						new Vector2(0, 1));

				if (tileId != GetTile(x, y - 1))
					mMeshGenerationData.AddQuad(
						tileId,
						tilePosition + new Vector3(1, 1, 0) * mChunkSettings.TileSize,
						tilePosition + new Vector3(0, 1, 0) * mChunkSettings.TileSize,
						tilePosition + new Vector3(0, 1, 1) * mChunkSettings.TileSize,
						tilePosition + new Vector3(1, 1, 1) * mChunkSettings.TileSize,
						Vector3.up,
						new Vector2(0, 0),
						new Vector2(1, 0),
						new Vector2(1, 1),
						new Vector2(0, 1));
			}
		}
	}
	
	private void GenerateColliderData()
	{
		for (int y = 0; y < mChunkSettings.NumberOfRows; ++y)
		{
			for (int x = 0; x < mChunkSettings.NumberOfColumns; ++x)
			{
				bool hasCollision = mChunkSettings.TileData[GetTile(x, y)].IsCollision;
				if (!hasCollision)
					continue;

				Vector3 tilePosition = new Vector2(-mChunkSettings.ChunkWidth / 2 + x * mChunkSettings.TileSize, mChunkSettings.ChunkHeight - y * mChunkSettings.TileSize).ToVec3();
				
				if (hasCollision != mChunkSettings.TileData[GetTile(x + 1, y)].IsCollision)
					mColliderGenerationData.AddQuad(
						0,
						tilePosition + new Vector3(1, 1, 0) * mChunkSettings.TileSize,
						tilePosition + new Vector3(1, 1, 1) * mChunkSettings.TileSize,
						tilePosition + new Vector3(1, 0, 1) * mChunkSettings.TileSize,
						tilePosition + new Vector3(1, 0, 0) * mChunkSettings.TileSize,
						Vector3.right,
						new Vector2(0, 0),
						new Vector2(1, 0),
						new Vector2(1, 1),
						new Vector2(0, 1));

				if (hasCollision != mChunkSettings.TileData[GetTile(x - 1, y)].IsCollision)
					mColliderGenerationData.AddQuad(
						0,
						tilePosition + new Vector3(0, 1, 1) * mChunkSettings.TileSize,
						tilePosition + new Vector3(0, 1, 0) * mChunkSettings.TileSize,
						tilePosition + new Vector3(0, 0, 0) * mChunkSettings.TileSize,
						tilePosition + new Vector3(0, 0, 1) * mChunkSettings.TileSize,
						Vector3.left,
						new Vector2(0, 0),
						new Vector2(1, 0),
						new Vector2(1, 1),
						new Vector2(0, 1));

				if (hasCollision != mChunkSettings.TileData[GetTile(x, y + 1)].IsCollision)
					mColliderGenerationData.AddQuad(
						0,
						tilePosition + new Vector3(1, 0, 1) * mChunkSettings.TileSize,
						tilePosition + new Vector3(0, 0, 1) * mChunkSettings.TileSize,
						tilePosition + new Vector3(0, 0, 0) * mChunkSettings.TileSize,
						tilePosition + new Vector3(1, 0, 0) * mChunkSettings.TileSize,
						Vector3.down,
						new Vector2(0, 0),
						new Vector2(1, 0),
						new Vector2(1, 1),
						new Vector2(0, 1));

				if (hasCollision != mChunkSettings.TileData[GetTile(x, y - 1)].IsCollision)
					mColliderGenerationData.AddQuad(
						0,
						tilePosition + new Vector3(1, 1, 0) * mChunkSettings.TileSize,
						tilePosition + new Vector3(0, 1, 0) * mChunkSettings.TileSize,
						tilePosition + new Vector3(0, 1, 1) * mChunkSettings.TileSize,
						tilePosition + new Vector3(1, 1, 1) * mChunkSettings.TileSize,
						Vector3.up,
						new Vector2(0, 0),
						new Vector2(1, 0),
						new Vector2(1, 1),
						new Vector2(0, 1));
			}
		}
	}

	private int GetTile(int x, int y)
	{
		if (x < 0 || x >= mChunkSettings.NumberOfColumns || y < 0 || y >= mChunkSettings.NumberOfRows)
			return 0;
		return mTiles[x + y * mChunkSettings.NumberOfColumns];
	}

	private void SetTile(int x, int y, int tileId)
	{
		if (x < 0 || x >= mChunkSettings.NumberOfColumns || y < 0 || y >= mChunkSettings.NumberOfRows)
			return;
		mTiles[x + y * mChunkSettings.NumberOfColumns] = tileId;
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

	public void Explode(Vector2 explosionSource, float explosionRadius)
	{
		bool dirty = false;
		for (int y = 0; y < mChunkSettings.NumberOfRows; ++y)
		{
			for (int x = 0; x < mChunkSettings.NumberOfColumns; ++x)
			{
				int tileId = GetTile(x, y);
				bool isIndistructible = mChunkSettings.TileData[tileId].IsIndestructible;
				if (isIndistructible)
					continue;

				Vector2 tilePosition = transform.position.ToVec2() + new Vector2(-mChunkSettings.ChunkWidth / 2 + x * mChunkSettings.TileSize, mChunkSettings.ChunkHeight - y * mChunkSettings.TileSize);
				float distance = Vector2.Distance(tilePosition, explosionSource);

				if (distance <= explosionRadius)
				{
					SetTile(x, y, 0);
					OnTileDestroyed(tilePosition.ToVec3() + Vector3.one * mChunkSettings.TileSize / 2, tileId, explosionSource.ToVec3() + Vector3.forward * mChunkSettings.TileSize / 2, explosionRadius);
					dirty = true;
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
}
