using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileType
{
    Pentagon,
    Hexagon,
    Regular,
    Start,
    Invert,
    RangeUp
};

public class Tile : MonoBehaviour
{
    public Material startMat;
    public AudioClip[] tapSounds;

    public static float transitionTime = 0.5f;
    public TileType type;

    public List<Tile> neighbours = new List<Tile>();

    private Vector3 normal;
    private int owner = -1;
    private Material material;
    private Color color, targetColor;
    float startTime;

    public virtual void Start()
    {
        //Set material as copy of original
        material = new Material(startMat);
        GetComponent<Renderer>().material = material;

        FindNeighbours();

        if (neighbours.Count == 6)
        {
            type = TileType.Hexagon;
        }
        else
        {
            type = TileType.Pentagon;
            //material.SetColor("_FromColor", Color.yellow);
        }

        normal = GetComponent<MeshFilter>().mesh.normals[0];
    }

    private void FindNeighbours()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, GameManager.TileRadius);

        foreach (var collider in hitColliders)
        {
            if(collider.gameObject != gameObject)
                neighbours.Add(collider.GetComponent<Tile>());
        }
    }

    public void SetOwner(Player player)
    {
        owner = player.GetID();
        material.SetColor("_FromColor", player.GetColor());

    }

    public void ChangeTo(Player player)
    {
        AudioSource.PlayClipAtPoint(tapSounds[Random.Range(0, tapSounds.Length)], transform.root.position);

        owner = player.GetID();

        Color edgeColor = player.GetColor();

        material.SetColor("_ToColor", edgeColor);

        float h, s, v;
        Color.RGBToHSV(edgeColor, out h, out s, out v);
        edgeColor = Color.HSVToRGB(h, s * 0.9f, v, true) * Mathf.Pow(2.0f, 2);
        material.SetColor("_EdgeColor", edgeColor);
        
        //targetColor = player.GetColor();
        startTime = Time.time;

        StartCoroutine(ChangeColor());
    }

    private IEnumerator ChangeColor()
    {
        float t;

        do
        {
            t = (Time.time - startTime) / transitionTime;
            material.SetFloat("_Duration", t);
            yield return null;
        } while (t < 1);

        //Set the Final Color to last color
        material.SetColor("_FromColor", material.GetColor("_ToColor"));
        material.SetFloat("_Duration", 0);
    }

    public int GetDiffNeighbours(int playerID)
    {
        int opponentTiles = 0;
        foreach(Tile neighbour in neighbours)
        {
            if (!neighbour.IsFree() && neighbour.owner != playerID)
                opponentTiles++;
        }

        return opponentTiles;
    }

    public Vector3 GetNormal()
    {
        return transform.rotation * normal;
    }

    public void SetOwner(int i)
    {
        owner = i;
    }

    public bool CheckOwner(int i)
    {
        return owner == i;
    }

    public bool IsFree()
    {
        return owner == -1;
    }
}
