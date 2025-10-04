using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileMapSub : MonoBehaviour
{
    [SerializeField] Shader TileMapShad;
    [SerializeField] Texture2D TileSprite;
    [SerializeField] int TileSize;
    [SerializeField] Vector2 SideMargin;

    RectTransform rect;
    Material mat;
    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        mat = new Material(TileMapShad);
        GetComponent<Image>().material = mat;
        mat.SetVector("_TileSize",new Vector4(TileSize,TileSize,0,0));
        mat.SetTexture("_Atlas", TileSprite);
        OnRectTransformDimensionsChange();
    }

    void OnRectTransformDimensionsChange()
    {
        if (mat == null) return;
        mat.SetVector("_RectSize", new Vector4(rect.sizeDelta.x,rect.sizeDelta.y,0,0));
        mat.SetVector("_TileSize", new Vector4(TileSize, TileSize, 0, 0));
        mat.SetVector("_TileNum", new Vector4(Mathf.FloorToInt(rect.sizeDelta.x / TileSize), Mathf.FloorToInt(rect.sizeDelta.y/TileSize),0,0));
    }
}
