using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ProceduralTerrain;

[ExecuteInEditMode]
public class CustomTerrain : MonoBehaviour
{
    public bool additive = false;
    public Terrain terrain;
    public TerrainData terrainData;
    //Random
    public Vector2 randomHeightRange = new Vector2(0.0f, 0.1f);
    //From image
    public Texture2D heightMapImage;
    public Vector3 heightMapScale = new Vector3(1.0f, 1.0f, 1.0f);
    //Perlin noise
    public float perlinXScale = 0.001f;
    public float perlinYScale = 0.001f;
    public int perlinXOffset = 0;
    public int perlinYOffset = 0;
    public int perlinOctaves = 3;
    public float perlinPersistance = 8.0f;
    public float perlinHeightScale = 0.09f;
    //Voronoi
    public float voronoiFalloff = 1.0f;
    public float voronoiDropoff = 1.0f;
    public float voronoiMinHeight = 0.0f;
    public float voronoiMaxHeight = 1.0f;
    public int voronoiNumberPeaks = 1;
    //Sine
    public bool sineAllowNegative = true;
    public float sineFrequency = 2.0f;
    public float sineFalloff = 12.0f;
    public float sineStrength = 0.4f;
    //Midpoint Displacement
    public float mpdRoughness = 2.0f;
    public float mpdDisplacementFactor = 0.01f;
    public float mpdUpperBoundsRatio = 1.0f;
    public float mpdLowerBoundsRatio = 1.0f;
    //Terrain Layers
    public List<TerrainTexture> terrainTextures = new()
    {
        new TerrainTexture()
    };
    //Vegetation
    public List<VegetationLayer> vegetationLayers = new() { new VegetationLayer()};
    public int maxVeg = 5000;
    public float vegSpacing = 5.0f;

    public void ResetTerrain()
    {
        float[,] heightMap = new float[terrainData.heightmapResolution, terrainData.heightmapResolution];
        terrainData.SetHeights(0, 0, heightMap);
    }

    #region Random
    public void RandomTerrain()
    {
        float[,] heightMap = GetInitialHeights();


        for (int x = 0; x < heightMap.GetLength(0); x++)
        {
            for (int y = 0; y < heightMap.GetLength(1); y++)
                heightMap[x, y] += Random.Range(randomHeightRange.x, randomHeightRange.y);
        }

        terrainData.SetHeights(0, 0, heightMap);
    }
    #endregion
    #region FromImage
    public void TerrainFromImage()
    {
        float[,] heightMap = GetInitialHeights();

        for (int x = 0; x < terrainData.heightmapResolution; x++)
        {
            for (int y = 0; y < terrainData.heightmapResolution; y++)
            {
                int xPixel = (int)(x * heightMapScale.x);
                int yPixel = (int)(y * heightMapScale.z);
                heightMap[x, y] += heightMapImage.GetPixel(xPixel, yPixel).grayscale * heightMapScale.y;
            }
        }

        terrainData.SetHeights(0, 0, heightMap);
    }

