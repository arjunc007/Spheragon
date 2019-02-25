//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Monetization;


//public class UnityAdsScript : MonoBehaviour {

//    public static UnityAdsScript instance = null;
//    private readonly string bannerAd = "video";

//    private void Awake()
//    {
//        if (instance != null)
//            Destroy(gameObject);
//        else
//        {
//            instance = this;
//            DontDestroyOnLoad(gameObject);
//        }
//    }

//    // Use this for initialization
//    void Start () {
//        Monetization.Initialize("3056259", true);
//        StartCoroutine(ShowBannerWhenReady());
//	}

//    private IEnumerator ShowBannerWhenReady()
//    {
//        while(!Monetization.IsReady(bannerAd))
//        {
//            yield return new WaitForSecondsRealtime(0.5f);
//            Debug.Log("Ad not ready");
//        }

//        ShowAdPlacementContent ad = null;
//        ad = Monetization.GetPlacementContent(bannerAd) as ShowAdPlacementContent;

//        if(ad != null)
//        {
//            ad.Show();
//        }
//        Debug.Log("Banner Shown");
//    }
//}
