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

    //Fold outs
    bool showRandom = false;

    void OnEnable() //Essentially Awake. Allows processing without rerun
    {
        randomHeightRange = serializedObject.FindProperty("randomHeightRange");
        additive = serializedObject.FindProperty("additive");
        
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
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        if (GUILayout.Button("Reset Terrain"))
            terrain.ResetTerrain();
        #endregion
        //TODO add more generation methods
        #endregion


        serializedObject.ApplyModifiedProperties();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
