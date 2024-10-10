using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class Gum : PlayerSetting
{
    [SerializeField] Sprite Meat;
    [SerializeField] Sprite FailMeat;
    [SerializeField] Transform GetField;
    [SerializeField] GameObject MeatCount;
    [SerializeField] TMP_Text MeatCountText;

    [SerializeField] List<ParticleSystem> Particles;


    int MeatNum = 0;
    int ActCount = 0;

    int ItemCount = -1;
    string ItemName;
    bool OnSkill = false;
    BulletInfo Injure = new BulletInfo(0,false,0);

    protected override void Awake()
    {
        base.Awake();
        ItemCount = GameManager.instance.IM.MakeExternalItem(Meat, 15, 0.1f, 1); ItemName = $"{ItemCount}";
        GetField.tag = $"ExternalArea{ItemCount}";
        player.InitDefense = 40; player.InitHP = 500;
    }

    protected override void Start()
    {
        base.Start();
        GameManager.instance.IM.UpdateExternalItem(ItemCount, Target: name[0] - '0');
        Injure.DealFrom = NormalInfo.DealFrom;
        //for (int i = 0; i < 7;i++) WeaponLevelUp();
    }

    protected override void AttackMethod()
    {
        if (ActCount == 2)
        {
            NormalInfo.Damage = Mathf.FloorToInt((1 + GameManager.instance.PlayerStatus.attack + player.AttackRatio + player.ReinforceAmount[0]) * 19.5f);
            GameManager.instance.BM.MakeMeele(
                NormalInfo, 0.2f, TargetPos.position, Vector3.zero, 0, false, null);

            Injure.Damage = Mathf.FloorToInt(player.MaxHP * 0.03f);
            GetDamage(Injure, transform);
            ActCount = 0;
        }
        else
        {
            NormalInfo.Damage = Mathf.FloorToInt((1 + GameManager.instance.PlayerStatus.attack + player.AttackRatio + player.ReinforceAmount[0]) * 15);
            GameManager.instance.BM.MakeMeele(
                NormalInfo, 0.2f, TargetPos.position, Vector3.zero, 0, false, null);
            ActCount++;
        }
    }

    protected override void GetDamage(BulletInfo Info, Transform DamageFrom)
    {
        if (OnSkill) Info.Damage = Mathf.FloorToInt(Info.Damage * 0.5f);
        base.GetDamage(Info, DamageFrom);
    }

    protected void SkillJudge()
    {
        Particles[0].Stop(); Particles[1].transform.localPosition = (player.sprite.flipX ? new Vector3(2.7f, 0.45f) : new Vector3(-2.7f, 0.45f)); Particles[1].Play(); 
        float j = Random.Range(0, 1f);
        if (j < 0.05f) player.anim.SetTrigger("SkillFail");
        else if (j < 0.9f) player.anim.SetTrigger("SkillNormal");
        else player.anim.SetTrigger("SkillSuccess");
    }

    protected void SkillSuccess(int IsGreat)
    {
        if(IsGreat == 1)
        {
            
        }
        else
        {

        }
        SkillEnd();
    }

    protected void SkillFail()
    {
        if (ActCount == 2)
        {
            Injure.Damage = Mathf.FloorToInt((1 + GameManager.instance.PlayerStatus.attack + player.AttackRatio + player.ReinforceAmount[0]) * 26);
            GameManager.instance.BM.MakeBullet(Injure, 0, transform.position + Vector3.up, player.sprite.flipX ? Vector3.left : Vector3.right, 30, false,FailMeat);
            NormalInfo.Damage = Mathf.FloorToInt(player.MaxHP * 0.03f);
            GetDamage(NormalInfo, transform);
            ActCount = 0;
        }
        else
        {
            Injure.Damage = Mathf.FloorToInt((1 + GameManager.instance.PlayerStatus.attack + player.AttackRatio + player.ReinforceAmount[0]) * 20);
            GameManager.instance.BM.MakeBullet(Injure, 0, transform.position + Vector3.up, player.sprite.flipX ? Vector3.left : Vector3.right, 30, false,FailMeat);
            ActCount++;
        }
        MeatNum = 1;
        SkillEnd();
    }

    protected void SkillEnd()
    {
        MeatNum--; MeatCountText.text = $"{MeatNum}";
        if (MeatNum == 0)
        {
            GetField.localScale = Vector3.one * 3f; OnSkill = false; player.anim.SetBool("SkillEnd", true); CanMove = true;
        }
    }

    protected override int WeaponLevelUp()
    {
        switch (player.WeaponLevel++)
        {
            case 1: player.InitHP += 50; player.ChangeOccur = true; break;
            case 2: GameManager.instance.IM.UpdateExternalItem(ItemCount, prob: 0.2f);  break;
            case 3: player.InitDefense += 10; break;
            case 4: GetField.localScale *= 1.5f; break;
            case 5: player.InitHP += 100; player.ChangeOccur = true; break;
            case 6: MeatCount.SetActive(true); break;
        }
        return player.WeaponLevel;
    }

    protected override void EndBatch()
    {
        base.EndBatch();
        GetField.gameObject.SetActive(true);
        MeatNum = 0;
        if (player.WeaponLevel >= 7) { MeatCount.SetActive(true); MeatCountText.text = $"0"; }
    }


    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if(MeatNum != 0 && !OnSkill)
        {
            Particles[0].Play(); Particles[0].transform.localPosition = (player.sprite.flipX ? new Vector3(2.7f, 0.3f) : new Vector3(-2.7f, 0.3f));
            player.anim.SetBool("SkillEnd", false); player.anim.SetTrigger("SkillOn"); CanMove = false; GetField.localScale = Vector3.one * 100;
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        GetField.gameObject.SetActive(false); MeatCount.SetActive(false); OnSkill = false; if (OnSkill) GetField.localScale = Vector3.one * 3f; ActCount = 0;
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);

        if(collision.CompareTag("Item")) if (collision.name.Equals(ItemName))
            {
                if(player.WeaponLevel >= 7) 
                {
                    if (MeatNum == 0 && !OnSkill) { Particles[0].Play();  Particles[0].transform.localPosition = (player.sprite.flipX ? new Vector3(2.7f, 0.3f) : new Vector3(-2.7f,0.3f));  OnSkill = true;
                        player.anim.SetBool("SkillEnd",false); player.anim.SetTrigger("SkillOn"); CanMove = false; GetField.localScale = Vector3.one * 100; }
                    MeatNum++; MeatCountText.text = $"{MeatNum}";
                }
                else
                {
                    int HealAmount = Mathf.FloorToInt(player.MaxHP * 0.05f * (1 + GameManager.instance.PlayerStatus.heal));
                    Heal(HealAmount); 
                }
                collision.gameObject.SetActive(false);
            }
    }

}
