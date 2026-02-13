using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoomBird : Enemy
{
    [SerializeField] Sprite Boom;
    protected override void AttackMethod()
    {
        BI.Damage = Mathf.FloorToInt(Damage * (1 + GameManager.instance .EnemyStatus.attack - DeBuffVar[1]));
        GameManager.instance.BM.MakeBoom(BI,BI,transform.position, (AttackPos - transform.position).normalized,20,Bull,Boom,true,BL:BL);
        transform.GetChild(0).gameObject.SetActive(false); BeginAttack = false;
        anim.SetBool("IsAttack", false);
        speed = 3;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        transform.GetChild(0).gameObject.SetActive(true);
    }
}
