using System;
using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    [HideInInspector] public List<Action<int>> Vamp = new List<Action<int>>();

    [SerializeField] GameObject Bullet;
    [SerializeField] GameObject Warning;
    [SerializeField] LayerMask PlMask;
    [SerializeField] LayerMask EmMask;

    GameObject[] Bullets;
    GameObject[] Warnings;
    WarningBullet[] WarningScripts;
    Bullet[] BulletScripts;
    BulletInfo[] BulletInfos;

    private void Awake()
    {
        GameManager.instance.BM = this;
        GameManager.instance.StartLoading();
    }

    int BullNum = 400;
    int LastUseBull = 0;
    public void Init()
    {
        Bullets = new GameObject[BullNum + 20];
        BulletScripts = new Bullet[BullNum + 20];
        BulletInfos = new BulletInfo[BullNum + 20];
        Warnings = new GameObject[30];
        WarningScripts = new WarningBullet[30];
        for (int i = 0; i < 30; i++)
        {
            Warnings[i] = Instantiate(Warning, transform);
            Warnings[i].SetActive(false);
            WarningScripts[i] = Warnings[i].GetComponent<WarningBullet>();
        }

        for (int i = 0; i < BullNum; i++)
        {
            Bullets[i] = Instantiate(Bullet, transform);
            Bullets[i].gameObject.SetActive(false);
            Bullets[i].name = $"{i}";
            BulletScripts[i] = Bullets[i].GetComponent<Bullet>();
            BulletScripts[i].MyInd = i;
            BulletInfos[i] = new BulletInfo(0, false, 0);
        }
        GameManager.instance.StartLoading();
    }

    DeBuff NoDeBuff = new DeBuff();
    Buff NoBuff = new Buff();

    public void MakeWarning(Vector3 Pos, float time, Vector2 size, Color S, System.Action<Vector3> act)
    {
        for (int i = 0; i < 30; i++)
        {
            if (!Warnings[i].activeSelf)
            {
                Warnings[i].SetActive(true);
                Warnings[i].transform.position = Pos;
                WarningScripts[i].Init(time, size, act, S);
                break;
            }
        }
    }

    /// <summary>
    /// 원거리
    /// </summary>
    /// <param name="Damage"> Dmg </param>
    /// <param name="Penetrate"> Penetration </param>
    /// <param name="Power"> KnockBack Power </param>
    /// <param name="Start"> Start Pos </param>
    /// <param name="Dir"> Moving Dir(normaize) </param>
    /// <param name="speed"> speed </param>
    /// <param name="im"> Image Of Bullet </param>
    /// <param name="IsEnemy"> Is Enemy </param>
    /// <param name="debuffInfo"> About DeBuff </param>
    public void MakeBullet(BulletInfo Info, int Penetrate, Vector3 Start, Vector3 Dir, float speed, bool IsEnemy,
        Sprite im = null, DeBuff debuffInfo = null, BulletLine BL = null, RuntimeAnimatorController anim = null, float delay = 0)
    {
        /*for (int i = 0; i < BullNum; i++)
        {
            if (!Bullets[i].activeSelf)
            {
                Bullets[i].SetActive(true);
                Bullets[i].transform.position = Start; Dir.z = 0;
                Bullets[i].transform.rotation = Quaternion.FromToRotation(Vector3.up, Dir);
                if (Info.SpeedFactor != 0) speed *= Info.SpeedFactor;
                BulletScripts[i].Init_Attack(Penetrate, Dir * speed, false, IsEnemy, 0, Info.ScaleFactor, im, BL, anim, delay: delay, order: Info.LayerOrder);
                BulletInfos[i].Copy(Info);
                break;
            }
        }*/
        int Fnum = 0;
        while (Fnum++ < BullNum)
        {
            LastUseBull++; if (LastUseBull >= BullNum) LastUseBull = 0;
            if (!Bullets[LastUseBull].activeSelf)
            {
                Bullets[LastUseBull].SetActive(true);
                Bullets[LastUseBull].transform.position = Start; Dir.z = 0;
                Bullets[LastUseBull].transform.rotation = Quaternion.FromToRotation(Vector3.up, Dir);
                if (Info.SpeedFactor != 0) speed *= Info.SpeedFactor;
                BulletScripts[LastUseBull].Init_Attack(Penetrate, Dir * speed, IsEnemy, 0, Info.ScaleFactor, im, BL, anim, delay: delay, order: Info.LayerOrder);
                BulletInfos[LastUseBull].Copy(Info);
                break;
            }
        }
    }

    Collider2D[] hits = new Collider2D[100];
    int res = 0,mi = 0;
    /// <summary>
    /// 근거리(보스류랑 플레이어만 처리)
    /// </summary>
    /// <param name="Info"> Info </param>
    /// <param name="AfterTime"> Last Time </param>
    /// <param name="Start"> Start Pos </param>
    /// <param name="Dir"> Moving Dir(norm) </param>
    /// <param name="im"> Image Of Bullet(none = null) </param>
    /// <param name="IsEnemy"> Is Enemy </param>
    /// <param name="debuffInfo"> About DeBuff </param>
    public void MakeMeele(BulletInfo Info, float AfterTime, Vector3 Start, Vector3 Dir, float speed, bool IsEnemy, Sprite im = null, RuntimeAnimatorController Anim = null, DeBuff debuffInfo = null, float delay = 0)
    {
        if(im != null) MakeEffect(AfterTime, Start, Dir, speed, im,Anim:Anim);
        res = Physics2D.OverlapBoxNonAlloc(Start, (im == null ? Vector3.one : im.bounds.size) * Info.ScaleFactor, Vector2.SignedAngle(Vector2.up, Dir), hits,IsEnemy ? PlMask : EmMask);
        if (!IsEnemy) for (mi = 0; mi < res; mi++) GameManager.instance.ES.InstanceTo[hits[mi].gameObject].OnDamage(inf: Info);
        else for (mi = 0; mi < res; mi++) GameManager.instance.GetScript(hits[mi].gameObject).GetDamage(Info,DamageFrom:Start);
    }
    /*int Fnum = 0;
while (Fnum++ < BullNum)
{
    LastUseBull++; if (LastUseBull >= BullNum) LastUseBull = 0;
    if (!Bullets[LastUseBull].activeSelf)
    {
        Bullets[LastUseBull].SetActive(true);
        Bullets[LastUseBull].transform.position = Start; Dir.z = 0;
        Bullets[LastUseBull].transform.rotation = Quaternion.FromToRotation(Vector3.up, Dir);
        if (Info.SpeedFactor != 0) speed *= Info.SpeedFactor;
        BulletScripts[LastUseBull].Init_Attack(-1, Dir * speed, true, IsEnemy, AfterTime, Info.ScaleFactor, im, Anim: Anim, delay: delay, order: Info.LayerOrder);
        BulletInfos[LastUseBull].Copy(Info);
        break;
    }
}*/

    public void MakeBoom(BulletInfo Info, BulletInfo After, Vector3 Start, Vector3 Dir, float Speed, Sprite im, Sprite HitIm, bool IsEnemy, DeBuff debuffInfo = null, BulletLine BL = null, float delay = 0)
    {
        int Fnum = 0;
        while (Fnum++ < BullNum)
        {
            LastUseBull++; if (LastUseBull >= BullNum) LastUseBull = 0;
            if (!Bullets[LastUseBull].activeSelf)
            {
                Bullets[LastUseBull].SetActive(true);
                Bullets[LastUseBull].transform.position = Start; Dir.z = 0;
                Bullets[LastUseBull].transform.rotation = Quaternion.FromToRotation(Vector3.up, Dir);
                BulletScripts[LastUseBull].Init_Explode(After, Dir * Speed, IsEnemy, im, HitIm, BL: BL, delay: delay);
                BulletInfos[LastUseBull].Copy(Info);
                break;
            }
        }
        /*for (int i = 0; i < BullNum; i++)
        {
            if (!Bullets[i].activeSelf)
            {
                Bullets[i].SetActive(true);
                Bullets[i].transform.position = Start; Dir.z = 0;
                Bullets[i].transform.rotation = Quaternion.FromToRotation(Vector3.up, Dir);
                BulletInfos[i].Copy(Info);
                BulletScripts[i].Init_Explode(After, Dir * Speed, IsEnemy, im, HitIm, BL: BL, delay: delay);
                break;
            }
        }*/
    }


    public void MakeEffect(float AfterTime, Vector3 Start, Vector3 Dir, float speed, Sprite im, bool AlphaChange = true, BulletLine BL = null, RuntimeAnimatorController Anim = null)
    {
        int Fnum = 0;
        while (Fnum++ < BullNum)
        {
            LastUseBull++; if (LastUseBull >= BullNum) LastUseBull = 0;
            if (!Bullets[LastUseBull].activeSelf)
            {
                Bullets[LastUseBull].SetActive(true);
                Bullets[LastUseBull].transform.position = Start; Dir.z = 0;
                Bullets[LastUseBull].transform.rotation = Quaternion.FromToRotation(Vector3.up, Dir);
                BulletScripts[LastUseBull].Init_Effect(AfterTime, im, Dir * speed, AlphaChange, BL, Anim);
                break;
            }
        }
        /*for (int i = 0; i < BullNum; i++)
        {
            if (!Bullets[i].activeSelf)
            {
                Bullets[i].SetActive(true);
                Bullets[i].transform.position = Start; Dir.z = 0;
                Bullets[i].transform.rotation = Quaternion.FromToRotation(Vector3.up, Dir);
                BulletScripts[i].Init_Effect(AfterTime, im, Dir * speed, AlphaChange, BL, Anim);
                break;
            }
        }*/
    }

    Quaternion ZeroQuat = new Quaternion(0, 0, 0, 0);