    public void TerrainFromImage(float xTiles, float zTiles, float strength)
    {
        float[,] heightMap = GetInitialHeights();
        heightMapScale.y = strength;
        heightMapScale.x = ((float)heightMapImage.width / ((float)terrainData.heightmapResolution) * xTiles);
        heightMapScale.z = ((float)heightMapImage.height / ((float)terrainData.heightmapResolution) * zTiles);

        TerrainFromImage();
    }
    #endregion
    #region Perlin
    public void TerrainFromPerlin()
    {
        float[,] heightMap = GetInitialHeights();

        for (int x = 0; x < terrainData.heightmapResolution; x++)
        {
            for (int y = 0; y < terrainData.heightmapResolution; y++)
            {
                heightMap[x, y] += Utils.FractalBrownianMotion(x * perlinXScale,
                    y * perlinYScale,
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

    public void TerrainFromVoronoi()
    {

        float[,] heightMap = GetInitialHeights();

        for (int peakNumber = 0; peakNumber < voronoiNumberPeaks; peakNumber++)
        {

            //Determine coordinate of new peak
            int randX = Random.Range(0, terrainData.heightmapResolution);
            int randY = Random.Range(0, terrainData.heightmapResolution);

            //Determine height of new peak
            float randElevation = Random.Range(voronoiMinHeight, voronoiMaxHeight);

            float maxDistance = Vector2.Distance(new Vector2(0, 0), new Vector2(terrainData.heightmapResolution, terrainData.heightmapResolution));


            //Update all applicable points in the terrain
            for (int x = 0; x < terrainData.heightmapResolution; x++)
            {
                for (int y = 0; y < terrainData.heightmapResolution; y++)
                {
                    float distanceToPeak = Vector2.Distance(new Vector2(randX, randY), new Vector2(y, x));
                    float distanceRatio = distanceToPeak / maxDistance;
                    float height = randElevation - (randElevation * Mathf.Pow((distanceRatio * voronoiFalloff), voronoiDropoff));

                    //Terrain is only updated if a peak generates taller terrain than what currently exists.
                    //This can have the appearance of not creating the right number of peaks, if a peak is created at the point of a larger peak
                    if (height > heightMap[y, x])
                        heightMap[y, x] = height;
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

        for (int x = 0; x < terrainData.heightmapResolution; x++)
        {
            for (int y = 0; y < terrainData.heightmapResolution; y++)
            {

                float distanceFromCenter = Vector2.Distance(new Vector2(randX, randY), new Vector2(y, x));
                float distanceRatio = distanceFromCenter / (maxDistance * sineFalloff);
                float angle = 2 * Mathf.PI * (distanceFromCenter / maxDistance) * sineFrequency;

                if (sineAllowNegative)
                    heightMap[y, x] += Mathf.Sin(angle) * (1 - distanceRatio) * sineStrength;
                else
                    heightMap[y, x] += (1 + Mathf.Sin(angle)) / 2 * (1 - distanceRatio) * sineStrength;

            }
        }

        terrainData.SetHeights(0, 0, heightMap);
    }
    #endregion
    #region Mpd
    public void TerrainFromMpd()
    {
        float[,] heightMap = GetInitialHeights();


        int terrainWidth, terrainHeight;
        terrainWidth = terrainHeight = terrainData.heightmapResolution - 1;
        int squareSize = terrainWidth;

       float randomDisplacement = (float)squareSize / 2.0f * mpdDisplacementFactor;
       float dampening = Mathf.Pow(2, -1 * mpdRoughness);


        //Give the corners of the square random height values   

        while (squareSize > 0)//terrainData.heightmapResolution/32)
        { 
            //Iterate across each square at the present resolution (square size)
            //DIAMOND STEP
            for (int x = 0; x < terrainWidth; x += squareSize)
            {
                for (int y = 0; y < terrainHeight; y += squareSize)
                {
                    DiamondStep(heightMap, squareSize, x, y, randomDisplacement, dampening);
                }
            }
            //SQUARE STEP
            for (int x = 0; x < terrainWidth; x += squareSize)
            {
                for (int y = 0; y < terrainWidth; y += squareSize)
                {
                 SquareStep(heightMap, squareSize, x, y, randomDisplacement, dampening);
                }
            }

            squareSize = (int)(squareSize / 2.0f);
            randomDisplacement *= dampening;
        }


        terrainData.SetHeights(0, 0, heightMap);
    }


    //For DiamondStep and SquareStep, x and y paremeters of the method are the coordinates of the bottom left point
    private void DiamondStep(float[,] heightMap, int squareSize, int x, int y, float randomDisplacement, float dampening)
    {

        int cornerX, cornerY, midX, midY;

        //Find boundary points of the square
        cornerX = (x + squareSize);
        cornerY = (y + squareSize);

        //Derive the midpoint coordinates
        midX = (int)(x + squareSize / 2.0f);
        midY = (int)(y + squareSize / 2.0f);

        //Set the height of the midpoint to be the average height of it's corners
        heightMap[midX, midY] = Utils.AverageFloats(heightMap[x, y],
            heightMap[cornerX, y],
            heightMap[x, cornerY],
            heightMap[cornerX, cornerY]) + 
           (Random.Range(-randomDisplacement*mpdLowerBoundsRatio, randomDisplacement*mpdUpperBoundsRatio) * dampening);
    }
    private void SquareStep(float[,] heightMap, int squareSize, int x, int y, float randomDisplacement, float dampening)
    {
        int midX, midY, halfSquare;
        halfSquare = (int)(squareSize / 2.0f);
        //Derive the midpoint coordinates
        midX = (int)(x + halfSquare);
        midY = (int)(y + halfSquare);

        //Calculate the height of the square points
        SquareMulti(heightMap, halfSquare, midX - halfSquare, midY, randomDisplacement, dampening);
        SquareMulti(heightMap, halfSquare, midX, midY + halfSquare, randomDisplacement, dampening);
        SquareMulti(heightMap, halfSquare, midX + halfSquare, midY, randomDisplacement, dampening);
        SquareMulti(heightMap, halfSquare, midX, midY - halfSquare, randomDisplacement, dampening);



    }

    //x and y parameters define the point being calculated, unlike DiamondStep and SquareStep
    private void SquareMulti(float[,] heightMap, int halfSquare, int x, int y, float randomDisplacement, float dampening)
    {
        int leftX, upY, rightX, downY;

        //derive required coordinates of the four points adjacent to the point being calculated
        leftX = x - halfSquare;
        upY = y + halfSquare;
        rightX = x + halfSquare;
        downY = y - halfSquare;

        //Ensure indecies are in bounds
        if (leftX < 0 || leftX >= terrainData.heightmapResolution ||
            upY < 0 || upY >= terrainData.heightmapResolution ||
            rightX < 0 || rightX >= terrainData.heightmapResolution ||
            downY < 0 || downY >= terrainData.heightmapResolution)
            return;

        //calculate the height the height of heightMap[x,y]
        float calculatedHeight = Utils.AverageFloats(heightMap[leftX, y],
            heightMap[x, upY] ,
            heightMap[rightX, y] ,
            heightMap[downY, x]) +
           (Random.Range(-randomDisplacement * mpdLowerBoundsRatio, randomDisplacement * mpdUpperBoundsRatio) * dampening);

        heightMap[x,y] = calculatedHeight;
    }
    #endregion
    #region Smooth
    public void AdjacencyAverage()
    {
        bool additiveCache = additive;
        additive = true;

        float[,] heightMap = GetInitialHeights();
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        for (int x = 0; x < terrainData.heightmapResolution; x++) //y
        {
            for(int y = 0; y < terrainData.heightmapResolution; y++)//x
            {
                List<float> adjacencies = new();

                Vector2 position = new(Mathf.RoundToInt(x), Mathf.RoundToInt(y));

                adjacencies.Add(heightMap[x, y]);

                //Check adjacencies
                IEnumerable<float> neighbors = Utils.GetValuesAtCoordinates(heightMap,
                    Utils.GetNeighbors(position, width, height));

                adjacencies.AddRange(neighbors);

                heightMap[x, y] = Utils.AverageFloats(adjacencies);
            }
        }

        terrainData.SetHeights(0, 0, heightMap);
        additive = additiveCache;
    }
    #endregion
    #region TerrainTextures
    public void AddNewTexture()
    {
        terrainTextures.Add(new TerrainTexture());
    }

    public void RemoveTextures()
    {
        List<TerrainTexture> keptTextures = new List<TerrainTexture>();

        for (int i = 0; i < terrainTextures.Count; i++)
        {
            if (!terrainTextures[i].remove)
            {
                keptTextures.Add(terrainTextures[i]);
            }
        }
        terrainTextures = keptTextures;

        if (terrainTextures.Count == 0)
        {
            terrainTextures.Add(new TerrainTexture());
        }
    }

    //Uses an edge detection process to generate a vector
    //representing the steepness of a point compared to it's neighbors.
    //1.4 is the theoretical maximum steepness ( magnitude of (1,1) )
    float GetSteepness(float[,] heightmap, int x, int y)
    {
        float h = heightmap[x, y];
        int nextX = x + 1;
        int nextY = y + 1;

        if (nextX > terrainData.alphamapWidth-1) nextX = x - 1;
        if (nextY > terrainData.alphamapHeight-1) nextY = y - 1;

        float dx = heightmap[nextX, y] - h;
        float dy = heightmap[x, nextY] - h;

        Vector2 gradient = new Vector2(dx, dy);
        return gradient.magnitude;
    }

    public void ApplyTerrainTextures()
    {
        TerrainLayer[] layers = new TerrainLayer[terrainTextures.Count];
        int index = 0;
        foreach(TerrainTexture t in terrainTextures)
        {
            layers[index] = new TerrainLayer();
            layers[index].diffuseTexture = t.texture;
            layers[index].diffuseTexture.Apply(true);
            layers[index].tileOffset = t.offset;
            layers[index].tileSize = t.size;
            string path = "Assets/TerrainLayers/New Terrain Layer " + index + ".terrainlayer";
            AssetDatabase.CreateAsset(layers[index], path);
            index++;
            Selection.activeObject = this.gameObject;
        }
        terrainData.terrainLayers = layers;

        float[,] heightMap = terrainData.GetHeights(0, 0,terrainData.heightmapResolution, terrainData.heightmapResolution);
        float[,,] textureLayerData = new float[terrainData.alphamapWidth,
            terrainData.alphamapHeight,
            terrainData.alphamapLayers];
        for ( int y = 0; y < terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < terrainData.alphamapWidth; x++)
            {
                float[] layer = new float[terrainData.alphamapLayers];

                for (int i = 0; i < terrainTextures.Count; i++)
                {
                    float offset = Mathf.PerlinNoise(x * terrainTextures[i].noiseVelocity, y * terrainTextures[i].noiseVelocity) * terrainTextures[i].noiseScale;
                    float heightStart = terrainTextures[i].minHeight - offset;
                    float heightStop = terrainTextures[i].maxHeight + offset;
                    float steepness = GetSteepness(heightMap, x, y);
                    Mathf.Clamp(heightStop, 0, 1);
                    if ((heightMap[x,y] >= heightStart && heightMap[x,y] <= heightStop &&
                         terrainTextures[i].CheckSlope(steepness)))
                    
                    {
                        layer[i] = 1;
                    } 
                }
                Utils.NormalizeVector(layer);
                for (int j = 0; j < terrainTextures.Count; j++)
                {
                    textureLayerData[x, y, j] = layer[j];
                }
            }
        }

        terrainData.SetAlphamaps(0,0,textureLayerData);
    }
    #endregion
    #region Vegetation
    public void PlantVegetation() { }
    public void AddVegetationLayer() { vegetationLayers.Add(new VegetationLayer());  }
    public void RemoveVegetationLayers()
    {
        List<VegetationLayer> savedLayers = new();
        for (int i = 0; i < vegetationLayers.Count; i++)
        {
            if (!vegetationLayers[i].remove)
            {
                savedLayers.Add(vegetationLayers[i]);
            }
        }

        if (savedLayers.Count == 0)
        {
            savedLayers.Add(new VegetationLayer());
        }

        vegetationLayers = savedLayers;
    }
    public void ClearVegetationLayers()
    {
        foreach(VegetationLayer layer in vegetationLayers)
        {
            layer.remove = true;
        }

        RemoveVegetationLayers();
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

    public Texture2D GenerateImprint()
    {
        bool additiveHolder = additive;
        additive = true;
        float[,] heightMap = GetInitialHeights();
        Texture2D imprint = new Texture2D(heightMap.GetLength(0),heightMap.GetLength(1), TextureFormat.RGBA32, false);

        for(int y = 0; y < imprint.height;y++)
        {
            for (int x = 0; x < imprint.width;x++)
            {
                float grayscale = heightMap[x,y];
                imprint.SetPixel(x, y, new Color(grayscale, grayscale, grayscale,1));
               
            }
        }
        additive = additiveHolder;
        imprint.Apply(false, false);
        return imprint;
    }
}
