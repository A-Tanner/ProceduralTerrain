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
        foreach(float f in floats)
        {
            sum += f;
            count++;
        }

        return sum / count;

    }

}