/*    public void MakeBuff(BulletInfo BI, Vector3 Start, Sprite im, bool IsEnemy, bool IsField = false)
    {
        
        int Fnum = 0;
        while (Fnum++ < BullNum)
        {
            LastUseBull++; if (LastUseBull >= BullNum) LastUseBull = 0;
            if (!Bullets[LastUseBull].activeSelf)
            {
                Bullets[LastUseBull].SetActive(true);
                Bullets[LastUseBull].transform.position = Start;
                Bullets[LastUseBull].transform.rotation = ZeroQuat;
                BulletScripts[LastUseBull].Init_Buff(BI.ScaleFactor, im, IsEnemy, IsField);
                BulletInfos[LastUseBull].Copy(BI);
                break;
            }
        }
    }*/

    public BulletInfo GetBulletInfo(int ind)
    {
        return BulletInfos[ind];
    }


    int ExBulletNum = 0;
    public GameObject MakeExternalBullet(bool AllowControl = false)
    {
        if (ExBulletNum + BullNum >= Bullets.Length) return null;
        int i = BullNum + ExBulletNum++;
        Bullets[i] = Instantiate(Bullet, transform);
        Bullets[i].gameObject.SetActive(false);
        Bullets[i].name = $"{i}";
        BulletScripts[i] = Bullets[i].GetComponent<Bullet>();
        BulletInfos[i] = new BulletInfo(0, false, 0);

        return Bullets[i];
    }

    public int RegistExBullet(BulletInfo Info)
    {
        if (ExBulletNum + BullNum >= Bullets.Length) return -1;
        BulletInfos[ExBulletNum + BullNum] = Info;

        return BullNum + ExBulletNum++;
    }
}
