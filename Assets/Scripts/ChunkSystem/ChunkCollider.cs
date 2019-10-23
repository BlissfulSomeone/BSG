using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
public class ChunkCollider : MonoBehaviour
{
	private MeshCollider mMeshCollider;
	private Chunk mChunk;
	private Mesh mMesh;

	private List<Vector3> mVertices;
	private List<int> mTriangles;
	private int mTriangleIndex;
	
	public void AttachToChunk(Chunk aChunk)
	{
		if (mMeshCollider == null)
			mMeshCollider = GetComponent<MeshCollider>();

		mChunk = aChunk;
		mChunk.OnTileDestroyed += OnTileDestroyed;

		mVertices = new List<Vector3>();
		mTriangles = new List<int>();

		mMesh = new Mesh();

		GenerateMesh();
	}

	private void GenerateMesh()
	{
		mVertices.Clear();
		mTriangles.Clear();
		mTriangleIndex = 0;
		
		for (int y = 0; y < mChunk.GridSize.y; ++y)
		{
			for (int x = 0; x < mChunk.GridSize.x; ++x)
			{
				if (mChunk.GetTile(x, y).TileData.hasCollision == true)
				{
					Vector3 tilePosition = new Vector3(mChunk.Position.x + x * mChunk.TileSize.x, mChunk.Position.y + y * mChunk.TileSize.y, 0.0f);
					Vector3 tileHalfSize = mChunk.TileSize * 0.5f;

					if (mChunk.IsCollision(x + 1, y) == false)
					{
						mVertices.Add(new Vector3(tilePosition.x + tileHalfSize.x, tilePosition.y + tileHalfSize.y, -0.5f));
						mVertices.Add(new Vector3(tilePosition.x + tileHalfSize.x, tilePosition.y - tileHalfSize.y, -0.5f));
						mVertices.Add(new Vector3(tilePosition.x + tileHalfSize.x, tilePosition.y - tileHalfSize.y, 0.5f));
						mVertices.Add(new Vector3(tilePosition.x + tileHalfSize.x, tilePosition.y + tileHalfSize.y, 0.5f));

						mTriangles.Add(mTriangleIndex + 0);
						mTriangles.Add(mTriangleIndex + 2);
						mTriangles.Add(mTriangleIndex + 1);
						mTriangles.Add(mTriangleIndex + 0);
						mTriangles.Add(mTriangleIndex + 3);
						mTriangles.Add(mTriangleIndex + 2);

						mTriangleIndex += 4;
					}
					if (mChunk.IsCollision(x - 1, y) == false)
					{
						mVertices.Add(new Vector3(tilePosition.x - tileHalfSize.x, tilePosition.y + tileHalfSize.y, -0.5f));
						mVertices.Add(new Vector3(tilePosition.x - tileHalfSize.x, tilePosition.y - tileHalfSize.y, -0.5f));
						mVertices.Add(new Vector3(tilePosition.x - tileHalfSize.x, tilePosition.y - tileHalfSize.y, 0.5f));
						mVertices.Add(new Vector3(tilePosition.x - tileHalfSize.x, tilePosition.y + tileHalfSize.y, 0.5f));

						mTriangles.Add(mTriangleIndex + 0);
						mTriangles.Add(mTriangleIndex + 1);
						mTriangles.Add(mTriangleIndex + 2);
						mTriangles.Add(mTriangleIndex + 0);
						mTriangles.Add(mTriangleIndex + 2);
						mTriangles.Add(mTriangleIndex + 3);

						mTriangleIndex += 4;
					}
					if (mChunk.IsCollision(x, y - 1) == false)
					{
						mVertices.Add(new Vector3(tilePosition.x - tileHalfSize.x, tilePosition.y - tileHalfSize.y, -0.5f));
						mVertices.Add(new Vector3(tilePosition.x + tileHalfSize.x, tilePosition.y - tileHalfSize.y, -0.5f));
						mVertices.Add(new Vector3(tilePosition.x + tileHalfSize.x, tilePosition.y - tileHalfSize.y, 0.5f));
						mVertices.Add(new Vector3(tilePosition.x - tileHalfSize.x, tilePosition.y - tileHalfSize.y, 0.5f));

						mTriangles.Add(mTriangleIndex + 0);
						mTriangles.Add(mTriangleIndex + 1);
						mTriangles.Add(mTriangleIndex + 2);
						mTriangles.Add(mTriangleIndex + 0);
						mTriangles.Add(mTriangleIndex + 2);
						mTriangles.Add(mTriangleIndex + 3);

						mTriangleIndex += 4;
					}
					if (mChunk.IsCollision(x, y + 1) == false)
					{
						mVertices.Add(new Vector3(tilePosition.x - tileHalfSize.x, tilePosition.y + tileHalfSize.y, -0.5f));
						mVertices.Add(new Vector3(tilePosition.x + tileHalfSize.x, tilePosition.y + tileHalfSize.y, -0.5f));
						mVertices.Add(new Vector3(tilePosition.x + tileHalfSize.x, tilePosition.y + tileHalfSize.y, 0.5f));
						mVertices.Add(new Vector3(tilePosition.x - tileHalfSize.x, tilePosition.y + tileHalfSize.y, 0.5f));

						mTriangles.Add(mTriangleIndex + 0);
						mTriangles.Add(mTriangleIndex + 2);
						mTriangles.Add(mTriangleIndex + 1);
						mTriangles.Add(mTriangleIndex + 0);
						mTriangles.Add(mTriangleIndex + 3);
						mTriangles.Add(mTriangleIndex + 2);

						mTriangleIndex += 4;
					}
					//mVertices.Add(new Vector3(tilePosition.x - tileHalfSize.x, tilePosition.y - tileHalfSize.y, 0.0f));
					//mVertices.Add(new Vector3(tilePosition.x + tileHalfSize.x, tilePosition.y - tileHalfSize.y, 0.0f));
					//mVertices.Add(new Vector3(tilePosition.x + tileHalfSize.x, tilePosition.y + tileHalfSize.y, 0.0f));
					//mVertices.Add(new Vector3(tilePosition.x - tileHalfSize.x, tilePosition.y + tileHalfSize.y, 0.0f));
					//
					//mTriangles.Add(mTriangleIndex + 0);
					//mTriangles.Add(mTriangleIndex + 1);
					//mTriangles.Add(mTriangleIndex + 2);
					//mTriangles.Add(mTriangleIndex + 0);
					//mTriangles.Add(mTriangleIndex + 2);
					//mTriangles.Add(mTriangleIndex + 3);
					//
					//mTriangleIndex += 4;
				}
			}
		}

		mMesh.Clear();
		mMesh.vertices = mVertices.ToArray();
		mMesh.triangles = mTriangles.ToArray();

		mMeshCollider.sharedMesh = mMesh;
		mMeshCollider.convex = false;
	}

	private void OnTileDestroyed(Chunk.TileInstance aTileInstance, Vector3 aTilePosition)
	{
		GenerateMesh();
	}
}
