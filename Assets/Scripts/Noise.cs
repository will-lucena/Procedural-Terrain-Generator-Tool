using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class Noise
{
	public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, Vector2 offset, List<Octave> octaves = null)
    {
		float[,] noiseMap = new float[mapWidth,mapHeight];

		System.Random prng = new System.Random (seed);
		if (scale <= 0)
        {
			scale = 0.0001f;
		}

		float maxNoiseHeight = float.MinValue;
		float minNoiseHeight = float.MaxValue;

		float halfWidth = mapWidth / 2f;
		float halfHeight = mapHeight / 2f;
        Vector2 currentOffset = new Vector2(prng.Next(-100000, 100000) + offset.x, prng.Next(-100000, 100000) + offset.y);

        if (octaves == null)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    float sampleX = x / scale + offset.x;
                    float sampleY = y / scale + offset.y;
                    float elevation = Mathf.PerlinNoise(sampleX, sampleY);

                    if (elevation > maxNoiseHeight)
                    {
                        maxNoiseHeight = elevation;
                    }
                    else if (elevation < minNoiseHeight)
                    {
                        minNoiseHeight = elevation;
                    }
                    noiseMap[x, y] = elevation;
                }
            }
        }
        else
        {
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    float elevation = calculateElevation(x, y, currentOffset, scale, octaves);

                    if (elevation > maxNoiseHeight)
                    {
                        maxNoiseHeight = elevation;
                    }
                    else if (elevation < minNoiseHeight)
                    {
                        minNoiseHeight = elevation;
                    }
                    noiseMap[x, y] = elevation;
                }
            }
        }

		

		for (int y = 0; y < mapHeight; y++) {
			for (int x = 0; x < mapWidth; x++) {
				noiseMap [x, y] = Mathf.InverseLerp (minNoiseHeight, maxNoiseHeight, noiseMap [x, y]);
			}
		}

		return noiseMap;
	}

    public static float calculateElevation(float x, float y, Vector2 offset, float scale, List<Octave> octaves)
    {
        float elevation = 0;
        foreach (Octave octave in octaves)
        {
            float sampleX = x / scale * octave.frequency + offset.x;
            float sampleY = y / scale * octave.frequency + offset.y;
            elevation += Mathf.PerlinNoise(sampleX, sampleY) * octave.amplitude;
        }
        return elevation;
    }
}
