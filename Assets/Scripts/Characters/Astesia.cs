using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Astesia : PlayerSetting
{
    [SerializeField] GameObject Weapon;
    [SerializeField] Sprite Attack1;
    [SerializeField] Sprite Attack2;
    [SerializeField] Sprite Bullet;

    [SerializeField] BulletLine BL;

    Sprite AttackIm;

    public bool IsTest = false;

    protected override void Awake()
    {
        player.InitDefense = 40;
        base.Awake();
        AttackIm = Attack1;
        player.SubEffects.AddRange(Weapon.transform.GetComponentsInChildren<SpriteRenderer>());
    }

    Vector3 DropGap = new Vector3(-2, 8, 0);
    Vector3 DropDir = new Vector3(2, -8, 0).normalized;
    


    protected override void AttackMethod()
    {

        if (TargetPos != null && gameObject.activeSelf)
        {
            NormalInfo.Damage = (int)((1 + GameManager.instance.PlayerStatus.attack + player.AttackRatio + player.ReinforceAmount[0] + GameManager.instance.PlayerStatus.defense + player.DefenseRatio + player.ReinforceAmount[1]) * DamageRatio * 10);
            GameManager.instance.BM.MakeMeele(NormalInfo, 0.3f,transform.position, -player.Dir, 0, false, im: AttackIm);
            if (player.WeaponLevel == 7)
            {
                StartCoroutine(LateDamage(0.25f));
                GameManager.instance.BM.MakeEffect(0.25f, TargetPos.position + DropGap, DropDir, 30, Bullet, BL: BL);
            }
        }
    }

    IEnumerator LateDamage(float Time)
    {
        yield return new WaitForSeconds(Time);
        GameManager.instance.BM.MakeMeele(NormalInfo,0.3f,TargetPos.position, Vector3.zero, 0, false);
    }

    float DamageRatio = 1f;

    protected override void EndBatch()
    {
        base.EndBatch();
        if(player.WeaponLevel == 7) Weapon.SetActive(true);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        Weapon.SetActive(false);
    }

    protected override int WeaponLevelUp(bool IsUp = true)
    {
        switch (player.WeaponLevel++)
        {
            case 1: DamageRatio += 1f; break;
            case 2: player.InitDefense += 5; break;
            case 3: player.AttackSpeed *= 1.2f; player.ChangeOccur = true; break;
            case 4: DamageRatio += 1f; break;
            case 5: player.InitDefense += 10; break;
            case 6: DamageRatio += 1.5f; Weapon.SetActive(true); AttackIm = Attack2; break;
        }
        return player.WeaponLevel;
    }

}
