using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileType
{
    Pentagon,
    Hexagon,
    Start,
    Invert,
    RangeUp,
    Skip
};

public class Tile : MonoBehaviour
{
    public Material startMat;
    public List<AudioClip> tapSounds;

    public static float transitionTime = 0.5f;
    public TileType type;

    public List<Tile> neighbours = new List<Tile>();

    private Vector3 normal;
    private int owner = -1;
    private Material material;
    private Color color, targetColor;
    float startTime;

    public void Initialise()
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
        if(GameData.musicOn)
        {
                AudioSource.PlayClipAtPoint(tapSounds[Random.Range(0, tapSounds.Count)], transform.root.position);
        }

        Color edgeColor;

        if (player != null)
        {
            edgeColor = player.GetColor();
            owner = player.GetID();
        }
        else
        {
            edgeColor = new Color(0.1f, 0.1f, 0.1f);
        }


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

    public void RemovePowerEffect()
    {
        ParticleSystem ps = GetComponentInChildren<ParticleSystem>();
        if (ps)
        {
            ps.Stop();
            StartCoroutine(TranslateAndScale(transform.GetComponentInChildren<SpriteRenderer>().transform));
        }
    }

    IEnumerator TranslateAndScale(Transform powerIcon)
    {
        powerIcon.SetParent(null);

        Quaternion startRot = powerIcon.rotation;
        Quaternion endRot = Quaternion.LookRotation(Vector3.back);

        Vector3 startPos = powerIcon.position;
        Vector3 endPos = new Vector3(0, 0, -13);

        float animTime = 0, speed = 2;
        //Speed in pixel/second
        while (powerIcon.position.z != -13)
        {
            powerIcon.position = Vector3.Lerp(startPos, endPos, speed * animTime);
            powerIcon.rotation = Quaternion.Lerp(startRot, endRot, speed * animTime);
            animTime += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSecondsRealtime(0.5f);

        endPos.Set(10, 10, -13);
        speed = 5;
        animTime = 0;

        while (powerIcon.position != endPos)
        {
            powerIcon.position = Vector3.Lerp(startPos, endPos, speed * animTime);
            animTime += Time.deltaTime;
            yield return null;
        }

        Destroy(powerIcon.gameObject, 5);
        Destroy(this);
    }

    private IEnumerator FadeIcon(SpriteRenderer sr)
    {
        float t = 0;
        float alphaValue = 1;
        while(sr.color.a > 0)
        {
            alphaValue = Mathf.Lerp(1, 0, t);
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alphaValue);
            
            t += Time.deltaTime;
            yield return null;
        }
    }

    public void SetTapSound(AudioClip powerTapSound)
    {
        tapSounds.Clear();
        tapSounds.Add(powerTapSound);
    }
}
