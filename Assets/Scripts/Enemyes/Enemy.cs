using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] protected int HP;
    [SerializeField] protected float speed;
    [SerializeField] protected int Damage;
    [SerializeField] protected int Defense;
    [SerializeField] protected int Weight;
    [SerializeField] protected int EnemyType;
    protected int MaxDefense;
    protected int MaxDamage;
    protected float MaxSpeed;

    protected Rigidbody2D rigid;


    [NonSerialized] public bool MoveAble = true;
    [NonSerialized] public bool BeginAttack = false;
    [NonSerialized] public Transform Target = null;

    protected int MaxHP;
    protected int[] EnemyInd = { -1, -1 };
    protected bool IsLive = true;
    protected bool OnIce = false;
    protected bool OnStun = false;
    protected bool OnHit = false;

    protected float IceRatio = 1;
    protected bool StunImmun = false;
    protected float Cheeled = 0;

    protected Animator anim;
    protected SpriteRenderer spriteRenderer;
    protected CapsuleCollider2D coll;

    protected virtual void Awake()
    {
        MaxHP = HP; MaxDamage = Damage; MaxDefense = Defense; MaxSpeed = speed;
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        coll = GetComponent<CapsuleCollider2D>();
    }

    protected virtual void Start()
    {
        EnemyInd[0] = name[0] - 1; EnemyInd[1] = name[1] - 1;
    }

    protected virtual void FixedUpdate()
    {
        if (!IsLive || OnIce || OnStun) return;
        if (MoveAble && !anim.GetBool("IsAttack"))
        {
            Vector2 Dir = (GameManager.instance.player.Self.position - transform.position).normalized;
            if (Dir.x > 0 && !spriteRenderer.flipX) spriteRenderer.flipX = true;
            else if (Dir.x < 0 && spriteRenderer.flipX) spriteRenderer.flipX = false;

            if (!OnHit) rigid.MovePosition(rigid.position + Dir * speed * Time.fixedDeltaTime * (1 + GameManager.instance.EnemyStatus.speed - DeBuffVar[0]));
        }
        if (BeginAttack && !anim.GetBool("IsAttack"))
        {
            AttackPos = Target.position;
            MoveAble = false;
            anim.SetBool("IsAttack", true);
        }
    }

    protected virtual void AttackEnd()
    {
        if (!BeginAttack)
        {
            anim.SetBool("IsAttack", false);
            MoveAble = true;
        }
        else AttackPos = Target.position;
    }

    protected Vector3 AttackPos;


    //[SerializeField] protected Sprite Bullet;
    [SerializeField] protected bool IsRange;
    [SerializeField] protected bool MakeLine;
    [SerializeField] protected BulletInfo BI;
    [SerializeField] protected Sprite Bull;
    [SerializeField] protected BulletLine BL;

    protected virtual void AttackMethod()
    {

        if (IsRange)
        {
            BI.Damage = Mathf.FloorToInt(Damage * (1 + GameManager.instance.EnemyStatus.attack - DeBuffVar[1]));
            GameManager.instance.BM.MakeBullet(BI, 0, transform.position, (AttackPos - transform.position).normalized, 8, true, Bull, BL: MakeLine ? BL : null);
        }
        else
        {
            int CurDm = Mathf.FloorToInt(Damage * (1 + GameManager.instance.EnemyStatus.attack - DeBuffVar[1]));
            GameManager.instance.BM.MakeMeele(new BulletInfo(CurDm, false, 0), 0.2f, AttackPos, Vector2.zero, 0, true,im:Bull);
        }
    }

    bool CanHit = true;
    // 속, 공, 방, 피증, 빙, 스턴
    protected float[] LeftTime = { 0,0,0,0,0,0};
    protected float[] DeBuffVar = { 0, 0, 0, 0, 0 };

    // 속, 공, 방, 피증
    protected float[] LeftTimeBuff = { 0, 0, 0, 0};
    protected float[] BuffVar = { 0, 0, 0, 0};

    protected GameObject[] DeBuffObj = new GameObject[5];

    protected virtual void HPChange()
    {
        
    }

    protected virtual IEnumerator DeadLater()
    {
        yield return GameManager.DotOneSec;
        tag = "Untagged";
        StopAllCoroutines();
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsLive) return;
        if (collision.CompareTag("PlayerAttack") && CanHit)
        {
            BulletInfo Info = GameManager.instance.BM.GetBulletInfo(GameManager.StringToInt(collision.name));
            int GetDamage = Info.ReturnDamage(Defense * (1 + GameManager.instance.EnemyStatus.defense - DeBuffVar[2] + BuffVar[2]));
            if (GetDamage < 0) GetDamage = 0;
            HP -= GetDamage;

            if (MaxHP * Info.ExecuteRatio >= HP)
            {
                HP = 0;
                GameManager.instance.DM.MakeDamage(GetDamage, transform);
                GameManager.instance.UM.DamageUp(0, Info.DealFrom, HP + GetDamage);
            }
            else
            {
                GameManager.instance.DM.MakeDamage(GetDamage, transform);
                GameManager.instance.UM.DamageUp(0, Info.DealFrom, GetDamage);
            }
            if (Info.Vamp > 0) GameManager.instance.BM.Vamp[Info.DealFrom]((int)(GetDamage * Info.Vamp));

            HPChange();     // For Boss
            if (HP <= 0)
            {
                anim.SetTrigger("Dead");
                StartCoroutine(DeadLater());
                if (Info.DeadTrigger != null) Info.DeadTrigger(transform, 0);
                spriteRenderer.sortingOrder = 1; anim.enabled = true;
                IsLive = false; CanHit = false; rigid.simulated = false; coll.enabled = false;
                for (int i = 0; i < 5; i++) if (GameManager.instance.ES.TargetChange[EnemyInd[0]][EnemyInd[1]][i])
                    {
                        GameManager.instance.ES.TargetChangeAct[i](EnemyInd[0], EnemyInd[1]); GameManager.instance.ES.TargetChange[EnemyInd[0]][EnemyInd[1]][i] = false;
                    }
            }
            else if (Info.DeBuffs != null)
            {
                if (Info.DeBuffs.Speed != 0)
                {
                    /*if (DeBuffVar[0] == 0)
                    {
                        DeBuffObj[2] = GameManager.instance.BFM.RequestForDebuff(3);
                        DeBuffObj[2].transform.parent = transform;
                        DeBuffObj[2].transform.localPosition = spriteRenderer.sprite.bounds.size * 0.5f;
                        DeBuffObj[2].gameObject.SetActive(true);
                    }*/
                    if (DeBuffVar[0] == Info.DeBuffs.Speed) LeftTime[0] = Mathf.Max(LeftTime[0], Info.DeBuffs.Last);
                    else if (DeBuffVar[0] < Info.DeBuffs.Speed) { LeftTime[0] = Info.DeBuffs.Last; DeBuffVar[0] = Info.DeBuffs.Speed; }
                }
                if (Info.DeBuffs.Attack != 0)
                {

                }
                if (Info.DeBuffs.Defense != 0 && DeBuffVar[2] <= Info.DeBuffs.Defense)
                {
                    /*if (DeBuffVar[2] == 0)
                    {
                        DeBuffObj[2] = GameManager.instance.BFM.RequestForDebuff(3);
                        DeBuffObj[2].transform.parent = transform;
                        DeBuffObj[2].transform.localPosition = spriteRenderer.sprite.bounds.size * 0.5f;
                        DeBuffObj[2].gameObject.SetActive(true);
                    }*/
                    if (DeBuffVar[2] == Info.DeBuffs.Defense) LeftTime[2] = Mathf.Max(LeftTime[2], Info.DeBuffs.Last);
                    else if (DeBuffVar[2] < Info.DeBuffs.Defense) { LeftTime[2] = Info.DeBuffs.Last; DeBuffVar[2] = Info.DeBuffs.Defense; }
                }
                if (Info.DeBuffs.Ice != 0 && !OnIce && IceRatio > 0)
                {
                    Cheeled += Info.DeBuffs.Ice * IceRatio;
                    if (DeBuffObj[4] == null)
                    {
                        DeBuffObj[4] = GameManager.instance.BFM.RequestForDebuff(0);
                        DeBuffObj[4].transform.parent = transform;
                        DeBuffObj[4].transform.localPosition = new Vector3(0, spriteRenderer.sprite.bounds.size.y * 0.6f, 0);
                        DeBuffObj[4].gameObject.SetActive(true);
                        DeBuffVar[0] += 0.3f;
                    }
                    if (Cheeled >= 10)
                    {
                        DeBuffObj[4].SetActive(false); DeBuffObj[4].transform.parent = GameManager.instance.BFM.transform;
                        DeBuffObj[4] = null; LeftTime[4] = 1;
                        DeBuffObj[4] = GameManager.instance.BFM.RequestForDebuff(1,spriteRenderer.sprite.bounds.size.x,spriteRenderer.bounds.size.y);
                        DeBuffObj[4].transform.parent = transform;
                        DeBuffObj[4].transform.localPosition = Vector3.zero;
                        DeBuffObj[4].gameObject.SetActive(true); DeBuffVar[0] -= 0.3f; Cheeled = 0; anim.speed = 0; OnIce = true;
                    }
                }
                if (Info.DeBuffs.Fragility != 0)
                {

                }
                if (Info.DeBuffs.Stun && !StunImmun)
                {
                    LeftTime[5] = Mathf.Max(Info.DeBuffs.Last, LeftTime[5]); OnStun = true; anim.speed = 0;
                }
            }
            if(GetDamage > 0) StartCoroutine(NockBack_Enemy(Info.KnockBack,transform.position - collision.transform.position));
        }
        else if (collision.CompareTag("EnemyBuff"))
        {
            Buff Info = GameManager.instance.BM.GetBulletInfo(GameManager.StringToInt(collision.name)).Buffs;
            if (Info.Heal != 0) Heal(Info.Heal);
            if (Info.Defense != 0)
            {
                if (BuffVar[2] == Info.Defense) LeftTimeBuff[2] = Mathf.Max(LeftTimeBuff[2], Info.Last);
                else LeftTimeBuff[2] = Info.Last;
                BuffVar[2] = Mathf.Max(BuffVar[2],Info.Defense);
            }
        }
    }

    protected virtual void Heal(float amount)
    {
        HP += (int)amount; HP = Mathf.Min(MaxHP, HP);
    }
    

    IEnumerator BuffCheck()
    {
        int i;

        for (i = 0; i < 4; i++) { DeBuffVar[i] = 0; BuffVar[i] = 0; LeftTimeBuff[i] = 0; LeftTime[i] = 0; }

        for (i = 0; i < DeBuffObj.Length; i++) if (DeBuffObj[i] != null)
            {
                rigid.velocity = Vector2.zero;
                DeBuffObj[i].SetActive(false); DeBuffObj[i].transform.parent = GameManager.instance.BFM.transform; DeBuffObj[i] = null;
            }

        while (true)
        {
            for (i = 0; i < 4; i++)
            {
                // Buff Check
                if (LeftTimeBuff[i] != 0)
                {
                    LeftTimeBuff[i] -= 0.1f;
                    if (LeftTimeBuff[i] <= 0) { LeftTimeBuff[i] = 0; BuffVar[i] = 0; }
                }
            }
            // DeBuff Check
            for (i = 0; i < 6; i++) 
            {
                
                // About stun
                if (i == 5 && LeftTime[5] > 0)
                {
                    LeftTime[5] -= 0.1f;
                    if (LeftTime[5] <= 0)
                    {
                        LeftTime[5] = 0; OnStun = false;
                        if (!OnIce) anim.speed = 1;
                    }
                }
                else if (i == 4 && LeftTime[4] > 0)
                {
                    LeftTime[4] -= 0.1f;
                    if (LeftTime[4] <= 0)
                    {
                        LeftTime[4] = 0; OnIce = false;
                        if (DeBuffObj[4] != null) { DeBuffObj[4].SetActive(false); DeBuffObj[4].transform.parent = GameManager.instance.BFM.transform; DeBuffObj[4] = null; }
                        if(!OnStun) anim.speed = 1;
                    }
                }
                else
                {
                    if (LeftTime[i] > 0)
                    {
                        LeftTime[i] -= 0.1f;
                        if (LeftTime[i] <= 0) { LeftTime[i] = 0; DeBuffVar[i] = (i == 0 ? Cheeled == 0 ? 0 : 0.3f : 0); /*DeBuffObj[i].SetActive(false); DeBuffObj[i].transform.parent = GameManager.instance.BFM.transform; DeBuffObj[i] = null;*/ }
                    }
                }
            }
            yield return GameManager.DotOneSec;
        }
    }

    IEnumerator DefenseChange()
    {
        while (LeftTime[2] > 0)
        {
            yield return GameManager.OneSec;
            LeftTime[2]--;
        }
        Defense = MaxDefense;
        DeBuffObj[2].SetActive(false); DeBuffObj[2].transform.parent = GameManager.instance.BFM.transform;
        DeBuffObj[2] = null;
    }

    protected virtual void OnTriggerExit2D(Collider2D collision)
    {
        if (!IsLive) return;
        if (collision.CompareTag("Area") && !GameManager.instance.ES.IsPosFixed)
        {
            transform.position = GameManager.instance.player.Self.position + GameManager.instance.ES.ReBatchCall(transform.position);
        }
    }

    Coroutine Fric = null;

    IEnumerator NockBack_Enemy(float Power,Vector2 Dir)
    {
        CanHit = false; 
        spriteRenderer.color = Color.gray;
        Power += GameManager.instance.PlayerStatus.power - Weight * 5;
        OnHit = Power > 0;
        if (Power > 0) 
        {
            rigid.AddForce(Dir.normalized * (Power), ForceMode2D.Impulse);
            if (Fric == null) Fric = StartCoroutine(Friction());
        }
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.white;
        CanHit = true;
    }

    IEnumerator Friction()
    {
        while (rigid.velocity.magnitude >= 0.2f)
        {
            rigid.velocity = Vector2.Lerp(rigid.velocity, Vector2.zero, 0.5f);
            yield return new WaitForSeconds(0.1f);
        }
        rigid.velocity = Vector2.zero;
        Fric = null; OnHit = false;
    }


    protected virtual void Dead()
    {
        gameObject.SetActive(false);
        GameManager.instance.UM.KillCountUp(1);
        GameManager.instance.IM.MakeItem(transform.position);
    }
    protected bool IsInit = true;
    protected virtual void OnEnable()
    {
        if (IsInit) { IsInit = false; return; } 
        MoveAble = true; OnHit = false; Fric = null;
        
        anim.enabled = true; anim.speed = 1;
        anim.SetBool("IsAttack", false);
        BeginAttack = false;
        CanHit = true;
        rigid.simulated = true;
        coll.enabled = true;
        HP = Mathf.FloorToInt(MaxHP * (1 + GameManager.instance.EnemyStatus.hp));

        IsLive = true;

        OnIce = false; IceRatio = 1; Cheeled = 0;
        OnStun = false;
        Defense = MaxDefense; speed = MaxSpeed; Damage = MaxDamage;

        spriteRenderer.color = Color.white;
        spriteRenderer.sortingOrder = 2;

        tag = "Enemy";

        if(GameManager.instance.BFM != null) StartCoroutine(BuffCheck());
    }
}
