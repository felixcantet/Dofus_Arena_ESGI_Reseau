using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextEffect : MonoBehaviour
{
    public TextMeshPro text;
    
    public Color displayColor = new Color(1.0f, 0.8156204f, 0.0f, 0.0f);
    public float displayTime = 2.0f;
    public float upSpeed = 3.0f;

    private bool reverseAlpha = false;
    private float currentDisplay = 0.0f;
    
    private void OnEnable()
    {
        displayColor.a = 0.0f;
        text.color = displayColor;
    }

    private void Update()
    {
        this.transform.position += upSpeed * Time.deltaTime * Vector3.up;

        if (!reverseAlpha)
        {
            currentDisplay += Time.deltaTime / (displayTime * 0.5f);
            
            text.color = new Color(displayColor.r, displayColor.g, displayColor.b, currentDisplay);

            if (currentDisplay >= 1.0f)
                reverseAlpha = true;
        }
        else
        {
            currentDisplay -= Time.deltaTime / (displayTime * 0.5f);
            
            text.color = new Color(displayColor.r, displayColor.g, displayColor.b, currentDisplay);
            
            if (currentDisplay <= 0.0f)
                Destroy(this.gameObject);
        }
        
    }
}
