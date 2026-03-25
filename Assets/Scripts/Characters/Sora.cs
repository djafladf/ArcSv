using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Sora : PlayerSetting
{
    [SerializeField] ParticleSystem Norm;
    [SerializeField] ParticleSystem Spec;
    [SerializeField] ParticleSystem Field1, Field2;
    [SerializeField] List<Image> Synthe;
    [SerializeField] GameObject Display;

    [SerializeField] GameObject FlyOne;
    [SerializeField] GameObject FlyTwo;

    [SerializeField] List<Sprite> ETC;

    AudioSource As;


    bool SpecO = false;

    protected override void Awake()
    {
        base.Awake();
        As = GetComponent<AudioSource>();
        yPos = Display.transform.localPosition;
        xPos = new Vector3(-yPos.x, yPos.y);
    }

    public override void ExternInit()
    {
        base.ExternInit();
        player.SubEffects.Add(transform.GetChild(1).GetComponent<SpriteRenderer>());
        player.SubEffects.Add(FlyOne.GetComponent<SpriteRenderer>());
        player.SubEffects.Add(FlyTwo.GetComponent<SpriteRenderer>());
    }

    Vector3 xPos = new Vector3(-0.2f, 3, -1);
    Vector3 yPos = new Vector3(0.2f, 3, -1);
    protected override void Start()
    {
        base.Start();
        NormalInfo.Buffs = new Buff(last: 0.2f, heal: 0, attack: 0.1f, defense: 0.1f);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        foreach (var k in Synthe) k.gameObject.SetActive(false);
        Field1.Stop(); Field2.Stop();
    }

    new protected void FixedUpdate()
    {
        player.rigid.velocity = Vector2.zero;
        if (CanMove && !OnIce && player.AllowMove)
        {
            if (player.IsFollow && player.AllowFollow)
            {
                TargetPos = GameManager.instance.Git.transform;
                player.Dir = (TargetPos.position - transform.position).normalized;
                if (Vector3.Distance(transform.position, TargetPos.position) <= 2f) player.IsFollow = false;
            }
            else
            {
                var Test = GameManager.GetNearest(scanRange, 2, transform.position, targetLayer);
                TargetPos = null;
                foreach (var k in Test) if (k != transform) TargetPos = k;
                if (TargetPos != null)
                {
                    float ChangeRange = AttackRange;
                    if (TargetPos.position.y > transform.position.y)
                    {
                        float xgap = Mathf.Abs(TargetPos.position.x - transform.position.x);
                        if (xgap < 2) ChangeRange = AttackRange * 0.5f;
                        else if (xgap < 4) ChangeRange = AttackRange * 0.8f;
                    }
                    if (Vector3.Distance(transform.position, TargetPos.position) <= ChangeRange) CanMove = false;
                    player.Dir = (TargetPos.position - transform.position).normalized;
                }
                else player.Dir = Vector2.zero;
            }
            Vector2 nextVec = player.Dir * player.speed * (1 + player.SpeedRatio + GameManager.instance.PlayerStatus.speed) * Time.fixedDeltaTime;
            if (nextVec.Equals(Vector2.zero))
            {
                player.anim.SetBool("IsWalk", false);
            }
            else
            {
                if (player.Dir.x > 0 && !player.sprite.flipX)
                {
                    Display.transform.localPosition = xPos;
                    player.sprite.flipX = true;
                    foreach (var k in player.SubEffects) k.flipX = true;
                }
                else if (player.Dir.x < 0 && player.sprite.flipX)
                {
                    Display.transform.localPosition = yPos;
                    player.sprite.flipX = false;
                    foreach (var k in player.SubEffects) k.flipX = false;
                }
                player.anim.SetBool("IsWalk", true);
                player.rigid.MovePosition(player.rigid.position + nextVec);
            }
        }
        else if (TargetPos != null)
        {
            float ChangeRange = AttackRange;
            if (TargetPos.position.y > transform.position.y)
            {
                float xgap = Mathf.Abs(TargetPos.position.x - transform.position.x);
                if (xgap < 2) ChangeRange = AttackRange * 0.5f;
                else if (xgap < 4) ChangeRange = AttackRange * 0.8f;
            }
            if (Vector3.Distance(transform.position, TargetPos.position) > ChangeRange) CanMove = true;
        }
    }



    protected override void EndBatch()
    {
        base.EndBatch();
        if (gameObject.activeSelf)
        {
            StartCoroutine(SyntheEffect());
            StartCoroutine(FieldEffect());
            Field1.Play(); Field2.Play();
        }
        Norm.Play();
        foreach (var k in Synthe) k.gameObject.SetActive(true);
        if (player.WeaponLevel >= 7) { FlyOne.SetActive(true); FlyTwo.SetActive(true); }
    }

    void Emit()
    {
        if (SpecO) Spec.textureSheetAnimation.SetSprite(0, ETC[1]);
        else Spec.textureSheetAnimation.SetSprite(0, ETC[0]);
        SpecO = SpecO == false;
        Spec.Play();
    }

    IEnumerator FieldEffect()
    {
        while (true)
        {
            NormalInfo.Buffs.Heal = (int)Mathf.Round((1 + GameManager.instance.PlayerStatus.attack) * HealRatio);
            foreach(var j in GameManager.instance.Prefs)
            {
                if (!j.activeSelf) continue;
                var VectorSub = j.transform.position - transform.position;
                if(VectorSub.x >= BuffRange_x.x && VectorSub.x <= BuffRange_x.y && VectorSub.y >= BuffRange_y.x && VectorSub.y<= BuffRange_y.y) GameManager.instance.GetScript(j).SetBuff(NormalInfo);
            }
            yield return GameManager.DotQuarter;
        }
    }


    Vector2 BuffRange_x = new Vector2(-13f, 13f);
    Vector2 BuffRange_y = new Vector2(-14f, 4f);
    IEnumerator SyntheEffect()
    {
        while (true)
        {
            foreach (var k in Synthe)
            {
                int j = Mathf.FloorToInt(k.fillAmount * 10);
                if (Random.Range(0, j) < 3 && k.fillAmount < 1) k.fillAmount += 0.1f;
                else if (k.fillAmount > 0) k.fillAmount -= 0.1f;
                else k.fillAmount += 0.1f;
            }
            yield return GameManager.DotQuarter;
        }
    }


    float HealRatio = 1f;

    protected override int WeaponLevelUp(bool IsUp = true)
    {
        ParticleSystem.MainModule obj = Field1.main,obj2 = Field2.main;
        switch (player.WeaponLevel++)
        {
            case 1: HealRatio += 0.1f; break;
            case 2: obj2.startSize = obj.startSize = 35; BuffRange_x.x = -16.25f; BuffRange_x.y = 16.25f; BuffRange_y.y = 5f; BuffRange_y.x = -17.5f; break;
            case 3: HealRatio += 0.2f; break;
            case 4: obj2.startSize = obj.startSize = 42; BuffRange_x.x = -19.5f; BuffRange_x.y = 19.5f; BuffRange_y.y = 6f; BuffRange_y.x = -21f; break;
            case 5: NormalInfo.Buffs.Attack = 0.15f; NormalInfo.Buffs.Defense = 0.15f; break;
            case 6: NormalInfo.Buffs.Attack = 0.2f; NormalInfo.Buffs.Defense = 0.2f; NormalInfo.DeBuffs = new DeBuff(last: 0.2f, attack: 0.1f, defense: 0.1f); FlyOne.SetActive(true); FlyTwo.SetActive(true); break;
        }
        return player.WeaponLevel;
    }

    protected void PlayAudio()
    {
        As.Play();
    }
}
