using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrianChunk : MonoBehaviour
{
    public Vector3Int chunkPosition;
    public int chunkSize;
    public Mesh mesh;
    public float[,,] map;
    public TerrianGenerator terrianGenerator;
    MeshCollider meshCollider;
    public PhysicMaterial physicsMaterial;
    public void generateMesh()
    {
        mesh = terrianGenerator.generateMesh(map);
        MeshFilter meshfilter = GetComponent<MeshFilter>();
        meshfilter.sharedMesh = mesh;
        meshCollider = gameObject.GetComponent<MeshCollider>();
        meshCollider.sharedMaterial = physicsMaterial;
        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = mesh;
    }
    public void generateMaterial()
    {
        Renderer rend = GetComponent<Renderer>();
        Material mat = new Material(terrianGenerator.chunkMat);
        Texture2D tex = terrianGenerator.TextureFromHeightMap(chunkPosition, map);
        mat.mainTexture = tex;
        rend.sharedMaterial = mat;
    }
    public void generateMap()
    {
        map = Noise.get2dMap(chunkSize, chunkPosition, terrianGenerator.frequency, terrianGenerator.amplitude, terrianGenerator.persistance, terrianGenerator.octave, terrianGenerator.seed, terrianGenerator.yStep);
    }
}
