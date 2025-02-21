using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine;

public class AttackType
{
    public bool IsMeele;
    public float AttackSpeed;
    public float FirstDelay;
    public float LastDelay;
}


[System.Serializable]
public class ItemSub
{
    public string name;
    public int operatorid;
    public int lv;
    public int rarity;
    public bool Islocked;
    public List<string> description;
    [Multiline(2)]
    public string extra;
    public Sprite sprite;
    public attribute attributes;
    public attribute attribute_Enem;
}

[System.Serializable]
public class attribute
{
    public float attack;
    public float attackspeed;
    public float defense;
    public float speed;
    public float power;
    public float cost;
    public float pickup;
    public int selection;
    public float exp;
    public float heal;
    public float GoodsEarn;
    public int dragons;
    public int special;
    public float hp;
    public float vamp;
    public attribute()
    {
        attack = 0; attackspeed = 0; defense = 0; speed = 0; hp = 0;
    }
}

[System.Serializable]
public class EnemStat
{
    public float attack;
    public float speed;
    public float defense;
    public float spawn;
    public float hp;
    public float boss;
}

[System.Serializable]
public class BulletInfo
{
    public int Damage;
    public int DealFrom;
    public int LayerOrder;
    public bool IsEffect;
    public bool IsFix;
    public float KnockBack;
    public float Vamp;
    public float IgnoreDefense;
    public float ScaleFactor;
    public float SpeedFactor;
    public float ExecuteRatio;

    public Action<Transform, int> DeadTrigger;

    public DeBuff DeBuffs;

    public Buff Buffs;

    public BulletInfo(int damage = 0, bool isEffect = false, float knockBack = 0, float scalefactor = 1,
        float speedfactor = 1, bool isFix = false, float ignoreDefense = 0,
        DeBuff debuffs = null, Buff buffs = null, int dealFrom = -1, int layerOrder = 4, float vamp = 0,
        Action<Transform, int> DT = null, float executeRatio = 0)
    {
        Damage = damage; DealFrom = dealFrom;
        IsEffect = isEffect; IsFix = isFix; ScaleFactor = scalefactor; SpeedFactor = speedfactor;
        KnockBack = knockBack; IgnoreDefense = ignoreDefense;
        DeBuffs = debuffs;
        Buffs = buffs;
        LayerOrder = layerOrder; ExecuteRatio = executeRatio;
        if(GameManager.instance != null) Vamp = vamp + GameManager.instance.PlayerStatus.vamp;
    }

    public int ReturnDamage(float defense)
    {
        if (IsFix) return Damage;

        if (defense < 70) { }
        else if (defense < 80) defense = 80 + (defense - 80) * 0.5f;
        else if (defense < 90) defense = 85 + (defense - 90) * 0.2f;
        
        return Mathf.CeilToInt(Damage * (100 + IgnoreDefense - defense) * 0.01f);
    }

    public void Copy(BulletInfo From)
    {
        Damage = From.Damage; DealFrom = From.DealFrom; IsEffect = From.IsEffect; IsFix = From.IsFix; KnockBack = From.KnockBack; IgnoreDefense = From.IgnoreDefense; ScaleFactor = From.ScaleFactor; SpeedFactor = From.SpeedFactor; DeBuffs = From.DeBuffs; Buffs = From.Buffs; LayerOrder = From.LayerOrder;
        Vamp = From.Vamp; ExecuteRatio = From.ExecuteRatio; DeadTrigger = From.DeadTrigger;
    }
}

[System.Serializable]
public class Buff
{
    public float Last;
    public float Speed;
    public float Attack;
    public float Defense;
    public float AttackSpeed;
    public int Heal;

    public Buff(float last = 0, float speed = 0, float attack = 0, float defense = 0,float attackspeed = 0, int heal = 0)
    {
        Last = last;
        Speed = speed;
        Attack = attack;
        Defense = defense;
        AttackSpeed = attackspeed;
        Heal = heal;
    }
}

[System.Serializable]
public class DeBuff
{
    public float Last;
    public float Speed;
    public float Attack;
    public float Defense;
    public float Ice;
    public float Fragility;
    public bool Stun;

    public DeBuff(float last = 0, float speed = 0, float attack = 0, float defense = 0, float ice = 0, float fragility = 0, bool stun = false)
    {
        Last = last;
        Speed = speed;
        Attack = attack;
        Defense = defense;
        Ice = ice;
        Fragility = fragility;
        Stun = stun;
    }
}


[System.Serializable]
public class BulletLine
{
    public Gradient Color;
    public AnimationCurve Width;
    public float Time;
}

