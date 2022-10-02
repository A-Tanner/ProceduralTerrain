using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ProceduralTerrain;
using System.IO;

public class TextureCreatorWindow : EditorWindow
{
    string filename = "proceduraltexture";
    string saveDirectory = "/GeneratedTextures";
    float perlinXScale = 0.001f;
    float perlinYScale = 0.001f;
    int perlinXOffset = 0;
    int perlinYOffset = 0;
    int perlinOctaves = 3;
    float perlinPersistance = 8.0f;
    float perlinHeightScale = 0.09f;
    float minRemap = 0;
    float maxRemap = 1;
    bool alpha = false;
    bool seamless = false;
    bool remap = false;

    int width = 513;
    int height = 513;


    Texture2D previewTexture;


    [MenuItem("Window/Texture Creator")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(TextureCreatorWindow));
    }

    private void OnEnable()
    {
        previewTexture = new Texture2D(width, height, TextureFormat.ARGB32, false);
    }

    private void OnGUI()
    {
        GUILayout.Label("Settings", EditorStyles.boldLabel);

        int wSize = (int)(EditorGUIUtility.currentViewWidth - 100);

        filename = EditorGUILayout.TextField("Texture Name", filename);
        perlinXScale = EditorGUILayout.Slider("X Scale", perlinXScale, 0, 0.1f);
        perlinYScale = EditorGUILayout.Slider("Y Scale", perlinYScale, 0, 0.1f);
        perlinXOffset = EditorGUILayout.IntSlider("X Offset", perlinXOffset, 0, 4096);
        perlinYOffset = EditorGUILayout.IntSlider("Y Offset", perlinYOffset, 0, 4096);
        perlinOctaves = EditorGUILayout.IntSlider("Octaves", perlinOctaves, 1, 10);
        perlinPersistance = EditorGUILayout.Slider("Persistance", perlinPersistance,0,2);
        perlinHeightScale = EditorGUILayout.Slider("Height Scale", perlinHeightScale,0,1);
        alpha = EditorGUILayout.Toggle("Has Alpha", alpha);
        seamless = EditorGUILayout.Toggle("Is Seamless", seamless);
        remap = EditorGUILayout.Toggle("Remap range", remap);
        if (remap)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(minRemap.ToString());
            EditorGUILayout.MinMaxSlider(ref minRemap, ref maxRemap,0,1);
            GUILayout.Label(maxRemap.ToString());
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        float minValue, maxValue;

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Generate", GUILayout.Width(wSize))){
            float pixelValue;
            Color pixelColor = Color.red;
            minValue = 2;
            maxValue = -1;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (seamless)
                    {
                        float u = (float)x / (float)width;
                        float v = (float)y / (float)height;

                        float noise00 = Utils.FractalBrownianMotion(x * perlinXScale,
                        y * perlinYScale,
                        perlinOctaves,
                        perlinPersistance,
                        perlinXOffset,
                        perlinYOffset) * perlinHeightScale;

                        float noise01 = Utils.FractalBrownianMotion((x + width) * perlinXScale,
                        y * perlinYScale,
                        perlinOctaves,
                        perlinPersistance,
                        perlinXOffset,
                        perlinYOffset) * perlinHeightScale;

                        float noise10 = Utils.FractalBrownianMotion(x * perlinXScale,
                        (y + height) * perlinYScale,
                        perlinOctaves,
                        perlinPersistance,
                        perlinXOffset,
                        perlinYOffset) * perlinHeightScale;

                        float noise11 = Utils.FractalBrownianMotion((x + width) * perlinXScale,
                        (y + height) * perlinYScale,
                        perlinOctaves,
                        perlinPersistance,
                        perlinXOffset,
                        perlinYOffset) * perlinHeightScale;

                        float noiseTotal = u * v * noise00 +
                            (1-u) * v * noise01 +
                            u * (1-v) * noise10 +
                            (1-u) * (1-v) * noise11;

                        //float mappedVal = (int)(256 * noiseTotal);
                        //float r = Mathf.Clamp((int)noise00, 0, 256);
                        //float g = Mathf.Clamp(mappedVal, 0, 255);

                        //pixelValue = (r+g+g)/(3*255.0f);
                        pixelValue = noiseTotal;

                    }
                    else
                    {
                        pixelValue = Utils.FractalBrownianMotion(x * perlinXScale,
                        y * perlinYScale,
                        perlinOctaves,
                        perlinPersistance,
                        perlinXOffset,
                        perlinYOffset) * perlinHeightScale;
                    }
                    if(minValue > pixelValue) minValue = pixelValue;
                    if(maxValue < pixelValue) maxValue = pixelValue;

                    pixelColor = new Color(pixelValue, pixelValue, pixelValue, alpha ? pixelValue : 1);
                    previewTexture.SetPixel(x, y, pixelColor);
                }
            }


            if (remap)
            {
                for(int y = 0; y < height; y++)
                {
                    for(int x =0; x < width; x++)
                    {
                        float grayscaleValue = previewTexture.GetPixel(x, y).r;
                        grayscaleValue = Utils.Remap(grayscaleValue, minValue, maxValue, minRemap, maxRemap);
                        pixelColor.r=pixelColor.g=pixelColor.b=grayscaleValue;
                        previewTexture.SetPixel(x, y, pixelColor);
                    }
                }
            }

            previewTexture.Apply(false, false);
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label(previewTexture, GUILayout.Width(wSize), GUILayout.Height(wSize));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Save", GUILayout.Width(wSize))){
            byte[] bytes = previewTexture.EncodeToPNG();
            System.IO.Directory.CreateDirectory(Application.dataPath + saveDirectory);
            File.WriteAllBytes(Application.dataPath + saveDirectory + "/"+ filename + ".png", bytes);
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

}
