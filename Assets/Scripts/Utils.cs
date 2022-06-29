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
        frequency = amplitude = 1.0f;

        for (int i = 0; i < octave; i++)
        {
            total += Mathf.PerlinNoise((x + offsetX) * frequency, (y + offsetY) * frequency) * amplitude;
            maxValue += amplitude;
            amplitude *= persistance;
            frequency *= 2;
        }
        if (x == 0.0f && y == 0.0f)
        {
        Debug.Log(total / maxValue);
        }
        return total / maxValue;
    }

    public static float FractalBrownianMotion(float x, float y, PerlinParameters p)
    {
        return FractalBrownianMotion(x*p.xScale, y*p.yScale, p.octaves, p.persistance, p.xOffset, p.yOffset);
    }
}
