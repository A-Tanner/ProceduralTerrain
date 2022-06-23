using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class CustomTerrain : MonoBehaviour
{
    public bool additive = false;
    public Terrain terrain;
    public TerrainData terrainData;
    //Random
    public Vector2 randomHeightRange = new Vector2(0, 0.1f);
    //From image
    public Texture2D heightMapImage;
    public Vector3 heightMapScale = new Vector3(1.0f, 1.0f, 1.0f);
    //Perlin noise
    public float perlinXScale = 0.001f;
    public float perlinYScale = 0.001f;
    public void ResetTerrain()
    {
        float[,] heightMap = new float[terrainData.heightmapResolution, terrainData.heightmapResolution];
        terrainData.SetHeights(0, 0, heightMap);
    }

    #region Random
    public void RandomTerrain()
    {
        float[,] heightMap = GetInitialHeights();
        

        for (int i = 0; i < heightMap.GetLength(0); i++)
        {
            for (int j = 0; j < heightMap.GetLength(1); j++)
                heightMap[i, j] += Random.Range(randomHeightRange.x, randomHeightRange.y);
        }

        terrainData.SetHeights(0, 0, heightMap);
    }
    #endregion
    #region FromImage
    public void TerrainFromImage() {
        float[,] heightMap = GetInitialHeights();

        for (int i = 0; i < terrainData.heightmapResolution; i++)
        {
            for (int j =0; j < terrainData.heightmapResolution; j++)
            {
                int xPixel =  (int)(i *heightMapScale.x);
                int yPixel =  (int)(j *heightMapScale.z);
                heightMap[i, j] += heightMapImage.GetPixel(xPixel, yPixel).grayscale * heightMapScale.y;
            }
        }

        terrainData.SetHeights(0, 0, heightMap);
    }

    public void TerrainFromImage(float xTiles, float zTiles, float strength)
    {
        float[,] heightMap = GetInitialHeights();
        heightMapScale.y = strength;
        heightMapScale.x = ((float)heightMapImage.width / ((float)terrainData.heightmapResolution)*xTiles);
        heightMapScale.z = ((float)heightMapImage.height / ((float)terrainData.heightmapResolution)*zTiles);

        TerrainFromImage();
    }
    #endregion
    #region Perlin
    public void TerrainFromPerlin()
    {
        float[,] heightMap = GetInitialHeights();
        
        for (int i = 0; i < terrainData.heightmapResolution; i++)
        {
            for (int j = 0; j < terrainData.heightmapResolution; j++)
            {
                heightMap[i, j] += Mathf.PerlinNoise(j * perlinXScale, i * perlinYScale);
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }
    #endregion
    public void Awake()
    {
        SerializedObject tagManager = new SerializedObject
            (AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");
        AddTag(tagsProp, "Terrain");
        AddTag(tagsProp, "Cloud");
        AddTag(tagsProp, "Shore");

        tagManager.ApplyModifiedProperties();

        this.gameObject.tag = "Terrain";
    }

    private float[,] GetInitialHeights()
    {
        float[,] heightMap;

        if (additive)
            heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);
        else
            heightMap = new float[terrainData.heightmapResolution, terrainData.heightmapResolution];

        return heightMap;
    }
    private void OnEnable()
    {
        terrain = this.GetComponent<Terrain>();
        terrainData = Terrain.activeTerrain.terrainData;

    }

    void AddTag(SerializedProperty tagsProp, string newTag)
    {
        bool found = false;
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue.Equals(newTag)) { found = true; break; }
        }

        if (!found)
        {
            tagsProp.InsertArrayElementAtIndex(0);
            SerializedProperty newTagProp = tagsProp.GetArrayElementAtIndex(0);
            newTagProp.stringValue = newTag;
        }
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
