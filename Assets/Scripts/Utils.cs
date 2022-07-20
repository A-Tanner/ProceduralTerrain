using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static float FractalBrownianMotion
        (float x, float y, int octave, float persistance, int offsetX, int offsetY)
    {
        float total, maxValue;
        total = maxValue = 0;

        float frequency, amplitude;
        frequency = amplitude = 1;

        for (int i = 0; i < octave; i++)
        {
            total += Mathf.PerlinNoise((x + offsetX) * frequency, (y + offsetY) * frequency) * amplitude;
            maxValue += amplitude;
            amplitude *= persistance;
            frequency *= 2;
        }

        return total / maxValue;
    }

    public static float AverageFloats(params float[] floats)
    {
        float sum = 0.0f;
        for (int i = 0; i < floats.Length; i++)
        {
            sum += floats[i];
        }

        return sum / floats.Length;
    }

    public static float AverageFloats(IEnumerable<float> floats)
    {
        float sum = 0.0f;
        int count = 0;

        foreach (float f in floats)
        {
            sum += f;
            count++;
        }

        return sum / count;

    }

    public static IEnumerable<Vector2> GetNeighbors(Vector2 position, int width, int height)
    {
        List<Vector2> neighbors = new List<Vector2>();
        for (int x = -1; x < 2; x++)
        {
            for (int y = -1; y < 2; y++)
            {
                if (!(x == 0 && y == 0)) // Does not include self as a neighbor
                {
                    Vector2 validNeighbor = new Vector2(Mathf.Clamp(position.x + x, 0, width - 1),
                        Mathf.Clamp(position.y + y, 0, height - 1));

                    if (!neighbors.Contains(validNeighbor))
                        neighbors.Add(validNeighbor);
                }
            }
        }

        return neighbors;
    }

    public static IEnumerable<float> GetValuesAtCoordinates(float[,] matrix, IEnumerable<Vector2> coordinates)
    {
        List<float> values = new();

        foreach(Vector2 location in coordinates)
        {
            values.Add(matrix[Mathf.RoundToInt(location.x), Mathf.RoundToInt(location.y)]);
        }

        return values;
    }

}
