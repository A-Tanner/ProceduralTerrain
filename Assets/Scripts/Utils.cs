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
}