﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Material startMat;
    public static float transitionTime = 0.5f;
    public Shape shape;

    public List<Tile> neighbours = new List<Tile>();

    private Vector3 normal;
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

        normal = GetComponent<MeshFilter>().mesh.normals[0];
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

    public void ChangeTo(Player player)
    {
        owner = player.GetID();

        color = material.color;
        targetColor = player.GetColor();
        startTime = Time.time;

        StartCoroutine(ChangeColor());
    }

    private IEnumerator ChangeColor()
    {
        float t;

        do
        {
            t = (Time.time - startTime) / transitionTime;
            material.color = Color.Lerp(color, targetColor, t);
            yield return null;
        } while (t < 1);
        
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
