using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class Noise
{
    public static float[,,] getMap(Vector3Int mapSize,float frequency, float amplitude, float persistence, int octave, int seed)
    {
        float[,,] map = new float[mapSize.x, mapSize.y, mapSize.z];
        for(int x = 0; x < mapSize.x; x++)
        {
            for(int y = 0; y < mapSize.y; y++)
            {
                for( int z = 0; z < mapSize.z; z++)
                {
                    map[x, y, z] = Noise3D(x/20f,y/20f,z/20f,frequency,amplitude,persistence,octave,seed);
                }
            }
        }
        return map;
    }
    public static float[,,] get2dMap(int mapSize, Vector3Int chunkPosition, float frequency, float amplitude, float persistence, int octave, int seed, float yStep)
    {
        float[,,] map = new float[mapSize, mapSize, mapSize];
        for(int x = 0; x < mapSize; x++)
        {
            for(int z = 0; z < mapSize; z++)
            {
                float noise = 0;
                float _frequency = frequency;
                float _amplitude = amplitude;
                for(int i = 0; i < octave; i++) {
                    noise = (Mathf.PerlinNoise((x + chunkPosition.x * (mapSize - 1)) / 20f * _frequency + seed, (z + chunkPosition.z * (mapSize - 1)) / 20f * _frequency + seed) * _amplitude) + yStep;
                    _amplitude *= persistence;
                    _frequency *= 0.5f;
                }
                for(int y = 0; y < mapSize; y++)
                {
                    if(yStep * y > noise)
                    {
                        map[x, y, z] = 0;
                    }
                    else
                    {
                        map[x, y, z] = 1;
                    }
                }
            }
        }
        return map;
    }
    public static float[,] getHeightMap(float[,,] cubeMap, int chunkSize)
    {
        float[,] map = new float[chunkSize, chunkSize];
        for(int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    if (cubeMap[x,y,z] == 1)
                    {
                        map[x, z] = y;
                    }
                }
            }
        }
        return map;
    }
    public static float Noise3D(float x, float y, float z, float frequency, float amplitude, float persistence, int octave, int seed)
    {
        float noise = 0.0f;

        for (int i = 0; i < octave; ++i)
        {
            // Get all permutations of noise for each individual axis
            float noiseXY = Mathf.PerlinNoise(x * frequency + seed, y * frequency + seed) * amplitude;
            float noiseXZ = Mathf.PerlinNoise(x * frequency + seed, z * frequency + seed) * amplitude;
            float noiseYZ = Mathf.PerlinNoise(y * frequency + seed, z * frequency + seed) * amplitude;

            // Reverse of the permutations of noise for each individual axis
            float noiseYX = Mathf.PerlinNoise(y * frequency + seed, x * frequency + seed) * amplitude;
            float noiseZX = Mathf.PerlinNoise(z * frequency + seed, x * frequency + seed) * amplitude;
            float noiseZY = Mathf.PerlinNoise(z * frequency + seed, y * frequency + seed) * amplitude;

            // Use the average of the noise functions
            noise += (noiseXY + noiseXZ + noiseYZ + noiseYX + noiseZX + noiseZY) / 6.0f;

            amplitude *= persistence;
            frequency *= 2.0f;
        }

        // Use the average of all octaves
        return noise / octave;
    }
}
