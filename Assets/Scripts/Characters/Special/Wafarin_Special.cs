using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class Wafarin_Special : MonoBehaviour
{
    [SerializeField] Sprite Bullet;
    WaitForSeconds ZeroDotFive = new WaitForSeconds(0.5f);
    [SerializeField] LayerMask[] Layers;
    [SerializeField] Player Wafarin;
    BoxCollider2D Coll;

    private void Awake()
    {
        Coll = GetComponent<BoxCollider2D>();
    }

    BulletInfo BI;
    BulletInfo Buff;

    private void Start()
    {
        BI = new BulletInfo(0, false, 0, dealFrom: Wafarin.Id);
        Buff = new BulletInfo(0, false, 0,scalefactor:0.1f,buffs:new Buff(attack:0.2f),dealFrom:BI.DealFrom);
    }

    private void OnEnable()
    {
        StartCoroutine(Attack());
    }

    HashSet<GameObject> OnPlayer = new HashSet<GameObject>();
    List<GameObject> TmpPlayer = new();
    HashSet<GameObject> OnEnemy = new HashSet<GameObject>();
    List<GameObject> TmpEnemy = new();

    IEnumerator Attack()
    {
        OnPlayer.Clear(); OnEnemy.Clear();
        yield return GameManager.DotOneSec;
        for (int i = 0; i < 10; i++)
        {
            TmpPlayer.AddRange(OnPlayer);
            BI.Damage = (int)((1 + GameManager.instance.PlayerStatus.attack + Wafarin.AttackRatio + Wafarin.ReinforceAmount[0]) * 30);
            foreach (var j in TmpPlayer) if(j.activeSelf) GameManager.instance.GetScript(j).SetBuff(Buff);
            TmpPlayer.Clear();

            TmpEnemy.AddRange(OnEnemy);
            foreach (var j in TmpEnemy) if (j.activeSelf)
                {
                    //GameManager.instance.BM.MakeMeele(BI, 0.6f, j.transform.position, Vector3.zero, 0, false, Bullet);
                    GameManager.instance.ES.InstanceTo[j].OnDamage(inf : BI);
                    GameManager.instance.BM.MakeEffect(0.6f, j.transform.position, Vector3.zero, 0, Bullet);
                }
            TmpEnemy.Clear();
            yield return GameManager.DotOneSec;
        }
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.CompareTag("Player_Hide")) OnPlayer.Add(collision.gameObject);
        if (collision.CompareTag("Enemy")) OnEnemy.Add(collision.gameObject);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.CompareTag("Player_Hide")) OnPlayer.Remove(collision.gameObject);
        if (collision.CompareTag("Enemy")) OnEnemy.Remove(collision.gameObject);
    }
}
