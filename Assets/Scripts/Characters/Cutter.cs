using UnityEngine;

public class Cutter : PlayerSetting
{
    [SerializeField] Sprite NormalAttack;
    [SerializeField] Sprite Bullet;
    [SerializeField] ParticleSystem pt;
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

            if (player.WeaponLevel >= 7) MakeSpec = true;
            if (ProjNum != 0)
            {
                KnifeInfo.Damage = (int)(DamageSub * SpecialRatio * 10);
                Vector2 Sub = (TargetPos.position - transform.position).normalized;
                float rad = Vector2.Angle(Vector2.right, Sub) * Mathf.Deg2Rad;
                if (Sub.y < 0) rad = Mathf.PI * 2 - rad;
                for (int i = -ProjNum; i <= ProjNum; i++)
                {
                    GameManager.instance.BM.MakeBullet(
                        KnifeInfo, 0,
                    transform.position, new Vector3(Mathf.Cos(rad + 0.1f * i), Mathf.Sin(rad + 0.1f * i), 0),
                    15, false, Bullet);
                }
            }

        }
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
    int ProjNum = 0;

    bool MakeSpec = false;
    protected override int WeaponLevelUp()
    {
        switch (player.WeaponLevel++)
        {
            case 1: DamageRatio += 0.5f; break;
            case 2: DamageRatio += 0.75f; break;
            case 3: ProjNum++; AttackRange = 5; break;
            case 4: DamageRatio += 0.5f; break;
            case 5: DamageRatio += 0.75f; break;
            case 6: SpecialRatio = 3f; ProjNum++; MakeSpec = true; break;
        }
        return player.WeaponLevel;
    }

    protected override void AttackEnd()
    {
        if (MakeSpec) player.anim.SetTrigger("Spec");
        else base.AttackEnd();
    }

    /*IEnumerator Special()
    {
        CanMove = false;
        player.anim.SetBool("IsWalk", true);
        player.anim.SetBool("IsAttack", false);
        for (int i = 0; i < 1000; i++)
        {
            transform.Rotate(Vector3.up * 7.2f);
            float rad = Random.Range(-3.14f, 3.14f);
            GameManager.instance.BM.MakeBullet((int)(GameManager.instance.PlayerStatus.attack * DamageRatio), 0, 1,
                transform.position, new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0),
                15, Bullet, false, false);
            yield return new WaitForSeconds(0.01f);
        }
        CanMove = true;
        yield return new WaitForSeconds(7);
        StartCoroutine(Special());
    }*/
}

