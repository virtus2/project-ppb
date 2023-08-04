using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI loadingText;

    [SerializeField]
    private Image loadingImage;

    private WaitForSeconds updateTime = new WaitForSeconds(0.16f);

    private string[] loadingString =
    {
        "Loading",
        "Loading.",
        "Loading..",
        "Loading...",
    };

    private int currentIndex = 0;

    public void ShowUI()
    {
        gameObject.SetActive(true);
        StartCoroutine(UpdateLoadingText());
        StartCoroutine(UpdateLoadingImage());
    }

    private void OnDestroy()
    {
        StopCoroutine(UpdateLoadingText());
        StopCoroutine(UpdateLoadingImage());
    }

    private IEnumerator UpdateLoadingText()
    {
        while (true)
        {
            loadingText.text = loadingString[currentIndex++ % loadingString.Length];
            yield return updateTime;
        }
    }
    
    private IEnumerator UpdateLoadingImage()
    {
        while(true)
        {
            loadingImage.rectTransform.Rotate(new Vector3(0, 0, 1f), -1f);
            yield return null;
        }
    }
}
