using UnityEngine;

public static class GameData {

    public static Color[] colorChoices = { new Color(0.9490197f, 0.2078432f, 0.2666667f),
                                            new Color(0.6588235f, 0.2784314f, 0.7490196f),
                                            new Color(0.1019608f, 0.4705883f, 0.8392158f),
                                            new Color(0f, 0.6392157f, 0.2862745f),
                                            new Color(0.9686275f, 0.854902f, 0.3607843f)};
    public static bool musicOn = true;
    //Red = (242, 53, 68), Blue = (26, 120, 214) Divide by 255
    public static int[] playerColorIndex = { 0, 2 };

    public static void SavePlayerPrefs()
    {
        PlayerPrefs.SetInt("P1Color", playerColorIndex[0]);
        PlayerPrefs.SetInt("P2Color", playerColorIndex[1]);
        PlayerPrefs.Save();
    }

    public static void LoadPlayerPrefs()
    {
        if (PlayerPrefs.HasKey("Music"))
            GameData.musicOn = PlayerPrefs.GetInt("Music") > 0 ? true : false;
        if (PlayerPrefs.HasKey("P1Color"))
            GameData.playerColorIndex[0] = PlayerPrefs.GetInt("P1Color");
        if (PlayerPrefs.HasKey("P2Color"))
            GameData.playerColorIndex[1] = PlayerPrefs.GetInt("P2Color");
    }
}
