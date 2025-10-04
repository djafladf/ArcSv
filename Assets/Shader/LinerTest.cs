using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LinerTest : MonoBehaviour
{
    RectTransform Rect;
    Image im;
    private void Awake()
    {
        Rect = GetComponent<RectTransform>();
        im = GetComponent<Image>();
        im.material = new Material(im.material);
    }

    void OnRectTransformDimensionsChange()
    {
        if (Rect == null) return;
        float Ratio = Rect.sizeDelta.y / Rect.sizeDelta.x;
        im.material.SetFloat("_Ratio", Ratio);
    }
}
