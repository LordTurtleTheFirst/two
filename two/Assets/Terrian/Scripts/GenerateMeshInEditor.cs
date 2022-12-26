using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
[CustomEditor (typeof (TerrianGenerator))]
public class GenerateMeshInEditor : Editor
{
    public override void OnInspectorGUI()
    {
        TerrianGenerator gen = (TerrianGenerator)target;

        DrawDefaultInspector();
        if (GUILayout.Button("createChunks"))
        {
            gen.createChunks();
        }
        if (GUILayout.Button("generateMesh"))
        {
            gen.updateMesh();
        }
        if (GUILayout.Button("generateMaterial"))
        {
            gen.updateMaterial();
        }
        if (GUILayout.Button("destroyChunks"))
        {
            gen.removeChunks();
        }
    }
}
