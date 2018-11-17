using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Material startMat;
    public float transitionTime = 0.5f;
    public Shape shape;

    public List<Tile> neighbours = new List<Tile>();

    private int owner = -1;
    private bool isChanging = false;
    private Material material;
    private Color color, targetColor;
    float startTime;

    public void Start()
    {
        //Set material as copy of original
        material = new Material(startMat);
        GetComponent<Renderer>().material = material;

        FindNeighbours();

        if (neighbours.Count == 6)
            shape = Shape.Hexagon;
        else
            shape = Shape.Pentagon;
    }

    public void Update()
    {
        if (isChanging)
        {
            float t = (Time.time - startTime) / transitionTime;
            material.color = Color.Lerp(color, targetColor, t);
        }
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

    public void ChangeTo(int id, Color color, bool changeNeighbor = true)
    {
        owner = id;
        StartCoroutine(ChangeColor(color));

        if(changeNeighbor)
            StartCoroutine(ChangeNeighbors(color));
    }

    private IEnumerator ChangeColor(Color target)
    {
        color = material.color;
        targetColor = target;
        startTime = Time.time;

        isChanging = true;
        yield return new WaitForSeconds(transitionTime);
        isChanging = false;
    }

    IEnumerator ChangeNeighbors(Color color)
    {
        yield return new WaitForSeconds(transitionTime);

        foreach (Tile neighbor in neighbours)
        {
            if (neighbor.IsFree() || neighbor.owner == owner)
                continue;
            else
            {
                neighbor.ChangeTo(owner, color, false);
            }
        }
    }

    public int GetBlueNeighbors()
    {
        int count = 0;
        foreach (Tile neighbor in neighbours)
        {
            if (neighbor.owner == 1)
                count++;
        }
        return count;
    }

    public void SetOwner(int i)
    {
        owner = i;
    }

    public bool IsFree()
    {
        return owner == -1;
    }
}
