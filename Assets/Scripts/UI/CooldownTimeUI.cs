using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CooldownTimeUI : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Vector2 offset;
    [SerializeField] private Color OnCooldownColor;
    [SerializeField] private Color CooldownFinishedColor;

    public float CooldownTime { get; private set; } = 0f;
    public float ElapsedTime { get; private set; } = 0f;
    public bool OnCooldown { get; private set; } = false;

    private bool isShowing;

    private void Awake()
    {
        image = GetComponent<Image>();
        image.color = CooldownFinishedColor;
    }

    private void Update()
    {
        if(OnCooldown)
        {
            ElapsedTime += Time.deltaTime;
            image.fillAmount = ElapsedTime / CooldownTime;
            image.color = OnCooldownColor;
        }
        
        if(ElapsedTime >= CooldownTime)
        {
            ElapsedTime = 0f;
            image.color = CooldownFinishedColor;
            OnCooldown = false;
        }

        transform.position = Input.mousePosition + (Vector3)offset;
    }

    public void ShowUI(float cooldownTime)
    {
        gameObject.SetActive(true);
        CooldownTime = cooldownTime;
    }

    public void StartCooldown()
    {
        ElapsedTime = 0f;
        OnCooldown = true;
    }
}
