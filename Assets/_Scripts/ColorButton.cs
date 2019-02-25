using UnityEngine;
using UnityEngine.UI;

public class ColorButton : MonoBehaviour {

    public int playerID;
    private Image preview;
    private Button button;

    private void Start()
    {
        preview = transform.GetComponent<Image>();
        if(preview)
            preview.color = GameData.colorChoices[GameData.playerColorIndex[playerID]];
    }

    public void ChangePlayerColor()
    {
        //Get player's color
        int currentColorIndex = GameData.playerColorIndex[playerID];

        //Increment color index
        currentColorIndex = currentColorIndex + 1 > 4 ? 0 : currentColorIndex + 1;

        //If index of other player is same, increase again
        if (GameData.playerColorIndex[playerID ^ 1] == currentColorIndex)
        {
            currentColorIndex = currentColorIndex + 1 > 4 ? 0 : currentColorIndex + 1;
        }

        //Assign color at current index to preview and store in GameData
        GameData.playerColorIndex[playerID] = currentColorIndex;
        preview.color = GameData.colorChoices[GameData.playerColorIndex[playerID]];

        string prefName = "P" + (playerID + 1).ToString() + "Color";
        PlayerPrefs.SetInt(prefName, GameData.playerColorIndex[playerID]);
    }
}
