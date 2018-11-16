using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Material startMat;
    public float transitionTime = 0.5f;
    public Shape shape;

    public List<Tile> neighbours = new List<Tile>();

    private int owner = 0;
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

    public void ChangeColor(Color target)
    {
        color = material.color;
        targetColor = target;
        startTime = Time.time;

        StartCoroutine(ChangeColor());
        //owner = player;
        //StartCoroutine(ChangeNeighbors());
    }

    private IEnumerator ChangeColor()
    {
        isChanging = true;
        yield return new WaitForSeconds(transitionTime);
        isChanging = false;
    }

    IEnumerator ChangeNeighbors()
    {
        yield return new WaitForSeconds(0.2f);

        //foreach (Tile neighbor in neighbours)
        //{
        //    if (neighbor.owner == 0 || neighbor.owner == owner)
        //        continue;
        //    else if (neighbor.owner == 1)
        //    {
        //        neighbor.anim.SetTrigger("BlueToRed");
        //        neighbor.owner = 2;
        //    }
        //    else if (neighbor.owner == 2)
        //    {
        //        neighbor.anim.SetTrigger("RedToBlue");
        //        neighbor.owner = 1;
        //    }

        //}
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
        return owner == 0;
    }
}
