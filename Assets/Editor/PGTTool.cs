using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PGTTool : EditorWindow
{
    private float scale = 20;
    private Vector2 offset;
    private int width = 256;
    private int height = 256;
    private int seed = 0;
    private int exponent = 13;
    private List<Octave> octaves;
    public List<Biome> biomes;
    public AnimationCurve heightCurve;
    private bool octavesEnabled = true;
    private bool hideOctaves = true;
    private bool hideRegions = true;
    private Vector2 octavesScroll = Vector2.zero;
    private Vector2 regionsScroll = Vector2.zero;

    private static PGTTool instance;

    [MenuItem("Window/Procedural Terrain Generator")]
    static void Init()
    {
        instance = (PGTTool) GetWindow(typeof(PGTTool));
    }

    private void OnGUI()
    {
        if (octaves == null)
        {
            octaves = new List<Octave>();
        }

        if (biomes == null)
        {
            biomes = new List<Biome>();
        }

        EditorGUILayout.LabelField("Procedural Terrain Generator", EditorStyles.boldLabel);

        Rect baseRect = EditorGUILayout.BeginVertical();
        scale = EditorGUILayout.FloatField("Scale", scale);
        width = EditorGUILayout.IntField("Width", width);
        height = EditorGUILayout.IntField("Height", height);
        seed = EditorGUILayout.IntField("Seed", seed);
        exponent = EditorGUILayout.IntField("Exponent", exponent);
        offset = EditorGUILayout.Vector2Field("Offset", offset);
        heightCurve = EditorGUILayout.CurveField("Height Curve", heightCurve);
        EditorGUILayout.EndVertical();

        octavesEnabled = EditorGUILayout.BeginToggleGroup("Use octaves?", octavesEnabled);
        hideOctaves = EditorGUILayout.Foldout(hideOctaves, "List octaves");

        if (!hideOctaves && octavesEnabled)
        {
            octavesScroll = EditorGUILayout.BeginScrollView(octavesScroll);
            foreach (Octave item in octaves)
            {
                item.label = EditorGUILayout.TextField("Label", item.label);
                item.persistance = EditorGUILayout.FloatField("Persistance", item.persistance);
                item.lacunarity = EditorGUILayout.FloatField("Lacunarity", item.lacunarity);
                item.rank = EditorGUILayout.IntField("Rank", item.rank);
                EditorGUILayout.Separator();
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Octave"))
            {
                octaves.Add(new Octave());
                Repaint();
            }
            if (GUILayout.Button("Clear Octaves"))
            {
                octaves.Clear();
                Repaint();
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndToggleGroup();

        hideRegions = EditorGUILayout.Foldout(hideRegions, "List regions");

        if (!hideRegions)
        {
            regionsScroll = EditorGUILayout.BeginScrollView(regionsScroll);
            foreach (Biome item in biomes)
            {
                item.name = EditorGUILayout.TextField("Label", item.name);
                item.height = EditorGUILayout.FloatField("Heigth", item.height);
                item.color = EditorGUILayout.ColorField("Color", item.color);
                EditorGUILayout.Separator();
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Region"))
            {
                biomes.Add(new Biome());
                Repaint();
            }
            if (GUILayout.Button("Clear Regions"))
            {
                biomes.Clear();
                Repaint();
            }
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Generate"))
        {
            makeAsset(width, height, seed, scale);
        }
    }

    public void makeAsset(int mapWidth, int mapHeight, int seed, float noiseScale)
    {
        float[,] noiseMap;
        if (octavesEnabled && octaves.Count > 0)
        {
            noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, offset, octaves);
        }
        else
        {
            noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, offset);
        }
        
        Color[] colorMap = new Color[mapWidth * mapHeight];
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < biomes.Count; i++)
                {
                    if (currentHeight <= biomes[i].height)
                    {
                        colorMap[y * mapHeight + x] = biomes[i].color;
                        break;
                    }
                }
            }
        }

        MeshData data = MeshGenerator.GenerateTerrainMesh(noiseMap, exponent, heightCurve);
        Texture2D texture = TextureGenerator.textureFromColorMap(colorMap, mapWidth, mapHeight);
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Plane);

        Mesh mesh = go.GetComponent<MeshFilter>().sharedMesh;
        go.name = "Terrain";
        mesh = DrawMesh(data, texture, mesh);
        go.GetComponent<MeshRenderer>().sharedMaterial.mainTexture = texture;
        go.gameObject.AddComponent<MeshCollider>();
        generatePrefab(go);
    }

    public Mesh DrawMesh(MeshData meshData, Texture2D texture, Mesh mesh)
    {
        return meshData.updateMesh(mesh);
    }

    private void generatePrefab(GameObject gameObject)
    {
        string localPath = "Assets/" + gameObject.name + ".prefab";

        //Check if the Prefab and/or name already exists at the path
        if (AssetDatabase.LoadAssetAtPath(localPath, typeof(GameObject)))
        {
            //Create dialog to ask if User is sure they want to overwrite existing Prefab
            if (EditorUtility.DisplayDialog("Are you sure?",
                "The Prefab already exists. Do you want to overwrite it?",
                "Yes",
                "No"))
            //If the user presses the yes button, create the Prefab
            {
                CreateNew(gameObject, localPath);
            }
        }
        //If the name doesn't exist, create the new Prefab
        else
        {
            Debug.Log(gameObject.name + " is not a Prefab, will convert");
            CreateNew(gameObject, localPath);
        }
    }

    void CreateNew(GameObject obj, string localPath)
    {
        //Create a new Prefab at the path given
        Object prefab = PrefabUtility.CreatePrefab(localPath, obj);
        PrefabUtility.ReplacePrefab(obj, prefab, ReplacePrefabOptions.ConnectToPrefab);
    }
}
