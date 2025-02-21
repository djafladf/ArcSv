using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MTest : MonoBehaviour
{
    Camera cam;
    Texture2D tex;
    [SerializeField]
    SpriteRenderer spr;
    private void Awake()
    {
        cam = GetComponent<Camera>();
        tex = new Texture2D(cam.targetTexture.width, cam.targetTexture.height,
            TextureFormat.ARGB32,false);
    }
    private void Start()
    {
        StartCoroutine(CamRender());
    }

    IEnumerator CamRender()
    {
        WaitForEndOfFrame WFE = new WaitForEndOfFrame();
        Rect size = new Rect(0,0,cam.targetTexture.width, cam.targetTexture.height);
        while (true)
        {
            yield return WFE;
            RenderTexture.active = cam.targetTexture;
            cam.Render();
            tex.ReadPixels(size,0,0);
            tex.Apply();
            spr.sprite = 
                Sprite.Create(tex,new Rect(0,0,tex.width,tex.height),new Vector2(0.5f,0.5f),tex.width);
        }
    }
}
