using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResurrectGaugeUI : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Vector2 offset;

    public float ResurrectionTime { get; private set; } = 0f;
    public float ElapsedTime { get; private set; } = 0f;
    public bool OnResurrecting { get; private set; } = false;

    private void Update()
    {
        if (OnResurrecting)
        {
            ElapsedTime += Time.deltaTime;
            float fillAmount = (ResurrectionTime - ElapsedTime) / ResurrectionTime;
            image.fillAmount = fillAmount;

            if(fillAmount <= 0.0001f)
            {
                gameObject.SetActive(false);
            }
        }
    }

    public void ShowUI(float resurrectionTime)
    {
        gameObject.SetActive(true);
        ResurrectionTime = resurrectionTime;
        ElapsedTime = 0f;
        OnResurrecting = true;
    }
}
