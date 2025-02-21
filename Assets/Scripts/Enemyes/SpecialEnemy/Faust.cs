using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Faust : Enemy
{
    protected override void Awake()
    {
        base.Awake();
        MaxHP = Mathf.FloorToInt(MaxHP * (1 + GameManager.instance.EnemyStatus.boss * 0.05f)); HP = MaxHP;
        MaxDefense = Mathf.FloorToInt(MaxDefense * (1 + GameManager.instance.EnemyStatus.boss * 0.05f)); Defense = MaxDefense;
        MaxDamage = Mathf.FloorToInt(MaxDamage * (1 + GameManager.instance.EnemyStatus.boss * 0.05f)); Damage = MaxDamage;
    }

    protected override void OnEnable()
    {
        if (!IsInit)
        {
            ChangeDirCor = StartCoroutine(ChangeDir());
            StartCoroutine(BatchCool());
            transform.position = GameManager.instance.ES.ReBatchCall(transform.position);
            Dir = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
            if (Dir.x > 0 && !spriteRenderer.flipX) spriteRenderer.flipX = true;
            else if (Dir.x < 0 && spriteRenderer.flipX) spriteRenderer.flipX = false;
            MakeWalls();
        }
        base.OnEnable();      
    }


    Vector2 Dir;
    protected override void FixedUpdate()
    {
        if (!IsLive || OnIce||OnStun) return;
        if (MoveAble && !anim.GetBool("IsAttack"))
        {
            if (BatchAble) { anim.SetTrigger("Batch"); anim.SetBool("IsAttack", true); BatchAble = false; }
            else if (!OnHit) rigid.MovePosition(rigid.position + Dir * speed * Time.fixedDeltaTime * (1 + GameManager.instance.EnemyStatus.speed - DeBuffVar[0]));
        }
    }

    int AttackCount = 0;
    new Transform Target;
    Vector3 DirSub;

    WaitForSeconds changedir = new WaitForSeconds(7);
    Coroutine ChangeDirCor = null;
    IEnumerator ChangeDir()
    {
        yield return changedir;

        Dir = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;

        RaycastHit2D IsWallExit = Physics2D.Raycast(transform.position, Dir , 10, LayerMask.GetMask("Wall"));
        while (IsWallExit.collider != null)
        {
            Dir = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
            IsWallExit = Physics2D.Raycast(transform.position, Dir, 10, LayerMask.GetMask("Wall"));
        }

        anim.SetBool("IsAttack",true);
        if (AttackCount++ == 2) anim.SetTrigger("Special");

        Target = GameManager.instance.UM.ReturnOrderPriority().transform;
        DirSub = (Target.position - transform.position).normalized;

        if (DirSub.x > 0 && !spriteRenderer.flipX) spriteRenderer.flipX = true;
        else if (DirSub.x < 0 && spriteRenderer.flipX) spriteRenderer.flipX = false;
        ChangeDirCor = null;
    }

    bool BatchAble = false;
    WaitForSeconds batchcool = new WaitForSeconds(5);
    IEnumerator BatchCool()
    {
        yield return batchcool;
        BatchAble = true;
    }

    List<PaustBow> BowList = new List<PaustBow>();

    public void BatchBow()
    {
        if (Bows.Count != 0)
        {
            int Ind = Random.Range(0, Bows.Count);
            Bows[Ind].SetActive(true);
            PaustBow cnt = Bows[Ind].GetComponent<PaustBow>(); cnt.Fs = this; BowList.Add(cnt);
            Bows.RemoveAt(Ind);
            StartCoroutine(BatchCool());
        }
    }

    public Sprite bull;
    public Sprite ShootEffect;
    public Sprite Spbull;

    protected override void AttackMethod()
    {
        if (GameManager.instance.player.Self.gameObject.activeSelf)
        {
            Target = GameManager.instance.UM.ReturnOrderPriority().transform;
            DirSub = (Target.position - transform.position).normalized;

            if (DirSub.x > 0 && !spriteRenderer.flipX) spriteRenderer.flipX = true;
            else if (DirSub.x < 0 && spriteRenderer.flipX) spriteRenderer.flipX = false;

            if (AttackCount == 3)
            {
                foreach (var k in BowList) k.SpecialShoot();
                GameManager.instance.BM.MakeBullet(new BulletInfo(Mathf.FloorToInt(Damage * 3 * (1 + GameManager.instance.EnemyStatus.attack + GameManager.instance.EnemyStatus.boss - DeBuffVar[1])), false, 0, ignoreDefense: 0.5f), 100, transform.position, DirSub, 100, true, Spbull); AttackCount = 0;
            }
            else
            {
                GameManager.instance.BM.MakeBullet(new BulletInfo(Mathf.FloorToInt(Damage * (1 + GameManager.instance.EnemyStatus.attack + GameManager.instance.EnemyStatus.boss - DeBuffVar[1])), false, 0, ignoreDefense: 0.2f), 0, transform.position, DirSub, 50, true, bull);
                GameManager.instance.BM.MakeEffect(0.3f, transform.position + DirSub * 3, DirSub, 0, ShootEffect);
            }
        }
    }

    bool IsHide = true;

    protected override void AttackEnd()
    {
        anim.SetBool("IsAttack", false);
        if (Dir.x > 0 && !spriteRenderer.flipX) spriteRenderer.flipX = true;
        else if (Dir.x < 0 && spriteRenderer.flipX) spriteRenderer.flipX = false;
        if (GameManager.instance.player.Self.gameObject.activeSelf && ChangeDirCor == null) ChangeDirCor = StartCoroutine(ChangeDir());
    }

    [SerializeField] GameObject Wall;
    List<GameObject> Walls = new List<GameObject>();
    List<GameObject> Bows = new List<GameObject>();
    void MakeWalls()
    {
        Vector3 PlayerPos = GameManager.instance.player.Self.position;
        for (float x = -30; x <= 30; x += 2)
        {
            GameObject j = Instantiate(Wall); Walls.Add(j); j.transform.position = PlayerPos + new Vector3(x, 20, 0);
        }
        
        for (float x = 18.5f; x >= -19; x -= 1.5f) 
        {
            var cnt = Instantiate(Wall); Walls.Add(cnt);
            cnt.transform.position = PlayerPos + new Vector3(-30, x, 0);
            var tmp = cnt.transform.GetChild(0).gameObject; tmp.GetComponent<SpriteRenderer>().flipX = true; tmp.GetComponent<PaustBow>().Dir = Vector3.right;
            Bows.Add(tmp);
            cnt = Instantiate(Wall); Walls.Add(cnt);
            cnt.transform.position = PlayerPos + new Vector3(30, x, 0);
            tmp = cnt.transform.GetChild(0).gameObject; tmp.GetComponent<PaustBow>().Dir = Vector3.left;
            Bows.Add(tmp);
        }
        for (float x = -30; x <= 30; x += 2)
        {
            GameObject j = Instantiate(Wall); Walls.Add(j);
            j.transform.position = PlayerPos + new Vector3(x, -20.5f, 0);
        }
    }

    [SerializeField] GameObject HideEffect;
    public void PhazeT()
    {
        GameManager.instance.UM.BossHP.fillAmount = 1;
        IsHide = false; gameObject.layer = 6;
        HideEffect.SetActive(false);
        Damage = (int)(Damage * 1.5f);
        GameManager.instance.UM.BossTransform = transform;
    }

    protected override void Dead()
    {
        foreach (var j in Walls) Destroy(j);
        for (int i = 0; i < 15; i++)
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


    protected override void HPChange()
    {
        GameManager.instance.UM.BossHP.fillAmount = HP / (float)MaxHP;
    }


    protected override void OnTriggerExit2D(Collider2D collision)
    {
        if (!IsHide) if (collision.CompareTag("Area")) GameManager.instance.UM.BossShaft.gameObject.SetActive(true);
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsHide)
        {
            if (collision.CompareTag("Area")) GameManager.instance.UM.BossShaft.gameObject.SetActive(false);
            base.OnTriggerEnter2D(collision);
        }
    }

    protected override void Heal(float Amount) { HP += (int)Amount; HP = Mathf.Min(MaxHP, HP); GameManager.instance.UM.BossHP.fillAmount = HP / (float)MaxHP; }

    [SerializeField] ParticleSystem AttackEffect;
    public void MakeAttackEffect()
    {
        AttackEffect.transform.localPosition = spriteRenderer.flipX ? new Vector3(3.85f, 0) : new Vector3(-3.85f, 0);
        AttackEffect.Play();
    }

    private void OnDisable()
    {
        GameManager.instance.UM.BossTransform = null;
        GameManager.instance.UM.BossShaft.gameObject.SetActive(false);
    }
}
