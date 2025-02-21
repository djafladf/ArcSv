using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Himo : PlayerSetting
{
    [SerializeField] Sprite AttackIm;
    [SerializeField] ParticleSystem PTs;
    protected override void AttackMethod()
    {
        PTs.Play();
        NormalInfo.Damage = (int)((1 + GameManager.instance.PlayerStatus.attack + player.AttackRatio + player.ReinforceAmount[0]) * 15);
        GameManager.instance.BM.MakeMeele(
            NormalInfo, 0.5f, transform.position, player.sprite.flipX ? new Vector2(-1,-1) : Vector2.up, 0, false, AttackIm);
    }
    void AttackPrepEnd() 
    {
        // 떄리던 놈 근처에 있음
        if (Vector3.Distance(transform.position, TargetPos.position) <= AttackRange) return;

        // 때리던 놈 없어짐 -> 새로 찾음
        TargetPos = GetNearest(AttackRange);
        if (TargetPos != null && !player.IsFollow)
        {
            player.Dir = (TargetPos.position - transform.position).normalized;
            FlipAnim();
            if (CurFollow[0] != -1) GameManager.instance.ES.TargetChange[CurFollow[0]][CurFollow[1]][player.Id] = false;
            CurFollow[0] = TargetPos.name[0] - 1; CurFollow[1] = TargetPos.name[1] - 1; TargetChangeCall = false;
            GameManager.instance.ES.TargetChange[CurFollow[0]][CurFollow[1]][player.Id] = true;
        }
        else
        {
            player.anim.SetBool("IsAttack", false); if (CurFollow[0] != -1) GameManager.instance.ES.TargetChange[CurFollow[0]][CurFollow[1]][player.Id] = false;
             CurFollow[0] = -1; CurFollow[1] = -1; TargetChangeCall = true;
        }
    }

    void AttackAfter()
    {
        CanMove = true;
    }

}
