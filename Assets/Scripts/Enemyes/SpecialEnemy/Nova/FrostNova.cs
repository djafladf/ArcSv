using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrostNova : Enemy
{
    [SerializeField] List<ParticleSystem> Particles;
    [SerializeField] Sprite BullTwo;
    [SerializeField] GameObject Ground;
    [SerializeField] GameObject IceBull;
    [SerializeField] GameObject BPart;
    [SerializeField] SpriteRenderer Border;
    [SerializeField] SpriteRenderer TEffect;

    [SerializeField] GameObject NovaMap;
    [SerializeField] GameObject NovaWall;
    [SerializeField] GameObject HideObj;
    [SerializeField] Transform Phaze1Ground;

    [SerializeField] AudioClip clip;
    TrailRenderer BLine;

    [HideInInspector]public DeBuff DeBuffOne = new DeBuff(last : 1, ice : 1);
    [HideInInspector]public DeBuff DeBuffTwo = new DeBuff(last: 1, ice: 5);

    bool IsPhazeChanging = false;

    protected override void Awake()
    {
        base.Awake();
        BLine = BPart.GetComponent<TrailRenderer>();
        for (int i = 0; i < 25; i++) IceBulls.Add(Instantiate(IceBull));
        foreach (var j in IceBulls) j.GetComponent<IceBull>().nova = this;

        IceBull = null; IceRatio = 0;
    }

    int CurPhaze = 0;

    protected override void FixedUpdate()
    {
        if (!IsLive || OnIce) return;

        if (MoveAble && !anim.GetBool("IsAttack"))
        {
            Vector2 Dir = (GameManager.instance.player.Self.position - transform.position).normalized;
            if (Dir.x > 0 && !spriteRenderer.flipX)
            {
                spriteRenderer.flipX = true;
                Particles[9].transform.localPosition = new Vector3(0.1f, 0.7f);
                if (CurPhaze >= 3) for (int i = 2; i <= 4; i++)
                    {
                        Particles[i].transform.rotation = Quaternion.Euler(0, 0, 120);
                    }
            }
            else if (Dir.x < 0 && spriteRenderer.flipX)
            {
                spriteRenderer.flipX = false;
                Particles[9].transform.localPosition = new Vector3(-0.1f, 0.7f);
                if (CurPhaze >= 3) for (int i = 2; i <= 4; i++)
                    {
                        Particles[i].transform.rotation = Quaternion.Euler(0, 0, 0);
                    }
            }

            if (!OnHit) rigid.MovePosition(rigid.position + Dir * speed * Time.fixedDeltaTime * (1 + GameManager.instance.EnemyStatus.speed - DeBuffVar[0]));
        }

        

        if (BeginAttack && !anim.GetBool("IsAttack"))
        {
            MoveAble = false;
            anim.SetBool("IsAttack", true);
        }

        if (CurPhaze == 0)if(Vector3.Magnitude(GameManager.instance.player.Self.position - transform.position) < 10)
            {
                CurPhaze = 1; MoveAble = false;
                Particles[0].Play(); StartCoroutine(Phaze1GroundSet());
                anim.SetTrigger("Phaze1");
                GameManager.instance.AudioM.ChangeMusic(clip);
            }
    }

    protected override void OnEnable()
    {
        if (!IsInit)
        {
            GameManager.instance.ES.BossSet.SetActive(true);
            GameManager.instance.UM.BossName.text = "프로스트노바";
            GameManager.instance.UM.BossHP.fillAmount = 1;
            GameManager.instance.UM.BossTransform = transform;
            GameManager.instance.UM.BossShaft.gameObject.SetActive(true);
            GameManager.instance.ES.SetCurrentSpawnPos();

            GameManager.instance.ES.ExternalSpawnCall(20, -1, 8);
            GameManager.instance.ES.ExternalSpawnCall(21, -1, 10);
            GameManager.instance.ES.ExternalSpawnCall(22, -1, 12);

            Particles[8].transform.SetParent(Camera.main.transform);
            Particles[8].transform.localPosition = Vector3.zero;
            Border.transform.SetParent(Camera.main.transform);
            Border.transform.localPosition = new Vector3(0,0,5);
            MakeWalls();
            NovaMap = Instantiate(NovaMap, GameObject.Find("Map").transform); NovaMap.transform.position = GameManager.instance.player.Self.transform.position;
        }
        base.OnEnable();
    }

    // Normal Attack
    protected override void AttackMethod()
    {
        int CurDamage = Mathf.FloorToInt(Damage * (1 + GameManager.instance.EnemyStatus.attack - DeBuffVar[1]));
        GameManager.instance.BM.MakeMeele(new BulletInfo(CurDamage,false,0,scalefactor:10,debuffs:DeBuffOne), 0.2f, transform.position, Vector2.zero, 0, true);
    }
    #region Phaze 1
    protected void Phaze1_1()
    {

        IceBulls[0].transform.position = transform.position + (spriteRenderer.flipX ? new Vector3(1,1) : new Vector3(-1, 1));
        IceBulls[0].SetActive(true);

        IceBulls[1].transform.position = transform.position + new Vector3(-1f, 1.5f);
        IceBulls[1].SetActive(true);

        IceBulls[2].transform.position = transform.position + new Vector3(1f, 1.5f);
        IceBulls[2].SetActive(true);
    }

    protected void Phaze1_2()
    {
        for (int i = 0; i < 10; i++)
        {
            float j = Random.Range(-3, 3) * 0.5f;
            IceBulls[i].transform.position = transform.position + new Vector3(i + j * 0.25f - 4.5f, 2 + j);
            IceBulls[i].SetActive(true);
        }
    }

    protected void Phaze1_3()
    {
        for(int i = 0; i < IceBulls.Count; i++)
        {
            float j = Random.Range(-3, 3);
            IceBulls[i].transform.position = transform.position + new Vector3(i%10 + j * 0.25f - 4.5f,4 - Mathf.FloorToInt(i/10) + j);
            IceBulls[i].SetActive(true);
        }
    }
    IEnumerator Phaze1GroundSet()
    {
        Vector3 Sub = new Vector3(0.01f, 0.01f, 0);
        Phaze1Ground.transform.parent = null;
        for(int i = 0; i < 100; i++)
        {
            Phaze1Ground.localScale += Sub;
            yield return GameManager.DotOneSec;
        }
    }
    #endregion

    #region Phaze 2

    protected void BoomAttack_One()
    {
        int CurDm = Mathf.FloorToInt(Damage * 2 * (1 + GameManager.instance.EnemyStatus.attack - DeBuffVar[1]));
        GameManager.instance.BM.MakeMeele(new BulletInfo(CurDm, false, 0, debuffs: DeBuffTwo), 0.5f, transform.position, Vector2.zero, 0, true, im: Bull);
        Particles[1].Play();
    }

    #endregion

    #region Phaze 3

    // Phaze 3 Start
    protected void Phaze3()
    {
        StartCoroutine(Phaze3Sub());
    }

    IEnumerator Phaze3Sub()
    {
        Color s = new Color(0, 0, 0, 0.1f);
        for (int i = 0; i < 10; i++)
        {
            yield return GameManager.DotOneSec;
            TEffect.color += s;
        }
        foreach (var j in GameManager.instance.Scripts) if (j.gameObject.activeSelf) j.GetDamage(new BulletInfo(10000, false, 0, 200, isFix: true), MustHit: true);
        NovaMap.SetActive(true);
        Border.color = new Color(1, 1, 1, 0.8f - 0.35f * Phaze3Process);
        yield return GameManager.DotFiveSec;
        HideObj.SetActive(false);
        var cnt = Particles[8].emission;
        cnt.rateOverTime = 80 - 35 * Phaze3Process;
        for (int i = 0; i < 10; i++)
        {
            yield return GameManager.DotOneSec;
            TEffect.color -= s;
        }
    }

    IEnumerator Phaze3DeBuff()
    {
        BulletInfo InstanceDamage = new BulletInfo(2, false, 0, scalefactor: 125, debuffs: DeBuffOne);
        while (IsLive)
        {
            if (!IsPhazeChanging)
            {
                HP -= 50;
                HPChange();
                foreach (var j in GameManager.instance.Scripts) if(j.gameObject.activeSelf) j.GetDamage(InstanceDamage,MustHit:true);
            }
            yield return GameManager.OneSec;
        }
    }


    Vector3 PlayerPos;

    protected void SetHideObj()
    {
        HideObj.transform.position = PlayerPos + new Vector3(Random.Range(-15, 15), Random.Range(-10, 10));
        HideObj.SetActive(true);
    }
    // Effects Set
    protected void AttackThreeSet(int IsActive)
    {
        switch (IsActive)
        {
            case 0:
                Particles[5].Play();
                break;
            case 1:
                Particles[5].Stop(); break;
            case 2:
                Particles[6].Play(); break;
            default:
                Particles[7].Play(); break;
        }
    }

    protected void BoomAttack_Two()
    {
        int CurDm = Mathf.FloorToInt(Damage * 2.5f * (1 + GameManager.instance.EnemyStatus.attack - DeBuffVar[1]));
        GameManager.instance.BM.MakeMeele(new BulletInfo(CurDm, false, 0, debuffs: DeBuffTwo), 0.5f, transform.position, Vector2.zero, 0, true, im: BullTwo);
    }

    #endregion

    // IceGround
    int GroundCount = 3;
    protected void MakeGround()
    {
        List<int> AttackTargets = new List<int>();
        for (int i = 0; i < GroundCount; i++) { if (GameManager.instance.UM.BatchOrder.Count - 1 - i < 0) break; AttackTargets.Add(GameManager.instance.UM.BatchOrder[GameManager.instance.UM.BatchOrder.Count - 1 - i]); }
        foreach (var k in GameManager.instance.UM.BatchOrder) if (GameManager.instance.UM.IsPriorityAttack[k])
            {
                if (AttackTargets.Contains(k)) AttackTargets.Remove(k);
                AttackTargets.Insert(0, k);
            }

        int l = AttackTargets.Count;
        for (int i = GroundCount; i < l; i++) AttackTargets.RemoveAt(AttackTargets.Count - 1);

        foreach (var k in AttackTargets)
        {
            GameObject ground = Instantiate(Ground);
            ground.GetComponent<IceGround>().Init(this);
            ground.transform.position = GameManager.instance.Prefs[k].transform.position;
        }
        for (int i = AttackTargets.Count; i < GroundCount; i++)
        {
            GameObject ground = Instantiate(Ground);
            ground.GetComponent<IceGround>().Init(this);
            ground.transform.position = transform.position + new Vector3(Random.Range(-3, 4) * 5, Random.Range(-2, 3) * 5);
        }
    }

    // IceBull
    int IceBullCount = 1;
    public List<Sprite> IceBullSprites;
    List<GameObject> IceBulls = new List<GameObject>();
    protected IEnumerator MakeIceBull()
    {
        WaitForSeconds WFS = new WaitForSeconds(6);
        while (true)
        {
            for (int i = 0; i < IceBullCount; i++)
            {
                float j = Random.Range(-3, 3) * 0.2f;
                IceBulls[i].transform.position = transform.position + new Vector3(j * 2, 2 + j);
                IceBulls[i].SetActive(true);
                yield return new WaitForSeconds(Random.Range(0.05f,0.2f));
            }
            yield return WFS;
        }
    }
    public void ShootIceBull(Vector3 pos)
    {
        var j = GameManager.instance.UM.ReturnOrderPriority().transform.position;
        int CurDm = Mathf.FloorToInt(Damage * (1 + GameManager.instance.EnemyStatus.attack - DeBuffVar[1]));
        GameManager.instance.BM.MakeBullet(new BulletInfo(CurDm, false, 0,debuffs:DeBuffOne), 0, pos, (j - pos).normalized, 30, true, IceBullSprites[4]);
    }


    float Phaze3Process = 0f;

    protected override void HPChange()
    {
        if(CurPhaze == 3)
        {
            if(Phaze3Process == 0 && HP <= MaxHP * 0.65f)
            {
                MoveAble = false; IsPhazeChanging = true; anim.SetTrigger("Phaze3"); Phaze3Process = 1;
                speed = Mathf.FloorToInt(speed * 0.8f); Damage *= Mathf.FloorToInt(Damage  * 0.8f); anim.SetFloat("AttackSpeed", 0.8f);
            }
            else if(Phaze3Process == 1 && HP <= MaxHP * 0.3f)
            {
                MoveAble = false; IsPhazeChanging = true; anim.SetTrigger("Phaze3"); Phaze3Process = 2;
                speed = Mathf.FloorToInt(speed * 0.8f); Damage *= Mathf.FloorToInt(Damage * 0.6f); anim.SetFloat("AttackSpeed", 0.6f);
            }
        }
        else if(HP <= 1)
        {
            if(CurPhaze == 1 || CurPhaze == 2)
            {
                var cnt = Particles[8].emission;
                cnt.rateOverTime = CurPhaze * 40;
                if (SkillOne != null) StopCoroutine(SkillOne); if (SkillTwo != null) StopCoroutine(SkillTwo);
                MoveAble = false;
                HP = 1; IsPhazeChanging = true;
                CurPhaze++;
            }

            if(CurPhaze == 2)
            {
                Weight = 1;
                MaxHP = 15000;
                Destroy(Particles[0].gameObject);
                anim.SetTrigger("Phaze2");
            }
            else if (CurPhaze == 3)
            {
                Destroy(Particles[1].gameObject);
                Damage = Mathf.FloorToInt(Damage * 1.5f); speed = Mathf.FloorToInt(speed * 1.5f);
                GroundCount = 5; IceBullCount = 5;
                anim.SetTrigger("Phaze3");
            }
        }
        GameManager.instance.UM.BossHP.fillAmount = HP / (float)MaxHP;
    }

    #region Skill CoolDown
    bool SkillOneAble = false;
    bool SkillTwoAble = false;
    Coroutine SkillOne = null;
    Coroutine SkillTwo = null;
    WaitForSeconds SkillOneCool = new WaitForSeconds(10);
    WaitForSeconds SkillTwoCool = new WaitForSeconds(15);

    IEnumerator SkillCoolOne()
    {
        SkillOneAble = false;
        anim.SetBool("SkillOneAble", false);
        yield return SkillOneCool;
        SkillOneAble = true;
        anim.SetBool("SkillOneAble", true);
    }

    IEnumerator SkillCoolTwo()
    {
        SkillTwoAble = false;
        anim.SetBool("SkillTwoAble", false);
        yield return SkillTwoCool;
        SkillTwoAble = true;
        anim.SetBool("SkillTwoAble", true);
    }

    protected void SkillStart(int ind)
    {
        MoveAble = false; anim.SetBool("IsAttack", true);
        if (ind == 0) { SkillOneAble = false; anim.SetBool("SkillOneAble", false); }
        else { SkillTwoAble = false; anim.SetBool("SkillTwoAble", false); }
    }

    protected void SkillEnd(int ind)
    {
        MoveAble = true; anim.SetBool("IsAttack", false);
        if (ind == 0) StartCoroutine(SkillCoolOne());
        else StartCoroutine(SkillCoolTwo());
    }
    #endregion

    protected override void AttackEnd()
    {
        if (!BeginAttack || SkillOneAble || SkillTwoAble)
        {
            anim.SetBool("IsAttack", false);
            MoveAble = !(SkillOneAble || SkillTwoAble);
        }
    }


    // Phaze Change
    protected void ChangeHpHeal()
    {
        StartCoroutine(PhazeChangeHPHeal());
    }

    IEnumerator PhazeChangeHPHeal()
    {
        MoveAble = true; LineCount = 5; BPart.SetActive(true); StartCoroutine(Line1());
        if (Phaze3Process == 0)
        {
            SkillOne = StartCoroutine(SkillCoolOne()); SkillTwo = StartCoroutine(SkillCoolTwo());
            StartCoroutine(MakeIceBull());
            GameManager.instance.UM.BossHP.fillAmount = 0;
            if (CurPhaze == 2) { Border.gameObject.SetActive(true); StartCoroutine(BorderAct()); }
            if (CurPhaze == 3)
            {
                Particles[9].Play(); Particles[2].gameObject.SetActive(true); Particles[3].gameObject.SetActive(true); Particles[4].gameObject.SetActive(true); 
                GameManager.instance.UM.BossName.text = "프로스트노바-겨울의 상처";
                StartCoroutine(Phaze3DeBuff());
                if (spriteRenderer.flipX)
                {
                    for (int i = 2; i <= 4; i++)
                    {
                        Particles[i].transform.rotation = Quaternion.Euler(0, 0, 120);
                    }
                    Particles[9].transform.localPosition = new Vector3(0.1f, 0.7f);
                }
                // +@
            }
            for (int i = 0; i < 20; i++) { GameManager.instance.UM.BossHP.fillAmount += 0.05f; yield return GameManager.DotOneSec; }
        }
    }

    IEnumerator BorderAct()
    {
        Border.color = new Color(1, 1, 1, 0);
        for(int i = 0; i < 10; i++)
        {
            Border.color += new Color(0, 0, 0, 0.02f); yield return GameManager.DotOneSec;
        }
    }

    // 무적 Effect
    int LineCount = 0;
    IEnumerator Line1()
    {
        if (LineCount-- > 0)
        {
            BPart.transform.localPosition = new Vector3(1, -1, 0); BLine.Clear();
            float Theta1 = 315 * Mathf.Deg2Rad;
            float Theta2 = 135 * Mathf.Deg2Rad;
            float Theta = (Theta2 - Theta1) * 0.2f;
            for (int i = 1; i < 6; i++)
            {
                float NewTheta = Theta1 + Theta * i;
                BPart.transform.localPosition = new Vector3(Mathf.Cos(NewTheta), Mathf.Sin(NewTheta), 0);
                yield return new WaitForSeconds(0.1f);
            }

            yield return GameManager.DotOneSec;
            StartCoroutine(Line2());
        }
        else
        {
            if (Phaze3Process == 0)
            {
                GameManager.instance.UM.BossHP.fillAmount = 1;
                HP = MaxHP;
            }
            BPart.SetActive(false);
            IsPhazeChanging = false;
        }
    }

    IEnumerator Line2()
    {
        BPart.transform.localPosition = new Vector3(-1, -1, 0); BLine.Clear();

        float Theta1 = 225 * Mathf.Deg2Rad;
        float Theta2 = 45 * Mathf.Deg2Rad;
        float Theta = (Theta2 - Theta1) * 0.2f;
        float cnt = Mathf.Sqrt(6);
        for (int i = 1; i < 6; i++)
        {
            float NewTheta = Theta1 - Theta * i;
            BPart.transform.localPosition = new Vector3(Mathf.Cos(NewTheta), Mathf.Sin(NewTheta), 0);
            yield return new WaitForSeconds(0.1f);
        }

        yield return GameManager.DotOneSec;
        StartCoroutine(Line1());

    }
    // etc

    List<GameObject> Walls = new List<GameObject>();
    void MakeWalls()
    {
        PlayerPos = GameManager.instance.player.Self.position;
        for (float x = -40; x <= 40; x += 2)
        {
            var j = Instantiate(NovaWall); j.transform.position = PlayerPos + new Vector3(x, 26, 0); Walls.Add(j);
        }
        for (float x = 26f; x >= -26.5f; x -= 1.5f)
        {
            var j = Instantiate(NovaWall); j.transform.position = PlayerPos + new Vector3(-40, x, 0); Walls.Add(j);
            j = Instantiate(NovaWall); j.transform.position = PlayerPos + new Vector3(40, x, 0); Walls.Add(j);
        }
        for (float x = -40; x <= 40; x += 2)
        {
            var j = Instantiate(NovaWall); j.transform.position = PlayerPos + new Vector3(x,- 26.5f, 0); Walls.Add(j);
        }
    }

    public float ReturnDam()
    {
        return (1 + GameManager.instance.EnemyStatus.attack - DeBuffVar[1]);
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Area"))
        {
            GameManager.instance.UM.BossShaft.gameObject.SetActive(false);
        }
        if (!IsPhazeChanging) base.OnTriggerEnter2D(collision);
    }

    protected override void OnTriggerExit2D(Collider2D collision) 
    {
        if (collision.CompareTag("Area"))
        {
            GameManager.instance.UM.BossShaft.gameObject.SetActive(true);
        }
    }

    protected override void Dead()
    {
        foreach (var j in Walls) Destroy(j);
        for (int i = 0; i < 20; i++)
        {
            GameManager.instance.IM.MakeItem(transform.position +
                new Vector3(Random.Range(-10, 10) * 0.3f, Random.Range(-10, 10) * 0.3f, 0), true);
        }
        GameManager.instance.UM.ShowDialog(new List<string>() { },
                () => { GameManager.instance.BossEnd(); }
                );
        GameManager.instance.ES.ReleaseSpawnPos();
        gameObject.SetActive(false);
    }


    bool FirstDis = true;
    private void OnDisable()
    {
        if (FirstDis) FirstDis = false;
        else
        {
            if(Border != null) Destroy(Border.gameObject); if(Particles[8] != null) Destroy(Particles[8].gameObject);
        }
    }

}
