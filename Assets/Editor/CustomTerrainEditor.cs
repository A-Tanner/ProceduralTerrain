using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CustomTerrain))] //links this script with the editor of CustomTerrain
[CanEditMultipleObjects]
public class CustomTerrainEditor : Editor
{
    //Properties that get pulled off of the script being edited
    SerializedProperty randomHeightRange;
    SerializedProperty additive;
    SerializedProperty heightMapImage;
    SerializedProperty heightMapScale;

    //Fold outs
    bool showRandom = false;
    bool showImage = false;

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
            EditorGUILayout.PropertyField(heightMapImage);
            GUILayout.Label("X and Z influence how the image is scaled");
            GUILayout.Label("Y is the strength");
            EditorGUILayout.PropertyField(heightMapScale);
            //GUILayout.Label("Additive", EditorStyles.boldLabel);



            if (GUILayout.Button("Generate Geometry From Image"))
            {
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
