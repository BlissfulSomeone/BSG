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

			if (submeshIndex >= Triangles.Length)
			{
				Debug.LogError("Index: " + submeshIndex + ", Length: " + Triangles.Length);
			}
			Triangles[submeshIndex].Add(TriangleIndex + 0);
			Triangles[submeshIndex].Add(TriangleIndex + 1);
			Triangles[submeshIndex].Add(TriangleIndex + 2);
			Triangles[submeshIndex].Add(TriangleIndex + 0);
			Triangles[submeshIndex].Add(TriangleIndex + 2);
			Triangles[submeshIndex].Add(TriangleIndex + 3);

			TriangleIndex += 4;
		}
	}

	public delegate void OnTileDestroyedHandler(Vector3 tilePosition, int tileId, int depth, int variant, ExplosionInstance explosionInstance);
	public OnTileDestroyedHandler OnTileDestroyed;

	private int mChunkIndex;

	private MeshFilter mMeshFilter;
	private MeshRenderer mMeshRenderer;
	private MeshCollider mMeshCollider;

	private ChunkController mController;
	private ChunkController.ChunkSettings mChunkSettings;

	private MeshGenerationData mMeshGenerationData;
	private MeshGenerationData mColliderGenerationData;

	private int[] mTiles;
	private float[] mTileHealths;
	private int[] mTileDepth;
	private int[] mTileVariant;

	public void Generate(ChunkController controller,int chunkIndex, ChunkController.ChunkSettings chunkSettings, bool empty)
	{
		mController = controller;
		mChunkIndex = chunkIndex;
		mChunkSettings = chunkSettings;

		Sanitize();
		GenerateTiles(empty);
		ResetData();
		CalculateTileDepth();
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

		int numberOfSubmeshes = TotalMaterials();

		mMeshGenerationData.Sanitize(numberOfSubmeshes);
		mColliderGenerationData.Sanitize(1);
	}

	private void GenerateTiles(bool empty)
	{
		mTiles = new int[mChunkSettings.NumberOfTiles];
		mTileHealths = new float[mChunkSettings.NumberOfTiles];
		mTileDepth = new int[mChunkSettings.NumberOfTiles];
		mTileVariant = new int[mChunkSettings.NumberOfTiles];
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
		const float THRESHOLD = 0.5f;
		float actualY = y - transform.position.y;
		float perlin = Mathf.PerlinNoise(x * mChunkSettings.GenerationPerlinSize, actualY * mChunkSettings.GenerationPerlinSize);
		return (perlin + Mathf.Max(mChunkSettings.GenerationAmount - 0.5f, 0.0f)) < THRESHOLD ? 2 : 3;
	}

	private void ResetData()
	{
		mMeshGenerationData.Reset();
		mColliderGenerationData.Reset();
	}

	private void CalculateTileDepth()
	{
		for (int y = 0; y < mChunkSettings.NumberOfRows; ++y)
		{
			for (int x = 0; x < mChunkSettings.NumberOfColumns; ++x)
			{
				for (int z = 0; z < mChunkSettings.NumberOfLayers; ++z)
				{
					CalculateDepthForTile(x, y, z);
				}
			}
		}
	}
	
	private Vector2Int[] Depth1Lookup = new Vector2Int[]
	{
		new Vector2Int(1, 0),
		new Vector2Int(1, 1),
		new Vector2Int(0, 1),
		new Vector2Int(-1, 1),
		new Vector2Int(-1, 0),
		new Vector2Int(-1, -1),
		new Vector2Int(0, -1),
		new Vector2Int(1, -1),
	};
	private Vector2Int[] Depth2Lookup = new Vector2Int[]
	{
		new Vector2Int(2, 0),
		new Vector2Int(2, 1),
		new Vector2Int(2, 2),
		new Vector2Int(1, 2),
		new Vector2Int(0, 2),
		new Vector2Int(-1, 2),
		new Vector2Int(-2, 2),
		new Vector2Int(-2, 1),
		new Vector2Int(-2, 0),
		new Vector2Int(-2, -1),
		new Vector2Int(-2, -2),
		new Vector2Int(-1, -2),
		new Vector2Int(0, -2),
		new Vector2Int(1, -2),
		new Vector2Int(2, -2),
		new Vector2Int(2, -1),
	};

	private void CalculateDepthForTile(int x, int y, int z)
	{
		int index = GetTileIndex(x, y, z);
		int previousDepth = mTileDepth[index];

		int tileId = mTiles[index];
		if (!mChunkSettings.TileData[tileId].IsCollision)
		{
			mTileDepth[index] = 0;
		}
		else
		{
			mTileDepth[index] = 3;

			if (!TestDepth(ref Depth1Lookup, index, 1))
			{
				TestDepth(ref Depth2Lookup, index, 2);
			}
		}
		
		if (previousDepth != mTileDepth[index])
		{
			if (mTileDepth[index] == 1)
				mTileVariant[index] = Random.Range(0, mChunkSettings.TileData[mTiles[index]].Depth1Materials.Length);
			else if (mTileDepth[index] == 2)
				mTileVariant[index] = Random.Range(0, mChunkSettings.TileData[mTiles[index]].Depth2Materials.Length);
			else if (mTileDepth[index] == 3)
				mTileVariant[index] = Random.Range(0, mChunkSettings.TileData[mTiles[index]].Depth3Materials.Length);
		}
	}

	private bool TestDepth(ref Vector2Int[] lookupReference, int currentIndex, int depth)
	{
		int currentX = currentIndex % mChunkSettings.NumberOfColumns;
		int currentY = (currentIndex / mChunkSettings.NumberOfColumns) % mChunkSettings.NumberOfRows;
		int currentZ = (currentIndex / mChunkSettings.NumberOfColumns) / mChunkSettings.NumberOfRows;

		for (int i = 0; i < lookupReference.Length; ++i)
		{
			int nextX = currentX + lookupReference[i].x;
			int nextY = currentY + lookupReference[i].y;
			if (nextX < 0 || nextX >= mChunkSettings.NumberOfColumns)
				continue;

			if (nextY >= 0 && nextY < mChunkSettings.NumberOfRows)
			{
				int nextId = GetTileId(nextX, nextY, currentZ);
				if (!mChunkSettings.TileData[nextId].IsCollision)
				{
					mTileDepth[currentIndex] = depth;
					return true;
				}
			}
			else
			{
				Chunk otherChunk = null;
				if (nextY < 0)
				{
					otherChunk = mController.GetChunk(mChunkIndex - 1);
					nextY = mChunkSettings.NumberOfRows + nextY;
				}
				else
				{
					otherChunk = mController.GetChunk(mChunkIndex + 1);
					nextY = nextY - mChunkSettings.NumberOfRows;
				}
				if (otherChunk != null)
				{
					int nextId = otherChunk.GetTileId(nextX, nextY, currentZ);
					if (!mChunkSettings.TileData[nextId].IsCollision)
					{
						mTileDepth[currentIndex] = depth;
						return true;
					}
				}
			}
		}

		return false;
	}
	
	private void GenerateMeshData()
	{
		for (int y = 0; y < mChunkSettings.NumberOfRows; ++y)
		{
			for (int x = 0; x < mChunkSettings.NumberOfColumns; ++x)
			{
				for (int z = 0; z < mChunkSettings.NumberOfLayers; ++z)
				{
					int tileId = GetTileId(x, y, z);
					if (tileId == 0)
						continue;

					int index = GetTileIndex(x, y, z);
					int submeshIndex = mChunkSettings.GetSubmeshIndex(tileId, mTileDepth[index], mTileVariant[index]);
					
					Vector3 tilePosition = GetTileLocalPosition(x, y, z);
					Color layerColor = Color.Lerp(Color.white, mChunkSettings.BackLayerTint, z / (float)(mChunkSettings.NumberOfLayers - 1));
					layerColor *= mChunkSettings.TileData[tileId].TileColor;
					layerColor *= Color.Lerp(mChunkSettings.LightColorTint, mChunkSettings.ShadowColorTint, (mTileDepth[index] - 1.0f) / 2);

					// Front
					mMeshGenerationData.AddQuad(
						submeshIndex,
						tilePosition - Vector3.one / 2 + new Vector3(0, 1, 0) * mChunkSettings.TileSize,
						tilePosition - Vector3.one / 2 + new Vector3(1, 1, 0) * mChunkSettings.TileSize,
						tilePosition - Vector3.one / 2 + new Vector3(1, 0, 0) * mChunkSettings.TileSize,
						tilePosition - Vector3.one / 2 + new Vector3(0, 0, 0) * mChunkSettings.TileSize,
						Vector3.back,
						new Vector2(1, 2) * (1.0f / 3.0f),
						new Vector2(2, 2) * (1.0f / 3.0f),
						new Vector2(2, 1) * (1.0f / 3.0f),
						new Vector2(1, 1) * (1.0f / 3.0f),
						layerColor);

					// Right
					if (tileId != GetTileId(x + 1, y, z))
						mMeshGenerationData.AddQuad(
							submeshIndex,
							tilePosition - Vector3.one / 2 + new Vector3(1, 1, 0) * mChunkSettings.TileSize,
							tilePosition - Vector3.one / 2 + new Vector3(1, 1, 1) * mChunkSettings.TileSize,
							tilePosition - Vector3.one / 2 + new Vector3(1, 0, 1) * mChunkSettings.TileSize,
							tilePosition - Vector3.one / 2 + new Vector3(1, 0, 0) * mChunkSettings.TileSize,
							Vector3.right,
							new Vector2(2, 2) * (1.0f / 3.0f),
							new Vector2(3, 2) * (1.0f / 3.0f),
							new Vector2(3, 1) * (1.0f / 3.0f),
							new Vector2(2, 1) * (1.0f / 3.0f),
							layerColor);

					// Left
					if (tileId != GetTileId(x - 1, y, z))
						mMeshGenerationData.AddQuad(
							submeshIndex,
							tilePosition - Vector3.one / 2 + new Vector3(0, 1, 1) * mChunkSettings.TileSize,
							tilePosition - Vector3.one / 2 + new Vector3(0, 1, 0) * mChunkSettings.TileSize,
							tilePosition - Vector3.one / 2 + new Vector3(0, 0, 0) * mChunkSettings.TileSize,
							tilePosition - Vector3.one / 2 + new Vector3(0, 0, 1) * mChunkSettings.TileSize,
							Vector3.left,
							new Vector2(0, 2) * (1.0f / 3.0f),
							new Vector2(1, 2) * (1.0f / 3.0f),
							new Vector2(1, 1) * (1.0f / 3.0f),
							new Vector2(0, 1) * (1.0f / 3.0f),
							layerColor);

					// Bottom
					if (tileId != GetTileId(x, y + 1, z))
						mMeshGenerationData.AddQuad(
							submeshIndex,
							tilePosition - Vector3.one / 2 + new Vector3(1, 0, 1) * mChunkSettings.TileSize,
							tilePosition - Vector3.one / 2 + new Vector3(0, 0, 1) * mChunkSettings.TileSize,
							tilePosition - Vector3.one / 2 + new Vector3(0, 0, 0) * mChunkSettings.TileSize,
							tilePosition - Vector3.one / 2 + new Vector3(1, 0, 0) * mChunkSettings.TileSize,
							Vector3.down,
							new Vector2(2, 0) * (1.0f / 3.0f),
							new Vector2(1, 0) * (1.0f / 3.0f),
							new Vector2(1, 1) * (1.0f / 3.0f),
							new Vector2(2, 1) * (1.0f / 3.0f),
							layerColor);

					// Top
					if (tileId != GetTileId(x, y - 1, z))
						mMeshGenerationData.AddQuad(
							submeshIndex,
							tilePosition - Vector3.one / 2 + new Vector3(1, 1, 0) * mChunkSettings.TileSize,
							tilePosition - Vector3.one / 2 + new Vector3(0, 1, 0) * mChunkSettings.TileSize,
							tilePosition - Vector3.one / 2 + new Vector3(0, 1, 1) * mChunkSettings.TileSize,
							tilePosition - Vector3.one / 2 + new Vector3(1, 1, 1) * mChunkSettings.TileSize,
							Vector3.up,
							new Vector2(2, 2) * (1.0f / 3.0f),
							new Vector2(1, 2) * (1.0f / 3.0f),
							new Vector2(1, 3) * (1.0f / 3.0f),
							new Vector2(2, 3) * (1.0f / 3.0f),
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
				bool hasCollision = mChunkSettings.TileData[GetTileId(x, y, mChunkSettings.ClampedPlayableLayer)].IsCollision;
				if (!hasCollision)
					continue;
				
				Vector3 tilePosition = GetTileLocalPosition(x, y, mChunkSettings.ClampedPlayableLayer);

				if (hasCollision != mChunkSettings.TileData[GetTileId(x + 1, y, mChunkSettings.ClampedPlayableLayer)].IsCollision)
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

				if (hasCollision != mChunkSettings.TileData[GetTileId(x - 1, y, mChunkSettings.ClampedPlayableLayer)].IsCollision)
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

				if (hasCollision != mChunkSettings.TileData[GetTileId(x, y + 1, mChunkSettings.ClampedPlayableLayer)].IsCollision)
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

				if (hasCollision != mChunkSettings.TileData[GetTileId(x, y - 1, mChunkSettings.ClampedPlayableLayer)].IsCollision)
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
	
	private void Build()
	{
		mMeshGenerationData.Build();
		mColliderGenerationData.Build();

		mMeshFilter.mesh = mMeshGenerationData.Mesh;
		
		Material[] materials = new Material[TotalMaterials()];
		int index = 0;
		foreach (TileData tileData in mChunkSettings.TileData)
		{
			foreach (Material material in tileData.Depth1Materials)
			{
				materials[index] = material;
				++index;
			}
			foreach (Material material in tileData.Depth2Materials)
			{
				materials[index] = material;
				++index;
			}
			foreach (Material material in tileData.Depth3Materials)
			{
				materials[index] = material;
				++index;
			}
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
					int tileId = GetTileId(x, y, z);
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
							OnTileDestroyed(tilePosition, tileId, mTileDepth[index], mTileVariant[index], explosionInstance);
							SetTile(x, y, z, 0);
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
			CalculateTileDepth();
			GenerateMeshData();
			GenerateColliderData();
			Build();
		}
	}

	private int GetTileIndex(int x, int y, int z)
	{
		return x + y * mChunkSettings.NumberOfColumns + z * mChunkSettings.NumberOfColumns * mChunkSettings.NumberOfRows;
	}

	private int TotalMaterials()
	{
		int result = 0;
		for (int i = 0; i < mChunkSettings.TileData.Length; ++i)
		{
			result += TotalMaterialsInTile(i);
		}
		return result;
	}
	
	private int TotalMaterialsInTile(int tileId)
	{
		int result = 0;
		result += mChunkSettings.TileData[tileId].Depth1Materials.Length;
		result += mChunkSettings.TileData[tileId].Depth2Materials.Length;
		result += mChunkSettings.TileData[tileId].Depth3Materials.Length;
		return result;
	}

	private int GetTileId(int x, int y, int z)
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
		mTileDepth[index] = -1;
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

	//private void OnGUI()
	//{
	//	Camera cam = GameController.Instance.CameraControllerInstance.CameraComponent;
	//	for (int y = 0; y < mChunkSettings.NumberOfRows; ++y)
	//	{
	//		for (int x = 0; x < mChunkSettings.NumberOfColumns; ++x)
	//		{
	//			int z = 0;
	//			Vector3 tilePosition = GetTileWorldPosition(x, y, z);
	//			Vector3 screenPosition = cam.WorldToScreenPoint(tilePosition);
	//			const float SIZE = 64.0f;
	//			const float HALF_SIZE = SIZE / 2.0f;
	//			Rect rect = new Rect();
	//			rect.x = screenPosition.x - HALF_SIZE;
	//			rect.y = Screen.height - screenPosition.y - HALF_SIZE;
	//			rect.width = SIZE;
	//			rect.height = SIZE;
	//			int index = GetTileIndex(x, y, z);
	//			GUI.Label(rect, mTileDepth[index].ToString() + "\n" + mTileVariant[index].ToString() + "\n" + GetSubmeshIndex(mTiles[index], mTileDepth[index], mTileVariant[index]).ToString());
	//		}
	//	}
	//}
}
