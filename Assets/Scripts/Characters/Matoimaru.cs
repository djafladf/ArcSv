using System.Collections.Generic;
using UnityEngine;

public class Matoimaru : PlayerSetting
{
    [SerializeField] Sprite NormalAttack;
    [SerializeField] ParticleSystem pt;
    [SerializeField] List<Sprite> Effects;

    BulletInfo SpecialInfo = new BulletInfo();

    protected override void Start()
    {
        base.Start();
        player.HealRatio = -0.8f;
        SpecialInfo.Copy(NormalInfo);
        SpecialInfo.LayerOrder = 1;
        SpecialInfo.DeBuffs = new DeBuff(last: 2, speed: 0.9f,stun:true);
    }

    protected override void AttackMethod()
    {
        if (TargetPos != null)
        {
            float DamageSub = (1 + GameManager.instance.PlayerStatus.attack + player.AttackRatio + player.ReinforceAmount[0] + GameManager.instance.PlayerStatus.defense + player.ReinforceAmount[1]) * player.anim.GetFloat("AttackSpeed");
            NormalInfo.Damage = (int)(DamageSub * DamageRatio * 10);
            GameManager.instance.BM.MakeMeele(NormalInfo, 0.5f, transform.position, player.Dir, 0, false, NormalAttack);
            if (player.WeaponLevel >= 7) { player.anim.SetBool("IsSpec", true); AttackRange = 5; }
        }
    }

    protected override void EndBatch()
    {
        base.EndBatch();
        player.anim.SetFloat("As", As);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        AttackRange = 3.5f;
    }

    Vector3 SpecPos;
    Vector3 SpecDir;

    void SetSpecPos()
    {
        SpecPos = TargetPos.position + Vector3.down;
        SpecDir = (SpecPos - transform.position).normalized;
    }

    void SpecOne(int type)
    {
        float DamageSub = (1 + GameManager.instance.PlayerStatus.attack + player.AttackRatio + player.ReinforceAmount[0] + GameManager.instance.PlayerStatus.defense + player.ReinforceAmount[1]) * player.anim.GetFloat("AttackSpeed");
        switch (type)
        {
            case 0:
                SpecialInfo.Damage = (int)(DamageSub * 10); SpecialInfo.DeBuffs.Stun = false;
                GameManager.instance.BM.MakeMeele(SpecialInfo, 1f, transform.position + new Vector3(SpecDir.x,-1,0), SpecDir, 0, false, Effects[0]);
                break;
            case 1:
                NormalInfo.DeBuffs = SpecialInfo.DeBuffs; SpecialInfo.DeBuffs.Stun = true;
                NormalInfo.Damage = SpecialInfo.Damage = (int)(DamageSub * 75);
                GameManager.instance.BM.MakeMeele(NormalInfo, 0.5f, transform.position, player.Dir, 0, false, NormalAttack);
                pt.transform.position = SpecPos; pt.Play();
                GameManager.instance.BM.MakeMeele(SpecialInfo, 1f, SpecPos, Vector3.zero, 0, false, Effects[1]);
                break;
            default:
                player.anim.SetBool("IsSpec", false); AttackRange = 3.5f; NormalInfo.DeBuffs = null;
                break;
        }
    }

    float DamageRatio = 2f;
    float As = 1;

    protected override int WeaponLevelUp(bool IsUp = true)
    {
        switch (player.WeaponLevel++)
        {
            case 1: DamageRatio += 1f; break;
            case 2: player.anim.SetFloat("As", 1.1f); As = 1.1f; break;
            case 3: DamageRatio += 1f; break;
            case 4: player.anim.SetFloat("As", 1.2f); As = 1.2f; break;
            case 5: DamageRatio += 1.5f; break;
            case 6: player.anim.SetBool("IsSpec",true);  break;
        }
        return player.WeaponLevel;
    }

}

