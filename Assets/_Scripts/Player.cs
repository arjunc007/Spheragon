using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Player
{
    private int id = 0;
    private Color color;
    private List<Tile> ownedTiles = new List<Tile>();
    private int depth = 1;

    public Player(int id, Color c)
    {
        this.id = id;
        color = c;
    }

    public Color GetColor()
    {
        return color;
    }

    public int GetID()
    {
        return id;
    }

    public int GetScore()
    {
        return ownedTiles.Count;
    }

    public bool Contains(Tile t)
    {
        return ownedTiles.Contains(t);
    }

    public void AddTile(Tile t)
    {
        ownedTiles.Add(t);
    }

    public void RemoveTile(Tile t)
    {
        ownedTiles.Remove(t);
    }

    public int Depth()
    {
        return depth;
    }

    public void SetDepth(int i)
    {
        depth = i;
    }
}