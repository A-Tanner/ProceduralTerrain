using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CustomTerrain))] //links this script with the editor of CustomTerrain
[CanEditMultipleObjects]
public class CustomTerrainEditor : Editor
{

    //Properties
    SerializedProperty randomHeightRange;

    //Fold outs
    bool showRandom = false;

    void OnEnable() //Essentially Awake. Allows processing without rerun
    {
        randomHeightRange = serializedObject.FindProperty("randomHeightRange");
        
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        CustomTerrain terrain = (CustomTerrain)target;
        //Should use serialized values between the editor and target
        //This allows user input to stay live in the inspector
        //Even after script changes
        GUILayout.Label("Reset Terrain", EditorStyles.boldLabel);
        if (GUILayout.Button("Reset Terrain")) 
            terrain.ResetTerrain();

        showRandom = EditorGUILayout.Foldout(showRandom, "Random");
        if (showRandom)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Set Heights Between Random Values", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(randomHeightRange);
            if (GUILayout.Button("Randomize Heights"))
            {
                terrain.RandomTerrain();
            }
        }


        serializedObject.ApplyModifiedProperties();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
