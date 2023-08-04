using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameCountdownUI : MonoBehaviour
{
    [SerializeField] private Sprite Sprite_Three;
    [SerializeField] private Sprite Sprite_Two;
    [SerializeField] private Sprite Sprite_One;

    [SerializeField] private Image CountdownImage;

    WaitForSeconds InitialCountdownDelay = new WaitForSeconds(0.5f);

    public void ShowUI()
    {
        gameObject.SetActive(true);
        StartCoroutine(StartCountdown());
    }
    
    private IEnumerator StartCountdown()
    {
        yield return InitialCountdownDelay;

        GameInstance.Instance.SoundManager.PlaySoundEffectOneShot(Constants.SFX_Three);
        CountdownImage.sprite = Sprite_Three; 
        yield return new WaitForSeconds(1f);

        GameInstance.Instance.SoundManager.PlaySoundEffectOneShot(Constants.SFX_Two);
        CountdownImage.sprite = Sprite_Two;
        yield return new WaitForSeconds(1f);

        GameInstance.Instance.SoundManager.PlaySoundEffectOneShot(Constants.SFX_One);
        CountdownImage.sprite = Sprite_One;
        yield return new WaitForSeconds(1f);

        GameInstance.Instance.SoundManager.PlaySoundEffectOneShot(Constants.SFX_Whistle);
        CountdownImage.sprite = null;
        GameInstance.Instance.OnGameSceneCountdownFinished();
        gameObject.SetActive(false);
    }
}
