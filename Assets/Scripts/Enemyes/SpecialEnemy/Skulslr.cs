using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skulslr : Enemy
{

    [SerializeField] Sprite Boom;
    [SerializeField] Sprite Bullet;
    [SerializeField] Sprite MeeleBul;
    [SerializeField] ParticleSystem pt;
    [SerializeField] GameObject BPart;
    [SerializeField] AfterImMaker AfterIm;
    TrailRenderer BLine;
    ParticleSystem.Particle[] particles;

    protected override void Awake()
    {
        base.Awake();
        particles = new ParticleSystem.Particle[pt.main.maxParticles];
        MaxHP = Mathf.FloorToInt(MaxHP * (1 + GameManager.instance.EnemyStatus.boss * 0.05f));
        MaxDefense = Mathf.FloorToInt(MaxDefense * (1 + GameManager.instance.EnemyStatus.boss * 0.05f));
        MaxDamage = Mathf.FloorToInt(MaxDamage * (1 + GameManager.instance.EnemyStatus.boss * 0.05f));
        BLine = BPart.GetComponent<TrailRenderer>();
    }

    private void Start()
    {
        GameManager.instance.ES.ExternalSpawnCall(2, -1, 5f);
        GameManager.instance.ES.ExternalSpawnCall(4, -1, 5f);
        GameManager.instance.ES.ExternalSpawnCall(3, -1, 5f);
        GameManager.instance.ES.ExternalSpawnCall(5, -1, 5f);
        GameManager.instance.ES.ExternalSpawnCall(6, -1, 5f);
    }

    private void Update()
    {
        if (pt.gameObject.activeSelf)
        {
            int numParticlesAlive = pt.GetParticles(particles);
            for (int i = 0; i < numParticlesAlive; i++)
            {
                float angle = Mathf.Atan2(particles[i].velocity.y, particles[i].velocity.x) * Mathf.Rad2Deg;
                particles[i].rotation = -angle;
            }

            pt.SetParticles(particles, numParticlesAlive);
        }
    }

    bool IsRush = false;
    Transform RushTarget;
    int l = 0;

    protected override void FixedUpdate()
    {
        if (!IsLive) return;

        if (OnIce) return;
        if (IsRush)
        {
            Vector2 Dir = (RushTarget.position - transform.position).normalized;
            if (Dir.x > 0 && !spriteRenderer.flipX) spriteRenderer.flipX = true;
            else if (Dir.x < 0 && spriteRenderer.flipX) spriteRenderer.flipX = false;
            if (Vector2.Distance(transform.position, RushTarget.position) <= 2)
            { 
                AfterIm.StopMaking();
                anim.SetTrigger("Special2_End");
                AttackPos = RushTarget.position;
                IsRush = false;
            }
            rigid.MovePosition(rigid.position + Dir * speed * 15 * Time.fixedDeltaTime);
            return;
        }

        if (MoveAble && !anim.GetBool("IsAttack"))
        {
            Vector2 Dir = (GameManager.instance.player.Self.position - transform.position).normalized;
            if (Dir.x > 0 && !spriteRenderer.flipX) spriteRenderer.flipX = true;
            else if (Dir.x < 0 && spriteRenderer.flipX) spriteRenderer.flipX = false;

            if (!OnHit) rigid.MovePosition(rigid.position + Dir * speed * Time.fixedDeltaTime * (1 + GameManager.instance.EnemyStatus.speed));
        }
        if (BeginAttack && !anim.GetBool("IsAttack"))
        {
            AttackPos = Target.position;
            MoveAble = false;
            anim.SetBool("IsRange", Vector2.Distance(AttackPos, transform.position) >= 3f);
            anim.SetBool("IsAttack", true);
        }
    }

    protected override void AttackMethod()
    {
        GameManager.instance.BM.MakeBoom(
            new BulletInfo(Damage,false,0),new BulletInfo(Damage,false,0,ignoreDefense:0.5f),
            transform.position, (AttackPos - transform.position).normalized, 20, Bullet, Boom, true);
    }

    void MeeleAttack()
    {
        GameManager.instance.BM.MakeMeele(new BulletInfo(Mathf.FloorToInt(Damage * 1.5f), false, 0), 0.3f, transform.position, spriteRenderer.flipX ? Vector2.left:Vector2.zero, 0, true,MeeleBul);
    }

    Transform ReturnRandomPlayer()
    {
        List<Transform> Targets = new List<Transform>();
        foreach (var k in GameManager.instance.Prefs) if (k.activeSelf && k.tag == "Player") Targets.Add(k.transform);
        if (Targets.Count == 0) return null;
        else return Targets[Random.Range(0, Targets.Count)];
    }

    void SpecialAttack()
    {
        Vector3 cnt = ReturnRandomPlayer().position
            + new Vector3(Random.Range(-2f, 2f), Random.Range(-2f, 2f), 0);
        GameManager.instance.BM.MakeEffect(jj ? 0.5f : 1f, transform.position + Vector3.up, Vector3.up,jj? 60 : 40, Bullet);
        GameManager.instance.BM.MakeWarning(cnt,1f,Boom.bounds.size * 0.8f,Color.red, SpecialAttackSub);
    }

    void SpecialAttackSub(Vector3 pos)
    {    
        GameManager.instance.BM.MakeMeele(new BulletInfo(Damage,false,0,ignoreDefense:0.5f), 0.3f, pos, Vector3.zero, 0, true,Boom);
    }
    int SpecialCool = 20;


    int Count = 0;
    float SpecialRad;
    public void SpecialAttack3()
    {
        
        GameManager.instance.BM.MakeMeele(new BulletInfo(Damage, false, 0, ignoreDefense: 0.5f), 0.3f,
            transform.position + new Vector3(6 * Mathf.Cos(SpecialRad + 0.75f * Count),6 * Mathf.Sin(SpecialRad + 0.75f * Count),0), 
            Vector3.zero, 0, true, Boom);
        GameManager.instance.BM.MakeMeele(new BulletInfo(Damage, false, 0, ignoreDefense: 0.5f), 0.3f,
            transform.position + new Vector3(6 * Mathf.Cos(SpecialRad - 0.75f * Count),6 * Mathf.Sin(SpecialRad - 0.75f * Count), 0),
            Vector3.zero, 0, true, Boom);
        Count++;
    }

    IEnumerator SpecialCoolDown()
    {
        if (GameManager.instance.Prefs[0].activeSelf)
        {
            MoveAble = false;

            if (Target != null)
            {
                if (Vector2.Distance(Target.position, transform.position) <= 6f)
                {
                    anim.SetTrigger("Special3");
                    Vector2 Sub = (Target.position - transform.position).normalized;
                    SpecialRad = Vector2.Angle(Vector2.right, Sub) * Mathf.Deg2Rad;
                    if (Sub.y < 0) SpecialRad = Mathf.PI * 2 - SpecialRad; Count = 0;
                    yield return new WaitForSeconds(SpecialCool * 0.4f);
                }
                else
                    switch (Random.Range(0, 2))
                    {
                        case 0:
                            anim.SetTrigger("Special"); yield return new WaitForSeconds(SpecialCool * 0.5f); break;
                        case 1:
                            anim.SetTrigger("Special2"); anim.SetBool("IsAttack", false); yield return GameManager.OneSec; RushTarget = ReturnRandomPlayer(); l = 0; IsRush = true; AfterIm.StartMaking(); yield return new WaitForSeconds(SpecialCool * 0.5f); break;
                    }
            }

            else
                switch (Random.Range(0, 2))
                {
                    case 0:
                        anim.SetTrigger("Special"); yield return new WaitForSeconds(SpecialCool * 0.5f); break;
                    case 1:
                        anim.SetTrigger("Special2"); anim.SetBool("IsAttack", false); yield return GameManager.OneSec; RushTarget = ReturnRandomPlayer(); IsRush = true; AfterIm.StartMaking(); yield return new WaitForSeconds(SpecialCool * 0.5f); break;
                }
            StartCoroutine(SpecialCoolDown());
        }
    }

    
    protected override void AttackEnd()
    {
        base.AttackEnd();
        if (BeginAttack)
        {
            AttackPos = Target.position;
            Vector2 Dir = (AttackPos - transform.position).normalized;
            if (Dir.x > 0 && !spriteRenderer.flipX) spriteRenderer.flipX = true;
            else if (Dir.x < 0 && spriteRenderer.flipX) spriteRenderer.flipX = false;
            anim.SetBool("IsRange", Vector2.Distance(AttackPos, transform.position) >= 3f);
        }
        
    }
    bool IsMisya = false;
    protected override void Dead()
    {
        gameObject.SetActive(false);
        for (int i = 0; i < 10; i++)
        {
            GameManager.instance.IM.MakeItem(transform.position + 
                new Vector3(Random.Range(-10, 10) * 0.3f, Random.Range(-10, 10) * 0.3f, 0),true);
        }
        if (!IsMisya) 
        {
            GameManager.instance.UM.ShowDialog(new List<string>() { "��..��..." },
                () => { GameManager.instance.BossEnd();  }
                );
            MaxDamage = MaxDamage / 2;
            MaxHP = MaxHP / 2;
            MaxDefense = MaxDefense / 2;
            MaxSpeed = 2;
            jj = false; pt.gameObject.SetActive(false); BPart.SetActive(false);
            IsMisya = true; IsRush = false;
            spriteRenderer.color = Color.white;
        }
        else
        {
            GameManager.instance.UM.ShowDialog(new List<string>() { "�̰ɷ�..." },
                () => { GameManager.instance.BossEnd(); }
                );
        }

    }

    bool StartEn = true;
    protected override void OnEnable()
    {
        base.OnEnable();
        
        if (StartEn) { StartEn = false; return; }
        GameManager.instance.UM.BossName.text = IsMisya? "���ý�����?" : "���ý�����";
        GameManager.instance.UM.BossHP.fillAmount = 1;
        if (!IsMisya) StartCoroutine(SpecialCoolDown());
    }

    bool jj = false;
    protected override void HPChange()
    {
        GameManager.instance.UM.BossHP.fillAmount = HP / (float)MaxHP;
        if(!jj && HP <= Mathf.FloorToInt(MaxHP * 0.5f))
        {
            jj = true; pt.gameObject.SetActive(true);
            float j = (float)Damage / MaxDamage;
            MaxDamage *= 2; Damage = Mathf.FloorToInt(MaxDamage * j);
            j = (float)speed / MaxSpeed;
            MaxSpeed = MaxSpeed * 1.5f; speed = Mathf.FloorToInt(MaxSpeed * j);
            BPart.SetActive(true); StartCoroutine(Line1());
        }
    }


    IEnumerator Line1()
    {
        BPart.transform.localPosition = new Vector3(2, -2, 0); BLine.Clear();
        float Theta1 = 315 * Mathf.Deg2Rad;
        float Theta2 = 135 * Mathf.Deg2Rad;
        float Theta = (Theta2 - Theta1) * 0.2f;
        float cnt = 2;
        for(int i = 1; i < 6; i++)
        {
            float NewTheta = Theta1 + Theta * i;
            BPart.transform.localPosition = new Vector3(Mathf.Cos(NewTheta) * cnt,Mathf.Sin(NewTheta) * cnt,0);
            yield return new WaitForSeconds(0.1f);
        }

        yield return GameManager.DotOneSec;
        StartCoroutine(Line2());
    }

    IEnumerator Line2()
    {
        BPart.transform.localPosition = new Vector3(-2, -2, 0); BLine.Clear();
        
        float Theta1 = 225 * Mathf.Deg2Rad;
        float Theta2 = 45 * Mathf.Deg2Rad;
        float Theta = (Theta2 - Theta1) * 0.2f;
        float cnt = Mathf.Sqrt(6);
        for (int i = 1; i < 6; i++)
        {
            float NewTheta = Theta1 - Theta * i;
            BPart.transform.localPosition = new Vector3(Mathf.Cos(NewTheta) * cnt, Mathf.Sin(NewTheta) * cnt, 0);
            yield return new WaitForSeconds(0.1f);
        }

        yield return GameManager.DotOneSec;
        StartCoroutine(Line1());

    }
}
