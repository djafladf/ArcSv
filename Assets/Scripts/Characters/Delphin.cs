using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Delphin : PlayerSetting
{
    [SerializeField] SpriteRenderer ChargeBar;
    [SerializeField] ParticleSystem Effect;
    float Charge = 0;
    float KillCharge = 0.01f;
    float HitCharge = 0.005f;

    [SerializeField] Sprite[] sprs;
    [SerializeField]public SpriteRenderer[] Floats;
    [HideInInspector]public Animator[] anims = new Animator[3];
    [HideInInspector]public LineRenderer[] lines = new LineRenderer[3];
    Vector3[] Pos = new Vector3[3];
    Vector3[] Pos2 = new Vector3[3];
    [SerializeField] MagnetField Mag;
    int FloatCount = 1;

    protected override void Awake()
    {
        base.Awake();
        for (int i = 0; i < 3; i++)
        { 
            lines[i] = Floats[i].transform.GetComponent<LineRenderer>(); 
            anims[i] = Floats[i].transform.GetComponent<Animator>(); 
            Pos[i] = Floats[i].transform.localPosition; Pos2[i] = new Vector3(-Pos[i].x, Pos[i].y);
        }
    }

    protected override void StatChange()
    {
        base.StatChange();
        foreach(var j in anims) j.SetFloat("AttackSpeed", player.AttackSpeed + GameManager.instance.PlayerStatus.attackspeed + player.ReinforceAmount[3]);
    }

    protected override void Start()
    {
        base.Start();
        NormalInfo.DeadTrigger = KillUp;
    }

    int Flipi = 0;
    protected override void Flip_X()
    {
        base.Flip_X();
        for (Flipi = 0; Flipi < FloatCount; Flipi++) Floats[Flipi].transform.localPosition = Pos2[Flipi];
    }

    protected override void Flip_Y()
    {
        base.Flip_Y();
        for (Flipi = 0; Flipi < FloatCount; Flipi++) Floats[Flipi].transform.localPosition = Pos[Flipi];
    }

    float normaldm = 15;
    float dmred = 0.6f;
    int refCount = 2;

    protected override int WeaponLevelUp(bool IsUp = true)
    {
        switch (player.WeaponLevel++)
        {
            case 1: KillCharge = 0.015f; HitCharge = 0.0075f; break;
            case 2: normaldm = 20; break;
            case 3: dmred = 0.5f; break;
            case 4: KillCharge = 0.02f; HitCharge = 0.01f; break;
            case 5: refCount = 3; break;
            case 6: break;
        }
        return player.WeaponLevel;
    }


    public void KillUp(Transform a, int b)
    {
        if (OnMod) return;
        Charging(KillCharge);
    }

    bool OnMod = false;
    void Charging(float value)
    {
        if (OnMod) return;
        Charge = Mathf.Min(1,Charge + value * 3);
        ChargeBar.material.SetFloat("_Process",Charge);

        if(Charge >= 0.33 && FloatCount == 1) 
        {
            if (Floats[1].gameObject.activeSelf) Floats[1].gameObject.SetActive(false);
            FloatCount = 2; Floats[1].gameObject.SetActive(true); Floats[1].transform.localPosition = player.sprite.flipX ? Pos2[1] : Pos[1];
        }
        else if (Charge >= 0.66 && FloatCount == 2)
        {
            if (Floats[2].gameObject.activeSelf) Floats[2].gameObject.SetActive(false);
            FloatCount = 3; Floats[2].gameObject.SetActive(true); Floats[2].transform.localPosition = player.sprite.flipX ? Pos2[2] : Pos[2];
        }
        if (Charge == 1)
        {
            Effect.Play();
            OnMod = true;
            StartCoroutine(ChargingMod());
        }
    }

    WaitForSeconds wfs = new WaitForSeconds(10f);
    IEnumerator ChargingMod()
    {
        for (int i = 0; i < 200; i++)
        {
            yield return GameManager.DotOneSec;
            ChargeBar.material.SetFloat("_Process", 1 - i * 0.005f);
        }
        Effect.Stop();
        FloatCount = 1; 
        Charge = 0;
        if (player.WeaponLevel < 7) { anims[1].SetTrigger("Dead"); anims[2].SetTrigger("Dead"); }
        else { Mag.EndMod(); player.anim.SetTrigger("Spec2"); }
        yield return wfs;
        OnMod = false;
    }

    protected void AttackPre()
    {
        for (int i = 0; i < FloatCount; i++) anims[i].SetTrigger("Attack");
    }

    protected override void AttackEnd()
    {
        if (player.WeaponLevel >= 7 && OnMod)
        {
            foreach (var j in anims) j.SetTrigger("Dead");
            player.anim.SetTrigger("Spec");
            player.anim.SetBool("IsAttack", false);
            CanMove = false;
        }
    }

    void SpecEnd()
    {
        Floats[0].gameObject.SetActive(true); Floats[0].transform.localPosition = player.sprite.flipX ? Pos2[0] : Pos[0]; CanMove = true;
        OnMod = false;
        base.AttackEnd();
    }

    void SpecPre()
    {
        Mag.transform.position = transform.position;
        Mag.SetActive(null);
    }


    public void SpecAttack()
    {
        player.anim.SetTrigger("Spec2");
    }
    
    protected override void AttackMethod()
    {
        base.AttackMethod();
        Dictionary<Transform, float> set = new();
        
        for(int i = 0; i < FloatCount; i++)
        {
            Vector3 CurStart = Floats[i].transform.position;
            List<Transform> Inc = new();
            for (int x = 0; x < refCount; x++)
            {
                var s = GameManager.GetNearest(AttackRange + 3, CurStart, targetLayer,Inc);
                if (s == null) break;
                Inc.Add(s);
                if (set.ContainsKey(s)) set[s] *= 0.5f;
                else set.Add(s, 1);
                CurStart = s.position;
            }
            //Debug.Log(Inc.Count);
            if (Inc.Count == 0) continue;
            lines[i].positionCount = Inc.Count + 1;
            lines[i].SetPosition(0, Floats[i].transform.GetChild(1).position);
            Charging(Inc.Count * HitCharge);
            float startRat = 1 + (refCount - Inc.Count) * 0.1f;
            
            int l = 1;
            foreach (var j in Inc)
            {
                NormalInfo.Damage = (int)((1 + GameManager.instance.PlayerStatus.attack + player.AttackRatio + player.ReinforceAmount[0]) * startRat * normaldm * set[j]);
                lines[i].SetPosition(l++,j.position);
                GameManager.instance.ES.InstanceTo[j.gameObject].OnDamage(inf : NormalInfo);
                startRat *= dmred;
            }
            if (Lighter == null) Lighter = StartCoroutine(LightEffect());
            else { StopCoroutine(Lighter); Lighter = StartCoroutine(LightEffect()); }
        }
    }

    Coroutine Lighter = null;
    Color[] LightAlpha = { new Color(1, 1, 1, 0.66f), new Color(1, 1, 1, 0.33f), new Color(1, 1, 1, 0) };
    IEnumerator LightEffect()
    {
        lines[0].sharedMaterial.SetColor("_BaseColor",Color.white);
        yield return GameManager.DotFiveSec;
        foreach (var j in LightAlpha)
        {
            lines[0].sharedMaterial.SetColor("_BaseColor", j);
            yield return GameManager.DotOneSec;
        }
        foreach (var j in lines) j.positionCount = 0;
    }


    protected override void EndBatch()
    {
        base.EndBatch();
        Floats[0].gameObject.SetActive(true); Floats[0].transform.localPosition = player.sprite.flipX ? Pos2[0] : Pos[0];
        player.WeaponLevel = 7;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        foreach (var j in Floats)j.transform.SetParent(transform);
    }

    protected void OnDisable()
    {
        
        Effect.Stop(); FloatCount = 1;
        Charge = 0; OnMod = false; ChargeBar.material.SetFloat("_Process", 0);
        foreach (var j in Floats) j.gameObject.SetActive(false);
        foreach (var j in lines) j.positionCount = 0;
        Mag.gameObject.SetActive(false);
    }

}
