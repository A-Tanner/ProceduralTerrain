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

    SerializedProperty randomHeightRange;

    SerializedProperty heightMapImage;
    SerializedProperty heightMapScale;

    //Fold outs
    bool showRandom = false;
    bool showImage = false;

    //Editor fields
    bool heightMapAutoScale = false;
    float heightMapXTiles = 0;
    float heightMapZTiles = 0;

    void OnEnable() //Essentially Awake. Allows processing without rerun
    {
        randomHeightRange = serializedObject.FindProperty("randomHeightRange");
        additive = serializedObject.FindProperty("additive");
        heightMapImage = serializedObject.FindProperty("heightMapImage");
        heightMapScale = serializedObject.FindProperty("heightMapScale");
        
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        CustomTerrain terrain = (CustomTerrain)target;
        //Should use serialized values between the editor and target
        //This allows user input to stay live in the inspector
        //Even after script changes

        #region options
        GUILayout.Label("Generation Options", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(additive);
        #endregion

        #region generation
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Label("Generation Methods", EditorStyles.boldLabel);
        #region random
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
        #region texture
        showImage = EditorGUILayout.Foldout(showImage, "From Image");
        if (showImage)
        {
            EditorGUILayout.HelpBox("Ensure your texture has option Non-Power of 2 set to None and Read/Write enabled in the import settings!" +
                " These can be accessed by clicking on the texture asset you'd like to use and changing them in the inspector window",MessageType.Info,true);
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
                    terrain.TerrainFromImage(heightMapXTiles, heightMapZTiles);
                else
                    terrain.TerrainFromImage();
            }
        }
        #endregion
        //TODO add more generation methods
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        #endregion

        if (GUILayout.Button("Reset Terrain"))
            terrain.ResetTerrain();



        serializedObject.ApplyModifiedProperties();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
