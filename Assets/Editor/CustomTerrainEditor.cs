using EditorGUITable;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CustomTerrain))] //links this script with the editor of CustomTerrain
[CanEditMultipleObjects]
public class CustomTerrainEditor : Editor
{
    //Properties that get pulled off of the script being edited
    SerializedProperty additive;
    //Random
    SerializedProperty randomHeightRange;
    //From Image
    SerializedProperty heightMapImage;
    SerializedProperty heightMapScale;
    //Perlin Noise
    SerializedProperty perlinXScale;
    SerializedProperty perlinYScale;
    SerializedProperty perlinXOffset;
    SerializedProperty perlinYOffset;
    SerializedProperty perlinOctaves;
    SerializedProperty perlinPersistance;
    SerializedProperty perlinHeightScale;
    //Voronoi
    SerializedProperty voronoiFalloff;
    SerializedProperty voronoiDropoff;
    SerializedProperty voronoiMinHeight;
    SerializedProperty voronoiMaxHeight;
    SerializedProperty voronoiNumberPeaks;

    //Sine
    SerializedProperty sineFrequency;
    SerializedProperty sineFalloff;
    SerializedProperty sineStrength;
    SerializedProperty sineAllowNegative;

    //Midpoint Displacement
    SerializedProperty mpdRoughness;
    SerializedProperty mpdDisplacementFactor;
    SerializedProperty mpdUpperBoundsRatio;
    SerializedProperty mpdLowerBoundsRatio;

    //Texture
    SerializedProperty terrainTextures;

    //Tables
    GUITableState textureTable;

    //Fold outs
    bool showRandom = false;
    bool showImage = false;
    bool showPerlin = false;
    bool showVoronoi = false;
    bool showSine = false;
    bool showMpd = false;
    bool showTextures = false;

    //Editor fields
    //Terrain from image
    bool heightMapAutoScale = false;
    float heightMapXTiles = 0;
    float heightMapZTiles = 0;
    float strength = 0.1f;

    float tempVoronoiMin = 0.0f;
    float tempVoronoiMax = 1.0f;

