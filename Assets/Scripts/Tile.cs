using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{

    public List<Tile> neighbourList = new List<Tile>();

    private CircleCollider2D circleCollider2D;

    public int owner = 0;
    private Animator anim;

    public void SetupTile()
    {
        anim = GetComponent<Animator>();

        circleCollider2D = GetComponent<CircleCollider2D>();
        //circleCollider2D.radius = HexGrid.outerRadius;

        FindNeighbours();

        //circleCollider2D.radius = HexGrid.innerRadius;
    }

    private void FindNeighbours()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, HexGrid.outerRadius);

        foreach (Collider2D collider in hitColliders)
        {
            if (collider.name == "HexagonTile(Clone)" && collider.transform.position != transform.position)
            {
                neighbourList.Add(collider.GetComponent<Tile>());
            }
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

        foreach (Tile neighbor in neighbourList)
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
        foreach (Tile neighbor in neighbourList)
        {
            if (neighbor.owner == 1)
                count++;
        }
        return count;
    }
}
