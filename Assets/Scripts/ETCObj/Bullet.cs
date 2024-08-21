using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    Rigidbody2D rigid;
    CapsuleCollider2D coll;
    SpriteRenderer sprite;
    Animator anim;
    Sprite HitImage;
    TrailRenderer Line;


    BulletInfo AfterBull;

    int Penetrate;
    
    bool IsMeele;
    bool IsEnem;
    bool IsBoom;

    private void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
        rigid = GetComponent<Rigidbody2D>();
        coll = GetComponent<CapsuleCollider2D>();
        anim = GetComponent<Animator>(); anim.enabled = false;
        Line = GetComponent<TrailRenderer>(); Line.enabled = false;
    }


    public void Init_Attack(int Penetrate, Vector3 Dir,
        bool IsMeele, bool IsEnemy, float AfterTime, float ScaleFactor = 1, Sprite Image = null, 
        BulletLine BL = null, RuntimeAnimatorController Anim = null,Sprite HitImage = null, float delay = 0)
    {
        if (BL != null)
        {
            Line.enabled = true;Line.colorGradient = BL.Color;Line.widthCurve = BL.Width;Line.time = BL.Time;
        }
        if (Anim != null) { anim.enabled = true; anim.runtimeAnimatorController = Anim; }

        this.HitImage = HitImage;
        rigid.simulated = true; rigid.velocity = Dir; this.Penetrate = Penetrate; sprite.sprite = Image; this.IsMeele = IsMeele; this.IsEnem = IsEnemy; IsBoom = false;

        if (delay != 0) StartCoroutine(AttackDelay(delay));
        else coll.enabled = true;

        if (Image != null) coll.size = sprite.bounds.size * ScaleFactor;
        else coll.size = Vector2.one * ScaleFactor;
        
        if (IsMeele) StartCoroutine(AfterImage(AfterTime,delay == 0));
        
        tag = IsEnemy ? "EnemyAttack" : "PlayerAttack";
    }

    IEnumerator AttackDelay(float time)
    {
        yield return new WaitForSeconds(time);
        coll.enabled = true;
    }

    public void Init_Explode(BulletInfo After, Vector3 Dir, bool IsEnemy, Sprite Image, Sprite HitImage,
        BulletLine BL = null, RuntimeAnimatorController Anim = null,float delay = 0)
    {
        if (BL != null)
        {
            Line.enabled = true; Line.colorGradient = BL.Color; Line.widthCurve = BL.Width; Line.time = BL.Time;
        }
        if (Anim != null) { anim.enabled = true; anim.runtimeAnimatorController = Anim; }

        this.HitImage = HitImage; this.AfterBull = After;
        rigid.simulated = true; rigid.velocity = Dir; this.Penetrate = 0; sprite.sprite = Image; this.IsMeele = false; this.IsEnem = IsEnemy; IsBoom = true;
        StartCoroutine(AttackDelay(delay));
        

        if (Image != null) coll.size = sprite.bounds.size * 0.9f;
        else coll.size = Vector2.one;

        tag = IsEnemy ? "EnemyAttack" : "PlayerAttack";
    }


    public void Init_Effect(float AfterTime, Sprite Image,Vector3 Dir, bool AlphaChange = true, BulletLine BL = null,RuntimeAnimatorController Anim = null)
    {
        if (BL != null)
        {
            Line.enabled = true; Line.colorGradient = BL.Color; Line.widthCurve = BL.Width; Line.time = BL.Time;
        }
        if (Anim != null){ anim.enabled = true; anim.runtimeAnimatorController = Anim; }

        rigid.simulated = true; rigid.velocity = Dir; sprite.sprite = Image;
        if (BL == null) Line.enabled = false;

        StartCoroutine(AfterImage(AfterTime,AlphaChange));
    }

    public void Init_Buff(float ScaleFactor, Sprite Im,  bool IsEnemy)
    {
        rigid.simulated = true;
        coll.enabled = true;
        
        sprite.sprite = Im;
        if (ScaleFactor == 0) ScaleFactor = 1;
        if (Im != null) coll.size = sprite.bounds.size * ScaleFactor;
        else coll.size = Vector2.one * ScaleFactor;

        IsMeele = false;
        tag = IsEnemy ? "EnemyBuff" : "PlayerBuff";
    }

    public void OnDisable()
    {
        Line.Clear(); Line.enabled = false;
        anim.enabled = false; rigid.simulated = false; coll.enabled = false;
        sprite.color = Color.white;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") && !IsEnem)
        {
            if (IsMeele) { coll.enabled = false; return; }
            if (HitImage != null)
            {
                if (IsBoom) GameManager.instance.BM.MakeMeele(AfterBull, 0.3f, transform.position, Vector3.zero,0, IsEnem, HitImage);
                else GameManager.instance.BM.MakeEffect(0.3f, transform.position, Vector3.zero,0, HitImage);
            }
            if (Penetrate-- <= 0) { if (Line.enabled) StartCoroutine(ForLine());  else gameObject.SetActive(false);  }
        }
        else if((collision.CompareTag("Player")||collision.CompareTag("Player_Hide")) && IsEnem)
        {
            if (IsMeele) { coll.enabled = false; return; }
            if (HitImage != null)
            {
                if (IsBoom) GameManager.instance.BM.MakeMeele(AfterBull, 0.3f, transform.position, Vector3.zero, 0,IsEnem, HitImage);
                else GameManager.instance.BM.MakeEffect(0.3f, transform.position, Vector3.zero, 0,HitImage);
            }
            if (Penetrate-- <= 0) { if (Line.enabled) StartCoroutine(ForLine()); else gameObject.SetActive(false); }
        }

    }

    IEnumerator AfterImage(float AfterTime,bool SizeChange = false) 
    {
        float j = AfterTime * 0.2f;
        Color D = new Color(0, 0, 0, 0.2f);
        yield return new WaitForSeconds(j);
        for (int i = 0; i < 5; i++)
        {
            yield return new WaitForSeconds(j);
            if (SizeChange) { sprite.color -= D; }
        }
        
        StartCoroutine(ForLine());
    }

    IEnumerator ForLine()
    {
        IsMeele = true;
        coll.enabled = false; rigid.simulated = false; sprite.sprite = null;
        yield return new WaitForSeconds(Line.time);
        gameObject.SetActive(false);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Area") && !IsMeele) { if (Line.enabled) StartCoroutine(ForLine()); else gameObject.SetActive(false); }
    }
}
