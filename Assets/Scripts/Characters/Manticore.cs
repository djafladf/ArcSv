using System.Collections;
using UnityEngine;

public class Manticore : PlayerSetting
{
    [SerializeField] GameObject HideObj;

    Coroutine Hide = null;

    [SerializeField] Sprite[] Bullets;

    protected override void Start()
    {
        base.Start();
        SpecInfo = new BulletInfo(0, false, 0, NormalInfo.DealFrom);
    }

    protected override void AttackMethod()
    {
        if (TargetPos != null && gameObject.activeSelf)
        {
            if (player.WeaponLevel < 7)
                StartCoroutine(Locker());
            else
                StartCoroutine(UpLocker());
            
            if (Hide == null) Hide = StartCoroutine(HideTime());
            else { HideReinforce = 10; StopCoroutine(Hide); Hide = StartCoroutine(HideTime()); }
        }
    }

    IEnumerator Locker()
    {
        Vector3 z = (TargetPos.position - transform.position).normalized * 2;

        NormalInfo.Damage = (int)((1 + GameManager.instance.PlayerStatus.attack + player.AttackRatio + player.ReinforceAmount[0]) * DamageRatio * HideReinforce);
        for (int i = 1; i < 6; i++)
        {
            GameManager.instance.BM.MakeMeele(NormalInfo, 0.5f, transform.position + z * i, Vector3.zero, 0, false, Bullets[0]);
            yield return new WaitForSeconds(0.1f);
        }
    }

    BulletInfo SpecInfo;
    IEnumerator UpLocker()
    {
        NormalInfo.Damage = (int)((1 + GameManager.instance.PlayerStatus.attack + player.AttackRatio + player.ReinforceAmount[0]) * DamageRatio * HideReinforce);
        GameManager.instance.BM.MakeMeele(NormalInfo, 0.5f, transform.position, Vector3.zero, 0, false, Bullets[2]);
        yield return new WaitForSeconds(0.2f);
        SpecInfo.Damage = NormalInfo.Damage * 2;
        GameManager.instance.BM.MakeMeele(SpecInfo, 0.5f, transform.position, Vector3.zero, 0, false, Bullets[1]);
    }

    float HideReinforce = 10;
    IEnumerator HideTime()
    {
        HideObj.SetActive(false);
        tag = "Player";
        yield return new WaitForSeconds(3);
        tag = "Player_Hide";
        HideObj.SetActive(true);
        Hide = null; HideReinforce = 13f;
    }


    protected override void EndBatch()
    {
        base.EndBatch(); HideObj.SetActive(true); tag = "Player_Hide";

    }                  

    protected override void OnEnable()
    {
        base.OnEnable(); HideObj.SetActive(false);
    }

    float DamageRatio = 1.5f;
    protected override int WeaponLevelUp()
    {
        switch (player.WeaponLevel++)
        {
            case 1: DamageRatio += 0.5f; break;
            case 2: DamageRatio += 0.5f; break;
            case 3: player.AttackSpeed *= 1.2f; player.ChangeOccur = true; break;
            case 4: DamageRatio += 0.75f; break;
            case 5: DamageRatio += 0.75f; break;
            case 6: break;
        }
        return player.WeaponLevel;
    }

}

