using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MTest : MonoBehaviour
{
    //Camera cam;
    //Texture2D tex;
    //WaitForEndOfFrame wfe;
    //Rect size;
    [SerializeField]
    RenderTexture tex;
    [SerializeField]
    SpriteRenderer spr;
    private void Awake()
    {
        spr.material.SetTexture("_RTTex", tex);
        /*cam = GetComponent<Camera>();
        int w = cam.targetTexture.width;
        int h = cam.targetTexture.height;
        tex = new Texture2D(w, h, TextureFormat.ARGB32, mipChain: false, linear: false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        size = new Rect(0, 0, w, h);
        wfe = new WaitForEndOfFrame();

        spr.sprite = Sprite.Create(
                tex,
                new Rect(0, 0, w, h),
                new Vector2(0.5f, 0.5f),
                pixelsPerUnit: w,
                extrude: 0,
                meshType: SpriteMeshType.FullRect
            );*/
    }
 /*   private void OnEnable()
    {
        StartCoroutine(CamRender());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    RenderTexture prev;
    IEnumerator CamRender()
    {
        while (true)
        {
            yield return wfe;
            prev = RenderTexture.active;
            RenderTexture.active = cam.targetTexture;
            cam.Render();
            tex.ReadPixels(size,0,0,false);
            tex.Apply();
            tex.Apply(updateMipmaps: false, makeNoLongerReadable: false);

            RenderTexture.active = prev;
        }
    }*/
}
