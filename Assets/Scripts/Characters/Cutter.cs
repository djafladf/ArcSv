using UnityEngine;

public class Cutter : PlayerSetting
{
    [SerializeField] Sprite NormalAttack;
    [SerializeField] Sprite EmtyAttack;
    [SerializeField] Sprite Bullet;
    
    [SerializeField] ParticleMy PM;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        for (int i = 0; i < PM.StartSize.Count; i++)
        {
            PM.StartSize[i] = Random.Range(8, 12);
            PM.StartRotations[i] = Quaternion.Euler(0, 0, -i * 60);
        }
        NormalInfo.IgnoreDefense = 0.2f;
        KnifeInfo = new BulletInfo(0, false, 0, ignoreDefense: 0.2f, dealFrom: NormalInfo.DealFrom);
    }

    BulletInfo KnifeInfo;
    protected override void AttackMethod()
    {
        if (TargetPos != null)
        {
            float DamageSub = (1 + GameManager.instance.PlayerStatus.attack + player.AttackRatio + player.ReinforceAmount[0]);
            NormalInfo.Damage = (int)(DamageSub * DamageRatio * 10);
            GameManager.instance.BM.MakeMeele(NormalInfo, 0.3f, transform.position, -player.Dir, 0, false, NormalAttack);
            GameManager.instance.BM.MakeMeele(NormalInfo, 0.3f, transform.position, Vector3.zero, 0, false, EmtyAttack, delay: 0.15f);
            if (player.WeaponLevel >= 7) MakeSpec = true;
        }
    }

    void RangeAttack()
    {
        float DamageSub = (1 + GameManager.instance.PlayerStatus.attack + player.AttackRatio + player.ReinforceAmount[0]);
        KnifeInfo.Damage = (int)(DamageSub * SpecialRatio * 10);
        Vector2 Sub = (TargetPos.position - transform.position).normalized;
        float rad = Vector2.Angle(Vector2.right, Sub) * Mathf.Deg2Rad;
        if (Sub.y < 0) rad = Mathf.PI * 2 - rad;
        for (int i = -ProjNum; i <= ProjNum; i+=2)
        {
            GameManager.instance.BM.MakeBullet(
                KnifeInfo, 0,
            transform.position, new Vector3(Mathf.Cos(rad + 0.1f * i), Mathf.Sin(rad + 0.1f * i), 0),
            15, false, Bullet);
        }
    }

    protected override void Attack()
    {
        if (Vector3.Distance(transform.position, TargetPos.position) > 3)
        {
            player.anim.SetTrigger("Range"); CanMove = false;
        }
        else base.Attack();
    }


    void MakeSpecialAttack()
    {
        PM.StartMaking();
        MakeSpec = false;
    }

    protected override void EndBatch()
    {
        base.EndBatch();
    }

    float DamageRatio = 1f;
    float SpecialRatio = 2f;
    int ProjNum = 1;

    bool MakeSpec = false;
    protected override int WeaponLevelUp(bool IsUp = true)
    {
        switch (player.WeaponLevel++)
        {
            case 1: DamageRatio += 0.5f; break;
            case 2: DamageRatio += 0.75f; break;
            case 3: ProjNum++;  break;
            case 4: DamageRatio += 0.5f; break;
            case 5: DamageRatio += 0.75f; break;
            case 6: SpecialRatio = 3f; ProjNum++; MakeSpec = true; break;
        }
        return player.WeaponLevel;
    }

    protected override void AttackEnd()
    {
        if (MakeSpec) player.anim.SetTrigger("Spec");
        else
        {
            base.AttackEnd();
            if (!CanMove)
            {
                float dist = Vector3.Distance(transform.position, TargetPos.position);
                if(dist > 3) player.anim.SetTrigger("Range");
            }
        }
    }
}

