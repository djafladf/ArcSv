using System;
using System.Collections;
using System.Linq.Expressions;
using UnityEngine;

public class Kazemaru : PlayerSetting
{
    [SerializeField] Sprite MeeleAttack;
    [SerializeField] Sprite bullet;
    [SerializeField] GameObject DollPref;
    [SerializeField] ParticleSystem Smoke;


    GameObject Doll;
    [HideInInspector] public Action Respawn;
    CapsuleCollider2D Coll;

    protected override void Awake()
    {
        if (!IsSummon)
        {
            Doll = Instantiate(DollPref, transform.parent);
            Doll.GetComponent<Kazemaru>().Respawn = RespawnAct;
            DamageRatio = 1.5f;
            SpecialRatio = 2f;
        }
        else
        {
            DamageRatio = 3f;
            SpecialRatio = 2f;
            ProjNum = 1;
            player.WeaponLevel = 7;
            CanEvade = true; CanAssasin = true;
        }
        Coll = GetComponent<CapsuleCollider2D>();
        base.Awake();

    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (CanAssasin && !OnEvade) Assasin();
    }

    bool CanEvade = false;
    bool OnEvade = false;

    Vector3 EvadeFrom;

    bool CanAssasin = false;

    protected override void AttackEnd()
    {
        base.AttackEnd();
        OnEvade = false;
    }

    protected override void GetDamage(BulletInfo Info, Transform DamageFrom)
    {
        if (CanEvade)
        {
            OnEvade = true;
            player.anim.SetTrigger("Evade");
            StartCoroutine(Evade());
            EvadeFrom = DamageFrom.position;
            CanMove = false;
            return;
        }
        base.GetDamage(Info, DamageFrom);
    }

    void MakeSmoke()
    {
        Smoke.transform.position = transform.position;
        Smoke.Play();
    }

    void Assasin()
    {
        var j = GameManager.GetNearest(10, 5, transform.position, targetLayer);
        if (j.Count != 0)
        {
            player.anim.SetTrigger("Spec");
            CanAssasin = false;
            EvadeFrom = j[j.Count - 1].position;
            CanMove = false;
            IsAssasin = true;
            StartCoroutine(AssasinCool());
        }
    }

    void AssasinOne()
    {
        player.rigid.MovePosition(EvadeFrom);
    }

    void EvadeOne()
    {
        if (EvadeFrom.x > transform.position.x)
        {
            player.sprite.flipX = true;
            Vector2 nextVec = new Vector2(-5, 0);
            player.rigid.MovePosition(player.rigid.position + nextVec);
        }
        else
        {
            player.sprite.flipX = false;
            Vector2 nextVec = new Vector2(5, 0);
            player.rigid.MovePosition(player.rigid.position + nextVec);
        }
    }

    void EvadeAttack()
    {
        Vector3 Attackfrom = transform.position + Vector3.up * 3;
        Vector2 Sub = (EvadeFrom - Attackfrom).normalized;
        float rad = Vector2.Angle(Vector2.right, Sub) * Mathf.Deg2Rad;
        if (Sub.y < 0) rad = Mathf.PI * 2 - rad;
        for (int i = -2; i <= 2; i++)
        {
            GameManager.instance.BM.MakeBullet(
                new BulletInfo((int)((1 + GameManager.instance.PlayerStatus.attack + player.AttackRatio + player.ReinforceAmount[0]) * SpecialRatio * 10), false, 0), 0,
            Attackfrom, new Vector3(Mathf.Cos(rad + 0.05f * i), Mathf.Sin(rad + 0.05f * i), 0),
            15, false, bullet, delay: 0.1f);
        }
    }

    IEnumerator Evade()
    {
        CanEvade = false;
        yield return new WaitForSeconds(10f);
        CanEvade = true;
    }

    IEnumerator AssasinCool()
    {
        CanAssasin = false;
        yield return new WaitForSeconds(10f);
        CanAssasin = true;
    }

    bool IsAssasin = false;
    protected override void AttackMethod()
    {
        float DamageSub = (1 + GameManager.instance.PlayerStatus.attack + player.AttackRatio + player.ReinforceAmount[0]);
        if (IsAssasin)
        {
            GameManager.instance.BM.MakeMeele(
            new BulletInfo((int)(DamageSub * DamageRatio * 20), false, 0, isFix: true), 0.3f,
            transform.position, -player.Dir, 0, false, MeeleAttack);
            IsAssasin = false;
        }
        else
        {
            GameManager.instance.BM.MakeMeele(
            new BulletInfo((int)(DamageSub * DamageRatio * 10), false, 0), 0.3f,
            transform.position, -player.Dir, 0, false, MeeleAttack);
            if (ProjNum != 0)
            {
                Vector2 Sub = (TargetPos.position - transform.position).normalized;
                float rad = Vector2.Angle(Vector2.right, Sub) * Mathf.Deg2Rad;
                if (Sub.y < 0) rad = Mathf.PI * 2 - rad;
                for (int i = -ProjNum; i <= ProjNum; i++)
                {
                    GameManager.instance.BM.MakeBullet(
                        new BulletInfo((int)(DamageSub * SpecialRatio * 10), false, 0), 0,
                    transform.position, new Vector3(Mathf.Cos(rad + 0.1f * i), Mathf.Sin(rad + 0.1f * i), 0),
                    15, false, bullet);
                }
            }
        }
    }

    void RespawnAct()
    {
        try
        {
            StartCoroutine(RespawnKaze());
        }
        catch
        {

        }
    }

    IEnumerator RespawnKaze()
    {
        yield return new WaitForSeconds(30);
        Doll.SetActive(true); Doll.transform.position = transform.position + Vector3.right;
    }

    float DamageRatio;
    float SpecialRatio;
    int ProjNum;
    protected override int WeaponLevelUp()
    {
        switch (player.WeaponLevel++)
        {
            case 1: DamageRatio += 0.5f; break;
            case 2: ProjNum = 1; break;
            case 3: CanEvade = true; break;
            case 4: DamageRatio += 1f; break;
            case 5: CanAssasin = true; break;
            case 6: if (gameObject.activeSelf) { Doll.SetActive(true); Doll.transform.position = transform.position + Vector3.right; } break;
        }
        return player.WeaponLevel;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        CanEvade = false;
        OnEvade = false;
        
        CanAssasin = false;
        IsAssasin = false;
        
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Area") && Coll.enabled)
        {
            transform.position = GameManager.instance.player.Self.transform.position * 1.5f - transform.position * 0.5f;
        }
    }

    protected override void EndBatch()
    {
        base.EndBatch();
        if (!IsSummon && player.WeaponLevel == 7) { Doll.SetActive(true); Doll.transform.position = transform.position + Vector3.right; }
        CanEvade = player.WeaponLevel >= 3; CanAssasin = player.WeaponLevel >= 5;
    }

    private void OnDisable()
    {
        player.sprite.color = Color.white;
        Coll.enabled = true;
        if (!IsSummon) Doll.SetActive(false);
        else if (Respawn != null) Respawn();
    }
}
