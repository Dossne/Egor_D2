using System.Collections;
using UnityEngine;

namespace ClawbearGames
{
    /// <summary>
    /// Deprecated ad provider stub kept to preserve existing scene references.
    /// Ads are intentionally disabled for this project.
    /// </summary>
    public class UnityAdController : MonoBehaviour
    {
        public void ShowBannerAd(float delay)
        {
            StartCoroutine(CRDelay(delay));
        }

        public void HideBannerAd()
        {
        }

        public bool IsInterstitialAdReady()
        {
            return false;
        }

        public void ShowInterstitialAd(float delay)
        {
            StartCoroutine(CRDelay(delay));
        }

        public bool IsRewardedAdReady()
        {
            return false;
        }

        public void ShowRewardedAd(float delay)
        {
            StartCoroutine(CRDelay(delay));
        }

        private IEnumerator CRDelay(float delay)
        {
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }
        }
    }
}
