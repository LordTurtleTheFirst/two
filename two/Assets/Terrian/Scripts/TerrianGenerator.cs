using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine;

public class TerrianGenerator : MonoBehaviour
{
    [HideInInspector] public float height = 0.2f;

    public float frequency;
    public float amplitude;
    public float persistance;
    public int octave;
    public int seed;
    [HideInInspector] public float yStep = 0.05f;
    public GameObject chunkPrefab;
    public int chunkSize;
    public Vector2Int chunks;
    public Reigons[] reigons;
    public Material chunkMat;
    static TerrianChunk[,] chunkList;
    public PhysicMaterial physicMat;
    public GameObject chunkRoot;
    public void removeChunks()
    {
        foreach (TerrianChunk chunk in chunkRoot.GetComponentsInChildren<TerrianChunk>())
        {
            DestroyImmediate(chunk.transform.gameObject);
        }
    }
    public void createChunks()
    {
        removeChunks();
        chunkList = new TerrianChunk[chunks.x, chunks.y];
        for(int x = 0; x < chunks.x; x++)
        {
            for(int y = 0; y < chunks.y; y++)
            {
                if (chunkList[x, y])
                {
                    Destroy(chunkList[x, y]);
                }
                GameObject chunk = Instantiate(chunkPrefab, chunkRoot.transform);
                TerrianChunk currentChunk = chunk.GetComponent<TerrianChunk>();
                currentChunk.chunkPosition = new Vector3Int(x, 0, y);
                currentChunk.chunkSize = chunkSize;
                currentChunk.transform.position = (chunkSize - 1) * currentChunk.chunkPosition;
                currentChunk.terrianGenerator = this;
                currentChunk.physicsMaterial = physicMat;
                chunkList[x, y] = currentChunk;
            }
        }
    }
    public void updateMesh()
    {
        for(int x = 0; x < chunks.x; x++)
        {
            for(int y = 0; y < chunks.y; y++)
            {
                Debug.Log(chunkList[x, y]);
                if(chunkList[x, y])
                {
                    chunkList[x, y].generateMap();
                    chunkList[x, y].generateMesh();
                }
            }
        }
    }
    public void updateMaterial()
    {
        for (int x = 0; x < chunks.x; x++)
        {
            for (int y = 0; y < chunks.y; y++)
            {
                Debug.Log(chunkList[x, y]);
                if (chunkList[x, y])
                {
                    chunkList[x, y].generateMaterial();
                }
            }
        }
    }
    public Mesh generateMesh(float[,,] map)
    {
        List<Triangle> triangles = new List<Triangle>();
        for(int x = 0; x < chunkSize; x++)
        {
            for(int y = 0; y < chunkSize; y++)
            {
                for(int z = 0; z < chunkSize; z++)
                {
                    March(new Vector3Int(x, y, z));
                }
            }
        }
        List<int> triangleList = new List<int>();
        List<Vector3> pointList = new List<Vector3>();
        foreach(Triangle triangle in triangles)
        {
            if (!pointList.Contains(triangle.pointA))
            {
                pointList.Add(triangle.pointA);
            }
            if (!pointList.Contains(triangle.pointB))
            {
                pointList.Add(triangle.pointB);
            }
            if (!pointList.Contains(triangle.pointC))
            {
                pointList.Add(triangle.pointC);
            }
            triangleList.Add(pointList.IndexOf(triangle.pointA));
            triangleList.Add(pointList.IndexOf(triangle.pointB));
            triangleList.Add(pointList.IndexOf(triangle.pointC));
        }
        Vector2[] uvs = new Vector2[pointList.Count];
        for (int i = 0; i < pointList.Count; i++)
        {
            uvs[i] = new Vector2(pointList[i].x / (float)chunkSize, pointList[i].z / (float)chunkSize);
        }
        Mesh mesh = new Mesh();
        mesh.vertices = pointList.ToArray();
        mesh.triangles = triangleList.ToArray();
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        return mesh;
        void March(Vector3Int cubePosition)
        {
            if (cubePosition.x == chunkSize - 1 || cubePosition.y == chunkSize - 1 || cubePosition.z == chunkSize - 1)
            {
                return;
            }
            Vector3[] corners = {
            new Vector3(cubePosition.x,     cubePosition.y,     cubePosition.z),
            new Vector3(cubePosition.x + 1, cubePosition.y,     cubePosition.z),
            new Vector3(cubePosition.x + 1, cubePosition.y,     cubePosition.z + 1),
            new Vector3(cubePosition.x,     cubePosition.y,     cubePosition.z + 1),
            new Vector3(cubePosition.x,     cubePosition.y + 1, cubePosition.z),
            new Vector3(cubePosition.x + 1, cubePosition.y + 1, cubePosition.z),
            new Vector3(cubePosition.x + 1, cubePosition.y + 1, cubePosition.z + 1 ),
            new Vector3(cubePosition.x,     cubePosition.y + 1, cubePosition.z + 1)
        };
            int cubeIndex = 0;
            if (getValue(corners[0]) > height) cubeIndex |= 1;
            if (getValue(corners[1]) > height) cubeIndex |= 2;
            if (getValue(corners[2]) > height) cubeIndex |= 4;
            if (getValue(corners[3]) > height) cubeIndex |= 8;
            if (getValue(corners[4]) > height) cubeIndex |= 16;
            if (getValue(corners[5]) > height) cubeIndex |= 32;
            if (getValue(corners[6]) > height) cubeIndex |= 64;
            if (getValue(corners[7]) > height) cubeIndex |= 128;
            for (int i = 0; MarchTables.triangulation[cubeIndex, i] != -1; i += 3)
            {
                // Get indices of corner points A and B for each of the three edges
                // of the cube that need to be joined to form the triangle.
                int a0 = MarchTables.cornerIndexAFromEdge[MarchTables.triangulation[cubeIndex, i]];
                int b0 = MarchTables.cornerIndexBFromEdge[MarchTables.triangulation[cubeIndex, i]];

                int a1 = MarchTables.cornerIndexAFromEdge[MarchTables.triangulation[cubeIndex, i + 1]];
                int b1 = MarchTables.cornerIndexBFromEdge[MarchTables.triangulation[cubeIndex, i + 1]];

                int a2 = MarchTables.cornerIndexAFromEdge[MarchTables.triangulation[cubeIndex, i + 2]];
                int b2 = MarchTables.cornerIndexBFromEdge[MarchTables.triangulation[cubeIndex, i + 2]];
                List<Vector3> pointList = new List<Vector3>();
                Triangle tri = new Triangle();
                tri.pointA = corners[a0] + ((corners[b0] - corners[a0]) / 2);
                tri.pointB = corners[a1] + ((corners[b1] - corners[a1]) / 2);
                tri.pointC = corners[a2] + ((corners[b2] - corners[a2]) / 2);
                triangles.Add(tri);
            }
        }
        float getValue(Vector3 position)
        {
            return map[(int)position.x, (int)position.y, (int)position.z];
        }
    }
    public Texture2D TextureFromHeightMap(Vector3Int chunkPosition, float[,,] cubeMap)
    {
        float[,] heightMap = Noise.getHeightMap(cubeMap, chunkSize);
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        Texture2D texture = new Texture2D(width, height);

        Color[] colorMap = new Color[width * height];
        for(int y = 0; y < width; y++)
        {
            for(int x = 0; x < height; x++)
            {
                for(int i = 0; i < reigons.Length; i++)
                {
                    if (heightMap[x,y] <= reigons[i].height)
                    {
                        colorMap[y * width + x] = reigons[i].color;
                    }
                }
            }
        }
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colorMap);
        texture.Apply();
        return (texture);
    }
}
struct Triangle
{
    public Vector3 pointA;
    public Vector3 pointB;
    public Vector3 pointC;
}
[Serializable]
public struct Reigons
{
    public float height;
    public Color color;
}