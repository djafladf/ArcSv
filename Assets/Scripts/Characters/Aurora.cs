using System.Collections;
using System.Security.Cryptography;
using UnityEngine;

public class Aurora : PlayerSetting
{
    [SerializeField] GameObject IceField;

    bool IceOn = false;

    bool IsHeal = false;

    float s = 0;

    protected override void Awake()
    {
        player.InitDefense = 50;
        base.Awake();
    }


    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (IsHeal)
        {
            s += Time.fixedDeltaTime;
            if (s >= 1)
            {
                s = 0;
                int amount = (int)(player.MaxHP * (1 + GameManager.instance.PlayerStatus.heal) * 0.01f);
                GameManager.instance.UM.DamageUp(1, NormalInfo.DealFrom,amount);
                Heal(amount);
            }
        }
    }

    protected override void AttackMethod()
    {
        NormalInfo.Damage = (int)((1 + GameManager.instance.PlayerStatus.attack + player.AttackRatio + player.ReinforceAmount[0]) * 10);
        GameManager.instance.BM.MakeMeele(
            NormalInfo, 0.2f, TargetPos.position, Vector3.zero, 0, false,null);
    }

    protected override int WeaponLevelUp(bool IsUp = true)
    {
        switch (player.WeaponLevel++)
        {
            case 1: player.InitDefense += 5; break;
            case 2: player.InitDefense += 5; break;
            case 3: NormalInfo.DeBuffs = new DeBuff(ice:5); break;
            case 4: player.InitDefense += 10; break;
            case 5: IsHeal = true; break;
            case 6: IceField.SetActive(true); IceOn = true; /*StartCoroutine(FieldAct());*/ break;
        }
        return player.WeaponLevel;
    }

    /*IEnumerator FieldAct()
    {
        while (true)
        {
            RaycastHit2D[] targets = Physics2D.CircleCastAll(transform.position, 3f, Vector2.zero, 0, targetLayer);
            foreach (RaycastHit2D t in targets)
            {
                Transform cnt = t.transform;
                GameManager.instance.BM.MakeMeele(
                    new BulletInfo((int)(player.InitDefense * (1 + player.DefenseRatio + GameManager.instance.PlayerStatus.defense)),false,0,debuffs: new DeBuff(ice: 1)) ,
                    0.2f, cnt.position, Vector3.zero, 0, false,null);
            }
            yield return GameManager.OneSec;
        }
    }*/

    protected override void EndBatch()
    {
        base.EndBatch();
        if (IceOn)
        {
            IceField.gameObject.SetActive(true);
            /*StartCoroutine(FieldAct());*/
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        IceField.gameObject.SetActive(false);
    }
}

