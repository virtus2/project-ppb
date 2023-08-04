using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameKillLogUIItem : MonoBehaviour
{
    private RectTransform rectTransform;
    public float duration = 4.0f;

    public TextMeshProUGUI killerText;
    [SerializeField] private Image killImage;
    public TextMeshProUGUI victimText;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }
    private IEnumerator Start()
    {
        float elapsedTime = 0.0f;
        while(elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float ratio = elapsedTime / duration;
            if (ratio > 0.75f)
            {
                Debug.Log(ratio);
                killerText.alpha = Mathf.Lerp(1f, 0, ratio);
                killImage.color = new Color(killImage.color.r, killImage.color.g, killImage.color.b, Mathf.Lerp(1f, 0, ratio));
                victimText.alpha = Mathf.Lerp(1f, 0, ratio);
            }
            yield return null;
        }
        Destroy(gameObject);
    }
}
