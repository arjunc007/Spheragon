using UnityEngine;
using GoogleMobileAds.Api;


public class AdManager : MonoBehaviour
{ 
    public static AdManager instance = null;
    public RectTransform gameUI;
    private BannerView bannerView;
    private readonly string testAdID = "ca-app-pub-3940256099942544/6300978111";
    private readonly string bannerAdID_android = "ca-app-pub-1613567415878541/2030859285";

    private void Awake()
    {
        if (instance != null)
            Destroy(gameObject);
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    // Use this for initialization
    void Start()
    {
        InputManager.instance.OrientationEvent += ReloadAd;

        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize(initStatus => { });

        Debug.Log("Ads Initiaised");

        this.RequestBanner();
    }

    private void RequestBanner()
    {
        // Create a 320x50 banner at the top of the screen.
        if (!InputManager.instance.IsPortrait())
        {
            gameUI.offsetMax = new Vector2(gameUI.offsetMax.x, 0);
        }
        else
        {
            gameUI.offsetMax = new Vector2(gameUI.offsetMax.x, -50);
        }

        bannerView = new BannerView(bannerAdID_android, AdSize.Banner, AdPosition.Top);

        // Create an empty ad request.
        //AdRequest request = new AdRequest.Builder().AddTestDevice("61FF8CC5A520CDFF8D2BDE246EF96DCA").Build();


        //Production
        AdRequest request = new AdRequest.Builder().Build();

        // Load the banner with the request.
        bannerView.LoadAd(request);

        Debug.Log("AdLoaded");

        bannerView.Hide();
    }

    public void ShowAd(bool show)
    {
        if (show)
            bannerView.Show();
        else
            bannerView.Hide();

        Debug.Log("Ad Shown: " + show.ToString());
    }

    private void OnApplicationQuit()
    {
        bannerView.Destroy();
    }

    void ReloadAd()
    {
        if (!InputManager.instance.IsPortrait())
        {
            gameUI.offsetMax = new Vector2(gameUI.offsetMax.x, 0);
        }
        else
        {
            gameUI.offsetMax = new Vector2(gameUI.offsetMax.x, -50);
        }
    }
}
