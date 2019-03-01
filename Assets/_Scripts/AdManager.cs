using UnityEngine;
using GoogleMobileAds.Api;


public class AdManager : MonoBehaviour
{ 
    public static AdManager instance = null;
    public RectTransform gameUI;
    private BannerView bannerView;
    private readonly string testAdID = "ca-app-pub-3940256099942544/6300978111";
    private readonly string bannerAdID_android = "ca-app-pub-5106176281120401/7918618705";

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

#if UNITY_ANDROID
        string appId = "ca-app-pub-5106176281120401~5994296974";
#elif UNITY_IPHONE
            string appId = "ca-app-pub-3940256099942544~1458002511";
#else
            string appId = "unexpected_platform";
#endif

        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize(appId);

        this.RequestBanner();
    }

    private void RequestBanner()
    {
        //#if UNITY_ANDROID
        //        string adUnitId = "ca-app-pub-5106176281120401/7918618705";
        //#elif UNITY_IPHONE
        //            string adUnitId = "ca-app-pub-3940256099942544/2934735716";
        //#else
        //            string adUnitId = "unexpected_platform";
        //#endif

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

        bannerView.Hide();
    }

    public void ShowAd(bool show)
    {
        if (show)
            bannerView.Show();
        else
            bannerView.Hide();
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
