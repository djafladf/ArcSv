using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rosmon_Special : MonoBehaviour
{
    [SerializeField] LayerMask TargetLay;
    [SerializeField] Player Rosmon;
    Rigidbody2D rigid;
    Transform Target = null;
    AfterImMaker AIM;
    Transform AimTrans;
    AudioSource AS;
    bool ComeBack = false;

    private void Awake()
    {
        AS = GetComponent<AudioSource>();
        AimTrans = transform.GetChild(0);
        AIM = transform.GetChild(0).GetComponent<AfterImMaker>();
        rigid = GetComponent<Rigidbody2D>();
    }

    BulletInfo BI;
    private void Start()
    {
        BI = new BulletInfo(0, false,0, debuffs: new DeBuff(last: 5, defense: 0.5f), dealFrom: Rosmon.Id);
    }


    bool tmp = false;
    bool IsAddForce = false;
    bool GapChange = false;
    float ChangeTime = 0;

    private void FixedUpdate()
    {
        if(Target == null)
        {
            if (ChangeTime <= 0)
            {
                if (!ComeBack)
                {
                    tmp = false; GapChange = false;
                    Vector3 Mag = (transform.position - transform.parent.position); Mag.y *= 1.5f; Mag.z = 0;
                    if (Vector3.Magnitude(Mag) > 20)
                    {
                        ComeBack = true; Target = transform.parent;
                    }
                    else
                    {
                        var cnt = GameManager.GetNearest(20, 1, transform.position, TargetLay); if (cnt.Count != 0) { Target = cnt[0]; }
                    }
                }
            }
            else ChangeTime -= Time.deltaTime;
        }
        else
        {
            Vector2 cnt = Target.position - transform.position;
            Vector2 Sub = cnt.normalized;

            IsAddForce = true;

            if (ComeBack) 
            {
                if (Vector3.Magnitude(transform.position - transform.parent.position) < 5) { Target = null; ComeBack = false; ChangeTime = 0.5f; } 
            }
            else if (!Target.CompareTag("Enemy")) { Target = null; ChangeTime = 0.5f; }
            else
            {
                if (tmp) IsAddForce = false;
                else if (Vector3.Magnitude(cnt) < 5 && !GapChange) { rigid.velocity = Sub * 10; IsAddForce = false; GapChange = true; }
            }

            if(IsAddForce) rigid.AddForce(Sub, ForceMode2D.Impulse);
        }

        float j = Vector3.Magnitude(rigid.velocity);


        if (j >  15) rigid.velocity = rigid.velocity.normalized * 15;

        transform.rotation = Quaternion.FromToRotation(Vector2.down, rigid.velocity);

    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            if (!AS.isPlaying) AS.Play();
            BI.Damage = Mathf.FloorToInt((1 + GameManager.instance.PlayerStatus.attack + GameManager.instance.PlayerStatus.attackspeed + Rosmon.AttackRatio + Rosmon.ReinforceAmount[0]) * 30);
            GameManager.instance.BM.MakeMeele(BI,0.3f,collision.transform.position,Vector3.zero,0,false);
            if (collision.transform == Target && !tmp) { tmp = true; rigid.AddForce(Vector2.right); }
        }
    }

    [SerializeField] float AttackGap = 0.2f;

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.transform == Target) Invoke("TestT",AttackGap);
    }

    void TestT()
    {
        tmp = false;
        GapChange = false;
    }

    private void OnEnable()
    {
        AIM.StartMaking();
    }
}
