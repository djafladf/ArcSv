using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Himo_Creature : MonoBehaviour
{
    [SerializeField] SpriteRenderer spr;
    [SerializeField] Himo main;

    [SerializeField] Material normal;
    [SerializeField] Material Shatter;
    private void Awake()
    {
        spr.material = new Material(spr.material); Shatter = spr.material;

        Texture2D tex = spr.sprite.texture;
        Rect rect = spr.sprite.textureRect;
        float w = 1f / tex.width, h = 1f / tex.height;
        float mx = rect.x * w;
        float my = rect.y * h;
        float sx = rect.width * w;
        float sy = rect.height * h;

        Shatter.SetFloat("_IsUp", 0);
        Shatter.SetVector("_SpriteUVMin", new Vector4(mx, my, 0, 0));
        Shatter.SetVector("_SpriteUVSize", new Vector4(sx, sy, 0, 0));
        Shatter.SetVector("_Start", new Vector4(1, 0));
        Shatter.SetVector("_End", new Vector4(1, 1));

        Ten[0].material = new Material(Ten[0].material);
        Ten[1].material = new Material(Ten[1].material);
        Victim.material = new Material(Victim.material);
    }
    [SerializeField] SpriteRenderer Victim;
    [SerializeField] SpriteRenderer[] Ten;

    public void SetTex(Sprite _spr)
    {
        Victim.sprite = _spr;

        Texture2D tex = _spr.texture;
        Rect rect = _spr.textureRect;

        float w = 1f / tex.width, h = 1f / tex.height;
        float mx = rect.x * w;
        float my = rect.y * h;
        float sx = rect.width * w;
        float sy = rect.height * h;

        Victim.material.SetVector("_SpriteUVMin",new Vector4(mx,my,0,0));
        Victim.material.SetVector("_SpriteUVSize",new Vector4(sx,sy,0,0));
        Victim.material.SetVector("_Start", new Vector2(0,0));
        Victim.material.SetVector("_End", new Vector2(1,0));
        gameObject.SetActive(true);
    }

    void TenS(int tp)
    {
        StopAllCoroutines();
        StartCoroutine(TenSub(tp == 0));
    }
    IEnumerator TenSub(bool tp)
    {
        main.MakeRT(transform.position);
        
        
        if (tp)
        {
            for (int i = 9; i >= 0; i--)
            {
                Ten[0].material.SetVector("_Start", new Vector2(0, i * 0.1f));
                Ten[0].material.SetVector("_End", new Vector2(1,i * 0.1f));
                Ten[1].material.SetVector("_Start", new Vector2(0, i * 0.1f));
                Ten[1].material.SetVector("_End", new Vector2(1, i * 0.1f));
                yield return GameManager.DotHalf;
            }
        }
        else
        {
            //Victim.material.SetFloat("_IsUp", 1);
            for (int i = 0; i < 10; i++)
            {
                Ten[0].material.SetVector("_Start", new Vector2(0, i * 0.1f));
                Ten[0].material.SetVector("_End", new Vector2(1, i * 0.1f));
                Ten[1].material.SetVector("_Start", new Vector2(0, i * 0.1f));
                Ten[1].material.SetVector("_End", new Vector2(1, i * 0.1f));
                Victim.material.SetVector("_Start", new Vector2(0, i * 0.1f));
                Victim.material.SetVector("_End", new Vector2(1, i * 0.1f));
                yield return GameManager.DotOneSec;
            }
        }
    }

    float gap = Mathf.PI * 0.05f;
    void Emerge()
    {
        StopAllCoroutines();
        StartCoroutine(EmergeSub());
    }
    IEnumerator EmergeSub()
    {
        main.MakeRT(transform.position);
        float angle = Mathf.PI * 0.5f;
        for (int i = 0; i < 10; i++)
        {
            spr.material.SetVector("_End", new Vector4(1f + Mathf.Cos(angle), Mathf.Sin(angle)));
            yield return GameManager.DotOneSec;
            angle += gap;
        }
        spr.material = normal;
    }

    private void OnEnable()
    {
        spr.material = Shatter;
    }
}
