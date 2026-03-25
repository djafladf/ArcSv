using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Himo : PlayerSetting
{
    [SerializeField] Sprite AttackIm;
    [SerializeField] ParticleSystem PTs;
    [SerializeField] CustomRenderTexture crt;
    [SerializeField] Material RipMat;

    protected override void Awake()
    {
        base.Awake();
        NormalInfo.DeadTrigger = MakeFish;
    }

    protected override void AttackMethod()
    {
        PTs.Play();
        NormalInfo.Damage = (int)((1 + GameManager.instance.PlayerStatus.attack + player.AttackRatio + player.ReinforceAmount[0]) * 20);
        GameManager.instance.BM.MakeMeele(
            NormalInfo, 0.5f, transform.position, player.sprite.flipX ? new Vector2(-1,-1) : Vector2.up, 0, false, AttackIm);
    }

    Vector4 PrefRip = new Vector4(5f, 4.9f, 1, 1);
    void MakeRT(int tp)
    {
        if (RipCor == null) RipCor = StartCoroutine(Rip(PrefRip,true));
    }

    public void MakeRT(Vector3 Pos)
    {
        if (RipCor == null) RipCor = StartCoroutine(Rip(new Vector4((Pos.x - transform.position.x + 7) * xsub, (Pos.y - transform.position.y + 5.25f + 1.5f) * ysub, 1, 1)));
    }
    float xsub = 1f / 14f;      // Width / 2
    float ysub = 1f / 10.5f;     // Height / 2

    IEnumerator RipSubEffect()
    {
        while (gameObject.activeSelf)
        {
            if (RipCor == null) { RipCor = StartCoroutine(Rip(new Vector4(Random.Range(0.2f, 0.8f), Random.Range(0.2f, 0.8f), 1, 1))); }
            yield return GameManager.OneSec;
        }
    }

    Coroutine RipCor = null;
    IEnumerator Rip(Vector4 Pos,bool needReg=false)
    {
        if(needReg) { Pos.x *= xsub; Pos.y *= ysub; }
        RipMat.SetVector("_SpawnPos", Pos);
        yield return GameManager.DotOneSec;
        RipMat.SetVector("_SpawnPos", Vector4.zero);
        RipCor = null;
    }

    int Fishn = 0;
    int FishMax = 2;
    [SerializeField] List<Himo_Creature> Fishs;
    public void MakeFish(Transform pos, int n)
    {
        if (Fishn > FishMax || Random.Range(0, 1f) < 0f) return;
        foreach (var j in Fishs)
        {
            if (!j.gameObject.activeSelf)
            {
                j.transform.position = pos.position;
                j.SetTex(GameManager.instance.ES.InstanceTo[pos.gameObject].spriteRenderer.sprite);
                Fishn++;
                break;
            }
        }
    }

    void AttackPrepEnd() 
    {
        if (player.IsFollow)
        {
            if (CurFollow != null) GameManager.instance.ES.TargetChange[CurFollow][player.Id] = false;
            player.anim.SetBool("IsAttack", false); CurFollow = null; TargetChangeCall = true; return;
        }

        if (TargetChangeCall)   // ‹š¸®´ø ³ð »ç¸Á
        {
            if (CurFollow != null) GameManager.instance.ES.TargetChange[CurFollow][player.Id] = false;
            CurFollow = null;
        }
        else if (Vector3.Distance(transform.position, TargetPos.position) <= AttackRange) return;
        // ‹š¸®´ø ³ð ±ÙÃ³¿¡ ÀÖÀ½

        // ¶§¸®´ø ³ðµµ ¾øÀ½
        TargetPos = GetNearest(AttackRange);
        if (TargetPos != null && !player.IsFollow)
        {
            player.Dir = (TargetPos.position - transform.position).normalized;
            FlipAnim();
            CurFollow = TargetPos.gameObject; TargetChangeCall = false;
            GameManager.instance.ES.TargetChange[CurFollow][player.Id] = true;
        }
        else
        {
            player.anim.SetBool("IsAttack", false); if (CurFollow != null) GameManager.instance.ES.TargetChange[CurFollow][player.Id] = false;
            CurFollow = null; TargetChangeCall = true;
        }
    }

    protected override void EndBatch()
    {
        base.EndBatch();
        if (RipCor != null) StopCoroutine(RipCor); RipCor = null; StartCoroutine(RipSubEffect());
    }

    void AttackAfter()
    {
        CanMove = true;
    }

    void OnDisable()
    {
        foreach (var j in Fishs) j.gameObject.SetActive(false); Fishn = 0;
        crt.Initialize();
    }
}
