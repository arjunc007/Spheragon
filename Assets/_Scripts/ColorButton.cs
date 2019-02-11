using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorButton : MonoBehaviour {

    public int playerID;
    private Image preview;
    Button button;

    private void Start()
    {
        preview = transform.parent.parent.GetChild(0).GetComponent<Image>();
        if(preview)
            preview.color = GameData.playerColor[playerID];
    }

    private void OnEnable()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => ChangePlayerColor(playerID));
    }

    private void ChangePlayerColor(int player)
    {
        Color thisColor = GetComponent<Image>().color;

        if (GameData.playerColor[player ^ 1] != thisColor)
        {
            GameData.playerColor[player] = GetComponent<Image>().color;
            preview.color = GameData.playerColor[player];
        }
        else
        {
            Debug.Log("Color occupied");
        }
    }

    private void OnDisable()
    {
        button.onClick.RemoveAllListeners();
    }
}
