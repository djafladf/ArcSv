using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class AttackRange_Enemy : MonoBehaviour
{
    Enemy enemy;

    HashSet<GameObject> Targets = new();
    CircleCollider2D Col;

    private void Awake()
    {
        Col = GetComponent<CircleCollider2D>();
        enemy = GetComponentInParent<Enemy>();
        fil.useLayerMask = true; fil.layerMask = (1 << 7);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (Targets.Add(collision.gameObject))
            {
                enemy.BeginAttack = true;
                ind = GameManager.instance.ObjToInd[collision.gameObject];
                if (enemy.Target == null || GameManager.instance.UM.IsPriorityAttack[ind]) enemy.Target = collision.transform;
            }
        }
    }

    int ind;
    float MinDist,dist;
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.CompareTag("Player_Hide"))
        {
            if (Targets.Remove(collision.gameObject))
            {
                if (Targets.Count == 0) { enemy.BeginAttack = false; enemy.Target = null; }
                else 
                {
                    MinDist = float.MaxValue;
                    foreach (var j in Targets) if (j.CompareTag("Player_Hide")) Targets.Remove(j);
                    foreach (var j in Targets)
                    {
                        dist = (transform.position - j.transform.position).sqrMagnitude * 1000;
                        if (GameManager.instance.UM.IsPriorityAttack[GameManager.instance.ObjToInd[j]]) dist *= 0.001f;
                        if (dist < MinDist) { MinDist = dist; enemy.Target = j.transform; }
                    }
                }
            }
        }
    }

    Collider2D[] hits = new Collider2D[5]; // 짜피 Player는 많아야 5임
    ContactFilter2D fil = new ContactFilter2D();
    private void OnEnable()
    {
        Targets.Clear();
        int res = Col.OverlapCollider(fil,hits);
        MinDist = float.MaxValue;
        for(int i = 0; i < res; i++)
        {
            Targets.Add(hits[i].gameObject);
            ind = GameManager.instance.ObjToInd[hits[i].gameObject];
            dist = (transform.position - hits[i].transform.position).sqrMagnitude * 1000;
            if (GameManager.instance.UM.IsPriorityAttack[ind]) dist *= 0.001f;
            if(dist < MinDist) { enemy.Target = hits[i].transform; enemy.BeginAttack = true; }
        }
    }
}