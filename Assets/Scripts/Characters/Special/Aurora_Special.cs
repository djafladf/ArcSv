using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Aurora_Special : MonoBehaviour
{
    [SerializeField] Aurora main;
    BulletInfo BI;
    IEnumerator EAE()
    {
        if(BI == null) BI = new BulletInfo(0, false, 0, debuffs: new DeBuff(ice: 2.5f), dealFrom: main.player.Id);
        OnEnemy.Clear();
        while (true)
        {
            BI.Damage = (int)(main.player.InitDefense * (1 + main.player.DefenseRatio + GameManager.instance.PlayerStatus.defense + main.player.ReinforceAmount[1]) * 0.5f);
            TmpEnemy.AddRange(OnEnemy);
            foreach (var j in TmpEnemy)j.OnDamage(inf: BI);
            TmpEnemy.Clear();
            yield return GameManager.DotFiveSec;
        }
    }
    private void OnEnable()
    {
        StartCoroutine(EAE());
    }

    HashSet<Enemy> OnEnemy = new();
    List<Enemy> TmpEnemy = new();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        OnEnemy.Add(GameManager.instance.ES.InstanceTo[collision.gameObject]);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        OnEnemy.Remove(GameManager.instance.ES.InstanceTo[collision.gameObject]);
    }

}
