using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorButton : MonoBehaviour {

    public int playerID;
    public Image preview;
    Button button;

    private void Start()
    {
        preview.color = GameData.playerColor[playerID];
    }

    private void OnEnable()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => ChangePlayerColor(playerID));
    }

    private void ChangePlayerColor(int player)
    {
        GameData.playerColor[player] = GetComponent<Image>().color;
        preview.color = GameData.playerColor[player];
    }

    private void OnDisable()
    {
        button.onClick.RemoveAllListeners();
    }
}
