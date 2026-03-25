using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenseDrone : Enemy
{
    protected override void OnEnable()
    {
        base.OnEnable();
        
    }
    protected override void FixedUpdate()
    {
        if (Vector3.Magnitude(transform.position - GameManager.instance.player.Self.position) < 10) return;
        base.FixedUpdate();
    }

    IEnumerator Buffs()
    {
        Objs.Clear(); 
        while (gameObject.activeSelf)
        {
            Tmp.Clear();
            Tmp.AddRange(Objs);
            foreach(var j in Tmp) j.OnBuff(BI.Buffs);
            yield return GameManager.DotFiveSec;
        }
    }

    HashSet<Enemy> Objs = new();
    List<Enemy> Tmp = new();

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enem")) Objs.Add(GameManager.instance.ES.InstanceTo[collision.gameObject]);
    }
    protected override void OnTriggerExit2D(Collider2D collision)
    {
        if (!IsLive) return;
        if (collision.CompareTag("Enem")) Objs.Remove(GameManager.instance.ES.InstanceTo[collision.gameObject]);
        base.OnTriggerExit2D(collision);
    }
}
