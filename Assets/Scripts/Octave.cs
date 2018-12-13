using UnityEngine;

[System.Serializable]
public class Octave
{
    public Octave()
    {
        label = "";
        persistance = 0.5f;
        lacunarity = 2f;
        rank = 0;
    }

    public string label;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;
    public int rank;
    public float frequency { get { return Mathf.Pow(lacunarity, rank); } }
    public float amplitude { get { return Mathf.Pow(persistance, rank); } }
}