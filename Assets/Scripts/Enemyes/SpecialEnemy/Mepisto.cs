using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mepisto : Enemy
{
    [SerializeField] ParticleSystem VirusSystem;

    Faust faust;

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
            transform.position = GameManager.instance.player.Self.position + new Vector3(8, 0);
            GameManager.instance.ES.SetCurrentSpawnPos();
            GameManager.instance.UM.BossTransform = transform;
            GameManager.instance.UM.BossShaft.gameObject.SetActive(true);
            StartCoroutine(StartEffect());
        }
        base.OnEnable();
    }

    IEnumerator StartEffect()
    {
        yield return GameManager.OneSec; yield return GameManager.OneSec;
        GameManager.instance.ES.BossSet.SetActive(true);
        GameManager.instance.UM.BossName.text = "메피스토";
        GameManager.instance.UM.BossHP.fillAmount = 1;
        StartCoroutine(SkillCool());
        
        GameManager.instance.ES.ExternalSpawnCall(15, -1, 8f);
        GameManager.instance.ES.ExternalSpawnCall(16, -1, 8f);
        faust = GameManager.instance.ES.TakeOffObj(18).GetComponent<Faust>();

        float[] Sub = { -1, 0, 1 };
        for (int i = 0; i < 9; i++)
        {
            if (i == 4) continue;
            Vector3 cnt = new Vector3(Sub[i % 3] * 2.5f, Sub[i / 3] * 2.5f) + transform.position; cnt.z = 1;
            var tmp = GameManager.instance.ES.TakeOffObj(17);
            tmp.transform.position = cnt;
        }
    }

    IEnumerator SkillCool()
    {
        while (true)
        {
            yield return new WaitForSeconds(10);
            anim.SetTrigger("Skill");
        }
    }

    [SerializeField]LayerMask TargetLayer;
    Vector2 SkillRange = new Vector2(50, 24);
    Collider2D[] hits = new Collider2D[100];
    public void MakeHeal()
    {
        VirusSystem.Play();
        int res = Physics2D.OverlapBoxNonAlloc(transform.position, SkillRange, 0, hits, TargetLayer);
        for(int i = 0; i < res; i++)
        {
            GameManager.instance.BM.MakeEffect(0.5f, hits[i].transform.position,Vector3.zero,0,Bull);
            GameManager.instance.ES.InstanceTo[hits[i].gameObject].OnBuff(BI.Buffs);
        }
    }

    protected override void HPChange()
    {
        GameManager.instance.UM.BossHP.fillAmount = HP / (float)MaxHP;
        if (HP <= Mathf.FloorToInt(MaxHP * 0.25f))
        {
            Invoke("Exit", 1);
            faust.PhazeT();
        }
    }

    void Exit()
    {
        gameObject.SetActive(false);
    }

    protected override void Heal(float Amount) { HP += (int)Amount; HP = Mathf.Min(MaxHP, HP); GameManager.instance.UM.BossHP.fillAmount = HP / (float)MaxHP; }

    new void FixedUpdate()
    {

    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Area"))
        {
            GameManager.instance.UM.BossShaft.gameObject.SetActive(false);
        }
        base.OnTriggerEnter2D(collision);
    }

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Area"))
        {
            GameManager.instance.UM.BossShaft.gameObject.SetActive(true);
        }
    }

}
