using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class TextureCreatorWindow : EditorWindow
{
    string filename = "proceduraltexture";
    float perlinXScale = 0.001f;
    float perlinYScale = 0.001f;
    int perlinXOffset = 0;
    int perlinYOffset = 0;
    int perlinOctaves = 3;
    float perlinPersistance = 8.0f;
    float perlinHeightScale = 0.09f;
    bool alpha = false;
    bool seamless = false;
    bool enhancedView = false;

    [MenuItem("Window/Texture Creator")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(TextureCreatorWindow));
    }

    private void OnGUI()
    {
        GUILayout.Label("Settings", EditorStyles.boldLabel);
        filename = EditorGUILayout.TextField("Texture Name", filename);
        perlinXScale = EditorGUILayout.Slider("X Scale", perlinXScale, 0, 0.01f);
        perlinYScale = EditorGUILayout.Slider("Y Scale", perlinXScale, 0, 0.01f);
        perlinXOffset = EditorGUILayout.IntSlider("X Offset", perlinXOffset, 0, 4096);
        perlinYOffset = EditorGUILayout.IntSlider("Y Offset", perlinYOffset, 0, 4096);
        perlinOctaves = EditorGUILayout.IntSlider("Octaves", perlinOctaves, 1, 10);
        perlinPersistance = EditorGUILayout.Slider("Persistance", perlinPersistance,0,2);
        perlinHeightScale = EditorGUILayout.Slider("Height Scale", perlinHeightScale,0,1);
        alpha = EditorGUILayout.Toggle("Has Alpha", alpha);
        seamless = EditorGUILayout.Toggle("Is Seamless", seamless);
        enhancedView = EditorGUILayout.Toggle("Enhanced View", enhancedView);
    }

}
