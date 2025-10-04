using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FEater : PlayerSetting
{
    [SerializeField] Sprite Spec;
    [SerializeField] ParticleSystem HitEffect;
    [SerializeField] ParticleSystem SteamEffect;
    [SerializeField] AfterImMaker AIM;
    int Power = 5;

    float ScaleF = 2;
    void MakeIm()
    {
        AIM.StartMaking();
    }

    void StopIm()
    {
        AIM.StopMaking();
    }

    protected override void Start()
    {
        base.Start();
        NormalInfo.ScaleFactor = ScaleF; NormalInfo.KnockBack = Power;
    }

    protected override void AttackMethod()
    {
        Vector3 Gap = (transform.position - TargetPos.position).normalized * 0.2f;
        HitEffect.gameObject.transform.position = TargetPos.position;
        HitEffect.Play();
        NormalInfo.Damage = (int)((1 + GameManager.instance.PlayerStatus.attack + player.AttackRatio + player.ReinforceAmount[0]) * DamageRatio * 10);
        GameManager.instance.BM.MakeMeele(
            NormalInfo, 0.2f, TargetPos.position + Gap, -player.Dir, 0, false);
    }

    Vector3 St1 = new Vector3(1.5f, 0.15f, 0);
    Vector3 St2 = new Vector3(-1.5f, 0.15f, 0);
    void MakeSteam()
    {
        SteamEffect.transform.localPosition = player.sprite.flipX ?  St2 : St1;
        SteamEffect.Play();
    }


    public void SpecialAttack()
    {
        Vector3 Gap = (transform.position - TargetPos.position).normalized * 0.2f;
        HitEffect.gameObject.transform.position = TargetPos.position;
        HitEffect.Play();
        NormalInfo.Damage = (int)((1 + GameManager.instance.PlayerStatus.attack + player.AttackRatio + player.ReinforceAmount[0]) * DamageRatio * 15);
        GameManager.instance.BM.MakeMeele(NormalInfo, 0.2f, TargetPos.position + Gap, -player.Dir, 0, false,Spec);
    }

    float DamageRatio = 2f;

    protected override void EndBatch()
    {
        base.EndBatch();
        if(player.WeaponLevel>6) MakeIm();
    }

    protected override int WeaponLevelUp(bool IsUp = true)
    {
        switch (player.WeaponLevel++)
        {
            case 1: DamageRatio += 1f; break;
            case 2: Power+=5;  NormalInfo.KnockBack = 10; break;
            case 3: ScaleF = 3; HitEffect.transform.localScale = Vector3.one; AttackRange = 3; NormalInfo.ScaleFactor = 3; break;
            case 4: Power+=5; NormalInfo.KnockBack = 15; break;
            case 5: DamageRatio += 1.5f; break;
            case 6: player.anim.SetBool("IsSpecial",true); break;
        }
        return player.WeaponLevel;
    }
}
