using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagnetField : MonoBehaviour
{
    [SerializeField] List<Transform> Rounder;
    [SerializeField] GameObject Field;
    float[] Thetas = new float[3];
    CircleCollider2D col;

    public void SetActive(Transform target)
    {
        Field.SetActive(true);
        if (TargetFindCor != null) StopCoroutine(TargetFindCor); TargetFindCor = null;
        if (AttackCor != null) StopCoroutine(AttackCor); AttackCor = null;
        if (SpinCor != null) StopCoroutine(SpinCor); SpinCor = null;

        gameObject.SetActive(true);
        Thetas[0] = 30 * Mathf.Deg2Rad; Thetas[1] = 150 * Mathf.Deg2Rad; Thetas[2] = 270 * Mathf.Deg2Rad;
        for (int i = 0; i < 3; i++)
        {
            Rounder[i].SetParent(transform);
            Rounder[i].localPosition = new Vector3(Mathf.Cos(Thetas[i]) * 3f, Mathf.Sin(Thetas[i]) * 2.5f);
            Rounder[i].gameObject.SetActive(true);
        }
        if (target != null) 
        { 
            var scr = GameManager.instance.ES.InstanceTo[target.gameObject];
            if (scr.IsLive) { CurTarget = target; CurScr = scr; transform.position = CurTarget.position; }
            else { CurTarget = null; TargetFindCor = StartCoroutine(FindTarget()); }
        }
        else { CurTarget = null; TargetFindCor = StartCoroutine(FindTarget()); }
        AttackCor = StartCoroutine(Attack());
        SpinCor = StartCoroutine(Spin());
    }

    private void ResetTarget()
    {
        Thetas[0] = 30 * Mathf.Deg2Rad; Thetas[1] = 150 * Mathf.Deg2Rad; Thetas[2] = 270 * Mathf.Deg2Rad;
        for (int i = 0; i < 3; i++)
        {
            Rounder[i].gameObject.SetActive(false);
            Rounder[i].localPosition = new Vector3(Mathf.Cos(Thetas[i]) * 3f, Mathf.Sin(Thetas[i]) * 2.5f);
            Rounder[i].gameObject.SetActive(true);
        }
        Field.SetActive(true);
        transform.position = CurTarget.position;
        if (AttackCor != null) StopCoroutine(AttackCor); AttackCor = StartCoroutine(Attack(false));
        if (SpinCor != null) StopCoroutine(SpinCor); SpinCor = StartCoroutine(Spin());
    }

    public void EndMod()
    {
        StopAllCoroutines(); TargetFindCor = null; AttackCor = null; SpinCor = null;
        CurTarget = null; CurScr = null;
        foreach (var j in Rounder) j.SetParent(main.transform);
        foreach (var j in main.anims) j.SetTrigger("Dead");
        gameObject.SetActive(false);
    }

    private void Awake()
    {
        col = GetComponent<CircleCollider2D>();
        fil.useLayerMask = true; fil.layerMask = 1 << 6;
    }

    Vector3 Dir = Vector3.one;
    IEnumerator Spin()
    {
        yield return GameManager.OneSec;
        while (true)
        {
            yield return GameManager.FrameWFS;
            speed = Mathf.Deg2Rad * Time.deltaTime * speedvar;
            if (CurTarget != null)
            {
                if (CurScr.IsLive)
                {
                    Dir = (CurTarget.position - transform.position).normalized * Time.deltaTime * 7.5f;
                    transform.position += Dir;
                }
                else if (TargetFindCor == null) { CurTarget = null; TargetFindCor = StartCoroutine(FindTarget()); }
            }
            else if (TargetFindCor == null) { TargetFindCor = StartCoroutine(FindTarget()); }
            for (int i = 0; i < 3; i++)
            {
                Thetas[i] += speed; if (Thetas[i] >= PI2) Thetas[i] -= PI2;
                if (Thetas[i] >= 0 && Thetas[i] <= Mathf.PI) main.Floats[i].sortingOrder = 4;
                else main.Floats[i].sortingOrder = 5;
                Rounder[i].localPosition = new Vector3(Mathf.Cos(Thetas[i]) * 3f, Mathf.Sin(Thetas[i]) * 2.5f);
            }

        }
    }

    Transform CurTarget = null;
    Enemy CurScr = null;

    [SerializeField] Delphin main;
    BulletInfo BI;

    Collider2D[] hits = new Collider2D[100];
    ContactFilter2D fil = new ContactFilter2D();

    IEnumerator Attack(bool IsInit = true)
    {
        if (BI == null) BI = new BulletInfo(0, false, 0, dealFrom: main.player.Id);
        yield return wfs;
        if (!IsInit)
        {
            OnEnemy.Clear();
            int res = col.OverlapCollider(fil, hits) - 1;

            BI.Damage = Mathf.FloorToInt((1 + main.player.AttackRatio + GameManager.instance.PlayerStatus.attack + main.player.ReinforceAmount[0]) * 30);
            for (; res >= 0; res--)
            {
                Enemy sc = GameManager.instance.ES.InstanceTo[hits[res].gameObject]; sc.OnDamage(inf: BI);
                OnEnemy.Add(sc);
            }
        }
        yield return wfs2;
        if (IsInit)
        {
            OnEnemy.Clear();
            int res = col.OverlapCollider(fil, hits) - 1;
            for (; res >= 0; res--) OnEnemy.Add(GameManager.instance.ES.InstanceTo[hits[res].gameObject]);
        }
        
        while (true)
        {
            BI.Damage = Mathf.FloorToInt((1 + main.player.AttackRatio + GameManager.instance.PlayerStatus.attack + main.player.ReinforceAmount[0]) * 10);
            TmpEnemy.AddRange(OnEnemy);
            foreach (var j in TmpEnemy) j.OnDamage(inf: BI);
            TmpEnemy.Clear();
            yield return GameManager.DotFiveSec;
        }
    }

    Coroutine AttackCor = null;
    Coroutine SpinCor = null;
    Coroutine TargetFindCor = null;

    WaitForSeconds wfs = new WaitForSeconds(0.7f);
    WaitForSeconds wfs2 = new WaitForSeconds(0.3f);
    IEnumerator FindTarget()
    {
        while (CurTarget == null)
        {
            CurTarget = GameManager.GetNearest(20, main.transform.position, 1 << 6);
            if (CurTarget != null) if (!GameManager.instance.ES.InstanceTo[CurTarget.gameObject].IsLive) CurTarget = null;
            if (CurTarget == null) yield return GameManager.DotOneSec;
        }
        CurScr = GameManager.instance.ES.InstanceTo[CurTarget.gameObject];
        
        if((transform.position - CurTarget.position).sqrMagnitude > 25)
        {
            foreach (var j in main.anims) j.SetTrigger("Dead");
            if(AttackCor != null) StopCoroutine(AttackCor); 
            AttackCor = null; 
            if(SpinCor != null) StopCoroutine(SpinCor);
            SpinCor = null;
            Field.SetActive(false);
            yield return wfs;
            main.SpecAttack();
            ResetTarget();
        }
        TargetFindCor = null;
    }


    public float speedvar = 10;
    float speed = Mathf.Deg2Rad;
    float PI2 = Mathf.PI * 2;

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