    void OnEnable() //Essentially Awake. Allows processing without rerun
    {
        randomHeightRange = serializedObject.FindProperty("randomHeightRange");
        additive = serializedObject.FindProperty("additive");

        heightMapImage = serializedObject.FindProperty("heightMapImage");
        heightMapScale = serializedObject.FindProperty("heightMapScale");

        perlinXScale = serializedObject.FindProperty("perlinXScale");
        perlinYScale = serializedObject.FindProperty("perlinYScale");
        perlinXOffset = serializedObject.FindProperty("perlinXOffset");
        perlinYOffset = serializedObject.FindProperty("perlinYOffset");
        perlinOctaves = serializedObject.FindProperty("perlinOctaves");
        perlinPersistance = serializedObject.FindProperty("perlinPersistance");
        perlinHeightScale = serializedObject.FindProperty("perlinHeightScale");

        voronoiFalloff = serializedObject.FindProperty("voronoiFalloff");
        voronoiDropoff = serializedObject.FindProperty("voronoiDropoff");
        voronoiMinHeight = serializedObject.FindProperty("voronoiMinHeight");
        voronoiMaxHeight = serializedObject.FindProperty("voronoiMaxHeight");
        voronoiNumberPeaks = serializedObject.FindProperty("voronoiNumberPeaks");

        sineFrequency = serializedObject.FindProperty("sineFrequency");
        sineFalloff = serializedObject.FindProperty("sineFalloff");
        sineStrength = serializedObject.FindProperty("sineStrength");
        sineAllowNegative = serializedObject.FindProperty("sineAllowNegative");

        mpdRoughness = serializedObject.FindProperty("mpdRoughness");
        mpdDisplacementFactor = serializedObject.FindProperty("mpdDisplacementFactor");
        mpdUpperBoundsRatio = serializedObject.FindProperty("mpdUpperBoundsRatio");
        mpdLowerBoundsRatio = serializedObject.FindProperty("mpdLowerBoundsRatio");

        textureTable = new GUITableState("textureTable");
        terrainTextures = serializedObject.FindProperty("terrainTextures");


    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        CustomTerrain terrain = (CustomTerrain)target;
        //Should use serialized values between the editor and target
        //This allows user input to stay live in the inspector
        //Even after script changes

        #region Options
        GUILayout.Label("Generation Options", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(additive);
        #endregion

        #region Generation
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Label("Generation Methods", EditorStyles.boldLabel);
        #region Random
        showRandom = EditorGUILayout.Foldout(showRandom, "Random");
        if (showRandom)
        {
            EditorGUILayout.PropertyField(randomHeightRange);
            //GUILayout.Label("Additive", EditorStyles.boldLabel);



            if (GUILayout.Button("Randomize Heights"))
            {
                terrain.RandomTerrain();
            }
        }
        #endregion
        #region Texture
        showImage = EditorGUILayout.Foldout(showImage, "From Image");
        if (showImage)
        {
            EditorGUILayout.HelpBox("Ensure your texture has option Non-Power of 2 set to None and Read/Write enabled in the import settings!" +
                " These can be accessed by clicking on the texture asset you'd like to use and changing them in the inspector window", MessageType.Info, true);
            EditorGUILayout.PropertyField(heightMapImage);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Auto-scale image");
            heightMapAutoScale = EditorGUILayout.Toggle(heightMapAutoScale);
            EditorGUILayout.EndHorizontal();
            if (heightMapAutoScale)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Horizontal tiles");
                heightMapXTiles = EditorGUILayout.FloatField(heightMapXTiles);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Vertical tiles");
                heightMapZTiles = EditorGUILayout.FloatField(heightMapZTiles);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Strength");
                strength = EditorGUILayout.FloatField(strength);
                EditorGUILayout.EndHorizontal();

            }


            GUILayout.Label("X and Z influence how the image is scaled");
            GUILayout.Label("Y is the strength");

            EditorGUI.BeginDisabledGroup(heightMapAutoScale);
            EditorGUILayout.PropertyField(heightMapScale);
            EditorGUI.EndDisabledGroup();
            //GUILayout.Label("Additive", EditorStyles.boldLabel);



            if (GUILayout.Button("Generate Geometry From Image"))
            {
                if (heightMapAutoScale)
                    terrain.TerrainFromImage(heightMapXTiles, heightMapZTiles, strength);
                else
                    terrain.TerrainFromImage();
            }
        }
        #endregion
        #region Perlin
        showPerlin = EditorGUILayout.Foldout(showPerlin, "Perlin");
        if (showPerlin)
        {
            EditorGUILayout.Slider(perlinXScale, 0, 0.01f, new GUIContent("X Scale"));
            EditorGUILayout.Slider(perlinYScale, 0, 0.01f, new GUIContent("Y Scale"));
            EditorGUILayout.IntSlider(perlinXOffset, 0, 4096, new GUIContent("X Offset"));
            EditorGUILayout.IntSlider(perlinYOffset, 0, 4096, new GUIContent("Y Offset"));
            EditorGUILayout.IntSlider(perlinOctaves, 1, 10, new GUIContent("Octaves"));
            EditorGUILayout.Slider(perlinPersistance, 0, 2, new GUIContent("Persistance"));
            EditorGUILayout.Slider(perlinHeightScale, 0, 1, new GUIContent("Height Scale"));
            if (GUILayout.Button("Apply Noise"))
            {
                terrain.TerrainFromPerlin();
            }

        }
        #endregion
        #region Voronoi
        showVoronoi = EditorGUILayout.Foldout(showVoronoi, "Voronoi");
        if (showVoronoi)
        {
            EditorGUILayout.MinMaxSlider("Height Range", ref tempVoronoiMin, ref tempVoronoiMax, 0.0f, 1.0f);
            voronoiMinHeight.floatValue = tempVoronoiMin;
            voronoiMaxHeight.floatValue = tempVoronoiMax;
            EditorGUILayout.Slider(voronoiFalloff, 0.1f, 10.0f);
            EditorGUILayout.Slider(voronoiDropoff, 0.1f, 4.0f);
            EditorGUILayout.PropertyField(voronoiNumberPeaks);
            if (GUILayout.Button("Generate Voronoi"))
            {
                terrain.TerrainFromVoronoi();
            }
        }
        #endregion
        #region Sine
        showSine = EditorGUILayout.Foldout(showSine, "Sine");
        if (showSine)
        {
            EditorGUILayout.PropertyField(sineAllowNegative, new GUIContent("Allow Negative Values"));
            EditorGUILayout.Slider(sineFrequency, 0.1f, 100f);
            EditorGUILayout.Slider(sineFalloff, 0.0f, 1f);
            EditorGUILayout.Slider(sineStrength, 0.01f, 1f);

            if (GUILayout.Button("Propagate Wave"))
            {
                terrain.PropagateSine();
            }

        }
        #endregion
        #region Mpd
        showMpd = EditorGUILayout.Foldout(showMpd, "Midpoint Displacement");
        if (showMpd)
        {
            EditorGUILayout.Slider(mpdRoughness, 0.5f, 4.0f);
            EditorGUILayout.Slider(mpdDisplacementFactor, 0.001f, 0.01f);
            EditorGUILayout.Slider(mpdUpperBoundsRatio, 0.05f, 2.0f);
            EditorGUILayout.Slider(mpdLowerBoundsRatio, 0.05f, 2.0f);

            if (GUILayout.Button("Run MPD"))
            {
                terrain.TerrainFromMpd();
            }
        }
        #endregion
        #region Textures
        showTextures = EditorGUILayout.Foldout(showTextures, "Texture Layers");
        if (showTextures)
        {
            textureTable = GUITableLayout.DrawTable(textureTable, serializedObject.FindProperty("terrainTextures"));
            GUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+"))
            {
                terrain.AddNewTexture();
            }
            if (GUILayout.Button("-"))
            {
                terrain.RemoveTextures();
            }
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Apply Textures"))
            {
                terrain.ApplyTerrainTextures();
            }
        
        }
        #endregion
        //TODO add more generation methods
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        #endregion
        if (GUILayout.Button("Smooth"))
            terrain.AdjacencyAverage();
        if (GUILayout.Button("Reset Terrain"))
            terrain.ResetTerrain();



        serializedObject.ApplyModifiedProperties();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
