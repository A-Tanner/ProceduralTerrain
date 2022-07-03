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
    public int perlinXOffset = 0;
    public int perlinYOffset = 0;
    public int perlinOctaves = 3;
    public float perlinPersistance = 8;
    public float perlinHeightScale = 0.09f;
    //Voronoi
    public float voronoiFalloff = 1f;
    public float voronoiDropoff = 1f;
    public float voronoiMaxHeight = 1f;
    //Sine
    public bool  sineAllowNegative = true;
    public float sineFrequency = 2.0f;
    public float sineFalloff = 12f;
    public float sineStrength = 0.4f;
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
                heightMap[i, j] += Utils.FractalBrownianMotion(j * perlinXScale,
                    i * perlinYScale,
                    perlinOctaves,
                    perlinPersistance,
                    perlinXOffset,
                    perlinYOffset) * perlinHeightScale;
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }
    #endregion
    #region Voronoi
    public void RandomPeak()
    {
        float[,] heightMap = GetInitialHeights();
        int randX = Random.Range(0, terrainData.heightmapResolution);
        int randY = Random.Range(0, terrainData.heightmapResolution);
        float randElevation = Random.Range(0.0f, voronoiMaxHeight);



        heightMap[randX, randY] = randElevation;

        float maxDistance = Vector2.Distance(new Vector2(0, 0), new Vector2(terrainData.heightmapResolution, terrainData.heightmapResolution));

        for (int i = 0; i < terrainData.heightmapResolution; i++)
        {
            for (int j = 0; j < terrainData.heightmapResolution; j++)
            {
                if (!(j == randX && i == randY)) //If the given heightmap index is not the peak...
                {
                    float distanceToPeak = Vector2.Distance(new Vector2(randX, randY), new Vector2(j, i));
                    float distanceRatio = distanceToPeak / maxDistance;
                    heightMap[j, i] = randElevation - (randElevation * Mathf.Pow((distanceRatio * voronoiFalloff), voronoiDropoff));
                }
            }
        }

        terrainData.SetHeights(0, 0, heightMap);
    }
    #endregion
    #region Sine
    public void PropagateSine()
    {
        float[,] heightMap = GetInitialHeights();
        int randX = Random.Range(0, terrainData.heightmapResolution);
        int randY = Random.Range(0, terrainData.heightmapResolution);


        float maxDistance = Vector2.Distance(new Vector2(0, 0), new Vector2(terrainData.heightmapResolution, terrainData.heightmapResolution));

        for (int i = 0; i < terrainData.heightmapResolution; i++)
        {
            for (int j = 0; j < terrainData.heightmapResolution; j++)
            {

                float distanceFromCenter = Vector2.Distance(new Vector2(randX, randY), new Vector2(j, i));
                float distanceRatio = distanceFromCenter / (maxDistance*sineFalloff);
                float angle = 2 * Mathf.PI * (distanceFromCenter/maxDistance) * sineFrequency;

                if (sineAllowNegative)
                    heightMap[j, i] += Mathf.Sin(angle) * (1 - distanceRatio) * sineStrength;
                else
                    heightMap[j, i] += (1+Mathf.Sin(angle))/2 * (1 - distanceRatio) * sineStrength;

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
        terrainData = terrain.terrainData;

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
