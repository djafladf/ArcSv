using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wafarin_Bat : MonoBehaviour
{
    [SerializeField] int IsAttack;
    [SerializeField] LayerMask[] Layers;
    [SerializeField] Wafarin Wafarin;

    bool OnTarget = false;
    Transform Target;
    Vector3 IdlePos = new Vector3(0, 2);
    Rigidbody2D rigid;
    BoxCollider2D coll;
    BulletInfo BI;
    private void Awake()
    {
        coll = GetComponent<BoxCollider2D>();
        rigid = GetComponent<Rigidbody2D>();
        if (IsAttack == 1) BI = new BulletInfo(0, false, 0, scalefactor: 0.1f, dealFrom: Wafarin.player.Id);
        else BI = new BulletInfo(0, false, 0, scalefactor: 0.1f, buffs: new Buff(heal: 0), dealFrom: Wafarin.player.Id);
    }

    Vector2 Dir;
    private void Update()
    {
        if (!Target.gameObject.activeSelf) { if(cor!=null) StopCoroutine(cor); var cnt = GameManager.GetNearest(10, 1, transform.position, Layers[IsAttack]);
            if (cnt.Count == 0) gameObject.SetActive(false); else Target = cnt[0]; 
            OnTarget = false; coll.enabled = true;  }

        if (!OnTarget)
        {
            Dir = (Target.position - transform.position).normalized;
            rigid.MovePosition(rigid.position + Dir * 75 * Time.deltaTime);
        }
        else transform.position = Target.position + IdlePos;
    }

    public void Init(Transform target)
    {
        Target = target; OnTarget = false; coll.enabled = true; ActCount = 3;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!gameObject.activeSelf) return;
        OnTarget = true;
        coll.enabled = false;

        if (IsAttack == 0 && collision.transform == Target) cor = StartCoroutine(HealCor());
        else
        {
            Target = collision.transform;
            cor = StartCoroutine(AttackCor());
        }
    }

    Coroutine cor = null;
    int ActCount;

    IEnumerator AttackCor()
    {
        while(ActCount-- > 0)
        {
            BI.Damage = (int)((1 + GameManager.instance.PlayerStatus.attack + Wafarin.player.AttackRatio + Wafarin.player.ReinforceAmount[0]) * Wafarin.DamageRatio);
            GameManager.instance.ES.InstanceTo[Target.gameObject].OnDamage(inf : BI);
            yield return GameManager.DotFiveSec;
        }
        gameObject.SetActive(false);
    }

    IEnumerator HealCor()
    {
        while (ActCount-- > 0)
        {
            BI.Buffs.Heal = (int)((1 + GameManager.instance.PlayerStatus.attack + Wafarin.player.AttackRatio + Wafarin.player.ReinforceAmount[0]) * Wafarin.HealRatio);
            GameManager.instance.GetScript(Target.gameObject).SetBuff(BI);
            //GameManager.instance.BM.MakeBuff(BI,Target.position,null,false);
            yield return GameManager.DotFiveSec;
        }
        gameObject.SetActive(false);
    }
}
