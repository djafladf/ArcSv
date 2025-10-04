using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platinum :PlayerSetting
{
    [SerializeField] Sprite Bullet;
    [SerializeField] Sprite Bullet2;
    [SerializeField] Sprite SpecBul;
    [SerializeField] ParticleSystem OneGi;
    [SerializeField] Transform Mark;
    [SerializeField] BulletLine BL;
    [SerializeField] BulletLine BL2;

    bool SpecAble = false;

    protected override void Start()
    {
        base.Start();
        NormalInfo.IgnoreDefense = DefenseIgnore;
        SpecInfo = new BulletInfo(0, false, 0, 1.5f, ignoreDefense:0.5f,dealFrom: NormalInfo.DealFrom);
    }

    protected override void AttackMethod()
    {
        if (TargetPos != null)
        {
            Vector2 Sub = (TargetPos.position - transform.position).normalized;
            float rad = Vector2.Angle(Vector2.right, Sub) * Mathf.Deg2Rad;
            if (Sub.y < 0) rad = Mathf.PI * 2 - rad;
            NormalInfo.Damage = (int)((1 + GameManager.instance.PlayerStatus.attack + player.AttackRatio + player.ReinforceAmount[0]) * DamageRatio * 10);
            for (int i = -ProjNum+1; i <= ProjNum-1; i++)
            {
                GameManager.instance.BM.MakeBullet( NormalInfo,Penetrate,
                transform.position, new Vector3(Mathf.Cos(rad + 0.25f * i), Mathf.Sin(rad + 0.25f * i)),
                15, false,Bullet,BL:BL);
            }

            if (SpecAble) { Target = TargetPos.transform.position; player.anim.SetTrigger("Spec"); if(SpecTime == 0) SpecTime = 5; }
        }
    }

    int SpecTime = 0;
    protected override void AttackEnd()
    {
        base.AttackEnd();

        if (TargetPos != null && SpecTime > 0) { player.anim.SetTrigger("Spec"); Target = TargetPos.transform.position; }
    }

    Vector3 Target;

    void ShowerSetting()
    {
        if (player.sprite.flipX) GameManager.instance.BM.MakeEffect(0.3f, transform.position, new Vector3(1, 1, 0), 25, SpecBul, BL: BL2);
        else GameManager.instance.BM.MakeEffect(0.5f, transform.position, new Vector3(-1, 1, 0), 25, SpecBul, BL: BL2);
        if(gameObject.activeSelf) StartCoroutine(RainSub());
    }
    


    Vector3 OneO = new Vector3(-2, 0.84f, 0);
    Vector3 OneT = new Vector3(2, 0.84f, 0);
    void StartOnegi()
    {
        if (player.sprite.flipX) OneGi.gameObject.transform.localPosition = OneT;
        else OneGi.gameObject.transform.localPosition = OneO;

        Mark.gameObject.SetActive(true);
        
        Mark.transform.parent = TargetPos; Mark.transform.localPosition = Vector3.up * 2;

        OneGi.Play();
    }

    void StopOnegi()
    {
        OneGi.Stop();
    }


    BulletInfo SpecInfo;
    IEnumerator RainSub()
    {
        yield return new WaitForSeconds(0.3f);
        SpecInfo.Damage = (int)((1 + GameManager.instance.PlayerStatus.attack + player.AttackRatio + player.ReinforceAmount[0]) * DamageRatio * 15);
        var cnt = GameManager.GetNearest(5, 7, Target, targetLayer);
        Vector3 Gap = new Vector3(0, 15, 0);
        for (int i = 0; i < 7; i++)
        {
            Vector3 RandomSub = Target + Gap;
            if (cnt.Count != 0) RandomSub = cnt[Random.Range(0, cnt.Count)].position + Gap;
            GameManager.instance.BM.MakeMeele(SpecInfo,0.4f,RandomSub,Vector3.down,30,false,Bullet2,delay:0.3f);
            yield return GameManager.DotOneSec;
        }
        Mark.gameObject.SetActive(false);


        if (--SpecTime == 0)
        {
            SpecAble = false;
            yield return new WaitForSeconds(10);
            SpecAble = true;
        }
    }


    protected override void OnEnable()
    {
        base.OnEnable();
        SpecAble = player.WeaponLevel >= 7;
        Mark.gameObject.SetActive(false);
        OneGi.Stop();
    }

    float DamageRatio = 3f;
    float DefenseIgnore = 0;
    int Penetrate = 0;
    int ProjNum = 1;

    protected override int WeaponLevelUp(bool IsUp = true)
    {
        switch (player.WeaponLevel)
        {
            case 1: DamageRatio++; break;
            case 2: Penetrate++; break;
            case 3: DamageRatio ++; break;
            case 4: ProjNum++; break;
            case 5: DamageRatio ++; break;
            case 6:
                Penetrate = 100;
                DefenseIgnore = 0.5f; NormalInfo.IgnoreDefense = 0.5f;
                SpecAble = true;
                break;
        }
        return player.WeaponLevel;
    }
}
