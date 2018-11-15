using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Shape shape;

    public List<Tile> neighbours = new List<Tile>();

    public int owner = 0;
    private Animator anim;

    public void Start()
    {
        FindNeighbours();

        if (neighbours.Count == 6)
            shape = Shape.Hexagon;
        else
            shape = Shape.Pentagon;
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

    public void ChangeColor(int player)
    {
        if (owner == 0)
        {
            if (player == 1)
            {
                anim.SetTrigger("GrayToBlue");
            }
            else if (player == 2)
            {
                anim.SetTrigger("GrayToRed");
            }
        }
        else if (owner == 1 && player != 1)
        {
            anim.SetTrigger("BlueToRed");
        }
        else if (owner == 2 && player != 2)
        {
            anim.SetTrigger("RedToBlue");
        }

        owner = player;
        StartCoroutine(ChangeNeighbors());
    }

    IEnumerator ChangeNeighbors()
    {
        yield return new WaitForSeconds(0.2f);

        foreach (Tile neighbor in neighbours)
        {
            if (neighbor.owner == 0 || neighbor.owner == owner)
                continue;
            else if (neighbor.owner == 1)
            {
                neighbor.anim.SetTrigger("BlueToRed");
                neighbor.owner = 2;
            }
            else if (neighbor.owner == 2)
            {
                neighbor.anim.SetTrigger("RedToBlue");
                neighbor.owner = 1;
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
}
