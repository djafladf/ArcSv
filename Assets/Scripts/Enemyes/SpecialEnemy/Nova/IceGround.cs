using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class IceGround : MonoBehaviour
{
    [SerializeField] List<ParticleSystem> PS;
    Animator anim;
    SpriteRenderer sprite;
    FrostNova nova;

    public void Init(FrostNova Nova)
    {
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        nova = Nova;
    }

    private void OnTriggerEnter(Collider other)
    {
        
    }


    protected void OnTriggerExit2D(Collider2D collision)
    {

    }

    protected void BatchAf()
    {
        PS[0].Play(); PS[1].Play();
    }

    protected void BatchAttack()
    {
        int CurDm = Mathf.FloorToInt(30 * nova.ReturnDam());
        GameManager.instance.BM.MakeMeele(new BulletInfo(CurDm, false, 0,scalefactor:4f,debuffs:nova.DeBuffTwo), 0.2f, transform.position, Vector3.zero, 0, true);
        StartCoroutine(Attack());
    }

    protected void BatchEnd()
    {
        anim.enabled = false;
    }

    protected IEnumerator Attack()
    {
        while (nova.gameObject.activeSelf)
        {
            yield return GameManager.OneSec;
            int CurDm = Mathf.FloorToInt(5 * nova.ReturnDam());
            GameManager.instance.BM.MakeMeele(new BulletInfo(CurDm, false, 0,scalefactor:4f,debuffs:nova.DeBuffOne), 0.2f, transform.position, Vector3.zero, 0, true);
        }
        gameObject.SetActive(false);
    }
}
