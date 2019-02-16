using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Player
{
    private int id = 0;
    private Color color;
    private HashSet<Tile> ownedTiles = new HashSet<Tile>();
    private int depth = 1;
    private bool extraTurn = false;
    private bool isAI = false;

    public Player(int id, Color c, bool ai = false)
    {
        this.id = id;
        color = c;
        isAI = ai;
    }

    public HashSet<Tile> GetTiles()
    {
        return ownedTiles;
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

    public bool ExtraTurn()
    {
        return extraTurn;
    }

    public void ExtraTurn(bool b)
    {
        extraTurn = b;
    }

    public bool IsAI()
    {
        return isAI;
    }
}