using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerSetting : MonoBehaviour
{
    [SerializeField] public Player player;
    /*[SerializeField] protected Sprite WeaponIm;
    [SerializeField] protected Sprite HeadIm;*/

    [SerializeField] bool IsPlayer;
    public bool IsSummon = false;
    public bool HasWeapon;

    public BulletInfo NormalInfo = new BulletInfo();
    protected bool OnIce;

    [NonSerialized] public bool CanMove = true;

    protected bool TargetChangeCall = true;

    public void TargetChangeCallAct(int type, int ind)
    {
        if (CurFollow[0] != type || CurFollow[1] != ind) { print($"{type},{ind} : {CurFollow[0]},{CurFollow[1]}"); return; }
        TargetChangeCall = true;
    }

    protected virtual void Awake()
    {
        player.SubEffects.Clear();
        player.Self = transform;
        player.rigid = GetComponent<Rigidbody2D>();
        player.anim = GetComponent<Animator>();
        player.sprite = GetComponent<SpriteRenderer>();
        player.WeaponLevel = 1;
        player.AttackRatio = 0; player.DefenseRatio = 0; player.HPRatio = 0; player.SpeedRatio = 0;
        CanMove = IsPlayer;
        player.CurHP = player.InitHP; player.MaxHP = player.InitHP;

        player.ChangeOccur = true; player.AllowFollow = true; player.AllowMove = true;

        GameManager.instance.ES.TargetChangeAct[player.Id] = TargetChangeCallAct;
        //player.MaxSp = player.CurSP = player.InitSP;
        if (!IsSummon)
        {
            GameManager.instance.RequestOfWeapon(WeaponLevelUp, player.Id);
            
        }

        if (IsPlayer)
        {
#if UNITY_STANDALONE
            GetComponent<PlayerInput>().defaultControlScheme = "Keyboard&Mouse";
#endif
#if UNITY_ANDROID || UNITY_IOS
            GetComponent<PlayerInput>().defaultControlScheme = "Gamepad";
#endif
        }
        gameObject.SetActive(false);
    }

    protected virtual void Start()
    {

    }


    protected int[] CurFollow = { -1, -1 };

    protected virtual void FixedUpdate()
    {
        if (player.ChangeOccur && !IsSummon)
        {
            player.ChangeOccur = false;
            int cnt = player.MaxHP;
            player.MaxHP = Mathf.FloorToInt(player.InitHP * (1 + player.HPRatio + GameManager.instance.PlayerStatus.hp));
            if (cnt - player.MaxHP != 0)
            {
                player.CurHP += player.MaxHP - cnt;
                HPBar.fillAmount = player.CurHP / (float)player.MaxHP;
                if (!IsPlayer) player.MyBatch.HPBar.fillAmount = player.CurHP / (float)player.MaxHP;
                else GameManager.instance.UM.HpChange();
            }
            player.anim.SetFloat("AttackSpeed", player.AttackSpeed + GameManager.instance.PlayerStatus.attackspeed + player.ReinforceAmount[3]);
        }

        player.rigid.velocity = Vector2.zero;

        if (!CanMove || OnIce) return;

        if (!IsPlayer)
        {
            if (player.AllowMove)
            {
                if (player.IsFollow && player.AllowFollow)
                {
                    if (CurFollow[0] != -1) { GameManager.instance.ES.TargetChange[CurFollow[0]][CurFollow[1]][player.Id] = false; CurFollow[0] = -1; }
                    TargetChangeCall = true;
                    TargetPos = GameManager.instance.Git.transform;
                    player.Dir = (TargetPos.position - transform.position).normalized;
                    if (Vector3.Distance(transform.position, TargetPos.position) <= 1.5f) player.IsFollow = false;
                }
                else
                {
                    if (TargetChangeCall) FindTarget();
                    else
                    {
                        if (Vector3.Distance(transform.position, TargetPos.position) <= AttackRange) Attack();
                        player.Dir = (TargetPos.position - transform.position).normalized;
                    }
                }
            }
            else
            {
                player.Dir = Vector2.zero;
                TargetPos = GetNearest(AttackRange);
                if (TargetPos != null) { CurFollow[0] = TargetPos.name[0] - 1; CurFollow[1] = TargetPos.name[1] - 1; TargetChangeCall = false; Attack(); }
            }
        }
        Vector2 nextVec = player.Dir * player.speed * (1 + player.SpeedRatio + GameManager.instance.PlayerStatus.speed + player.ReinforceAmount[2] - player.DeBuffAmount[0]) * Time.fixedDeltaTime;
        if (nextVec.Equals(Vector2.zero))
        {
            player.anim.SetBool("IsWalk", false);
        }
        else
        {
            FlipAnim();
            player.anim.SetBool("IsWalk", true);
            player.rigid.MovePosition(player.rigid.position + nextVec);
        }
    }

    protected virtual void FlipAnim()
    {
        if (player.Dir.x > 0 && !player.sprite.flipX)
        {
            player.sprite.flipX = true;
            foreach (var k in player.SubEffects) k.flipX = true;
        }
        else if (player.Dir.x < 0 && player.sprite.flipX)
        {
            player.sprite.flipX = false;
            foreach (var k in player.SubEffects) k.flipX = false;
        }
    }

    protected virtual void StopMoving()
    {

    }
    protected virtual void StartMoving()
    {

    }

    protected virtual void FindTarget()
    {
        TargetPos = GetNearest(scanRange);
        if (TargetPos != null)
        {
            if (Vector3.Distance(transform.position, TargetPos.position) <= AttackRange) Attack();
            player.Dir = (TargetPos.position - transform.position).normalized;
            CurFollow[0] = TargetPos.name[0] - 1; CurFollow[1] = TargetPos.name[1] - 1; TargetChangeCall = false;
            GameManager.instance.ES.TargetChange[CurFollow[0]][CurFollow[1]][player.Id] = true;
        }
        else
        {
            player.Dir = Vector2.zero;
            CurFollow[0] = -1; CurFollow[1] = -1; TargetChangeCall = true;
        }
    }

    protected virtual void WeaponAnim()
    {

    }
    protected virtual int WeaponLevelUp()
    {
        return -1;
    }


    // About Assistant ----------------------------------------------

    [SerializeField] protected LayerMask targetLayer;
    [SerializeField] protected float scanRange;
    protected Transform TargetPos = null;

    protected Transform GetNearest(float Range)
    {
        RaycastHit2D[] targets = Physics2D.CircleCastAll(transform.position, Range, Vector2.zero, 0, targetLayer);
        float diffs = scanRange + 10;
        Transform res = null;
        foreach (RaycastHit2D target in targets)
        {
            float curDiff = Vector3.Distance(transform.position, target.transform.position);
            if (curDiff < diffs)
            {
                diffs = curDiff; res = target.transform;
            }
        }

        return res;
    }

    protected AttackType AttackInf;
    [SerializeField] protected float AttackRange;
    protected Transform AttackTarget = null;

    protected virtual void Attack()
    {
        player.anim.SetBool("IsAttack", true);
        CanMove = false;
    }

    protected virtual void AttackEnd()
    {
        if (TargetChangeCall)   // 떄리던 놈 사망
        {
            if(CurFollow[0] != -1)GameManager.instance.ES.TargetChange[CurFollow[0]][CurFollow[1]][player.Id] = false;
            player.anim.SetBool("IsAttack", false); CanMove = true; CurFollow[0] = -1; CurFollow[1] = -1; TargetChangeCall = true; return;
        }
        
        // 떄리던 놈 근처에 있음
        if (Vector3.Distance(transform.position, TargetPos.position) <= AttackRange) return;
        
        // 때리던 놈도 없음
        TargetPos = GetNearest(AttackRange);
        if (TargetPos != null && !player.IsFollow)
        {
            player.Dir = (TargetPos.position - transform.position).normalized;
            FlipAnim();
            CurFollow[0] = TargetPos.name[0] - 1; CurFollow[1] = TargetPos.name[1] - 1; TargetChangeCall = false;
            GameManager.instance.ES.TargetChange[CurFollow[0]][CurFollow[1]][player.Id] = true;
        }
        else
        {
            player.anim.SetBool("IsAttack", false); if (CurFollow[0] != -1) GameManager.instance.ES.TargetChange[CurFollow[0]][CurFollow[1]][player.Id] = false;
            CanMove = true; CurFollow[0] = -1; CurFollow[1] = -1; TargetChangeCall = true;
        }
    }

    protected virtual void AttackMethod()
    {

    }

    protected virtual void EndBatch()
    {
        CanMove = true;
    }

    bool CanHit = true;

    [SerializeField] protected Image HPBar;


    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("EnemyAttack") && CanHit) GetDamage(GameManager.instance.BM.GetBulletInfo(GameManager.StringToInt(collision.name)), collision.transform);
        else if (collision.CompareTag("PlayerBuff"))
        {
            Buff Info = GameManager.instance.BM.GetBulletInfo(GameManager.StringToInt(collision.name)).Buffs;
            int Amount;
            if (Info.Heal != 0)
            {
                Amount = (int)(Info.Heal * (1 + GameManager.instance.PlayerStatus.heal));
                Amount = Math.Min(player.MaxHP - player.CurHP, Amount);
                if (Amount > 0 && player.CurHP < player.MaxHP)
                {
                    Heal(Amount);
                    GameManager.instance.UM.DamageUp(1, GameManager.instance.BM.GetBulletInfo(GameManager.StringToInt(collision.name)).DealFrom, Amount);
                }
            }
            if (player.ReinforceAmount[0] <= Info.Attack && Info.Attack != 0)
            {
                if (player.ReinforceAmount[0] == Info.Attack) player.ReinForceLast[0] = Mathf.Max(player.ReinforceAmount[0], Info.Last);
                else player.ReinForceLast[0] = Info.Last;
                player.ReinforceAmount[0] = Mathf.Max(Info.Attack, player.ReinforceAmount[0]);
            }
            if (player.ReinforceAmount[1] <= Info.Defense && Info.Defense != 0)
            {
                if (player.ReinforceAmount[1] == Info.Defense) player.ReinForceLast[1] = Mathf.Max(player.ReinforceAmount[1], Info.Last);
                else player.ReinForceLast[1] = Info.Last;
                player.ReinforceAmount[1] = Info.Defense;
            }
            if (player.ReinforceAmount[2] <= Info.Speed && Info.Speed != 0)
            {
                if (player.ReinforceAmount[2] == Info.Speed) player.ReinForceLast[2] = Mathf.Max(player.ReinforceAmount[2], Info.Last);
                else player.ReinForceLast[2] = Info.Last;
                player.ReinforceAmount[2] = Info.Speed;
            }
            if (player.ReinforceAmount[3] <= Info.AttackSpeed && Info.AttackSpeed != 0)
            {
                if (player.ReinforceAmount[3] == Info.AttackSpeed) player.ReinForceLast[3] = Mathf.Max(player.ReinforceAmount[3], Info.Last);
                else player.ReinForceLast[3] = Info.Last;
                player.ReinforceAmount[3] = Info.AttackSpeed;
                player.ChangeOccur = true;
            }

        }
    }

    string[] BFTest = { "공", "방", "속", "공속" };
    IEnumerator BuffCheck()
    {
        int i;
        for (i = 0; i < 4; i++) { player.ReinforceAmount[i] = 0; player.ReinForceLast[i] = 0; }
        for (i = 0; i < 5; i++) { player.DeBuffAmount[i] = 0; player.DeBuffLast[i] = 0; if (DeBuffObj[i] != null) { DeBuffObj[i].SetActive(false); Debug.Log(GameManager.instance.BFM); DeBuffObj[i].transform.parent = GameManager.instance.BFM.transform; DeBuffObj[i] = null; } }
        OnIce = false;

        while (true)
        {
            // Buff Check
            for (i = 0; i < 4; i++)
            {
                if (player.ReinForceLast[i] == 0) continue;
                player.ReinForceLast[i] -= 0.1f;
                if (player.ReinForceLast[i] <= 0) { player.ReinForceLast[i] = 0; player.ReinforceAmount[i] = 0; if (i == 3) player.ChangeOccur = true; }
            }

            // DeBuff Check

            // About Ice
            if (player.DeBuffLast[4] > 0)
            {
                player.DeBuffLast[4] -= 0.1f;
                if (player.DeBuffLast[4] <= 0)
                {
                    player.DeBuffLast[4] = 0;
                    DeBuffObj[4].SetActive(false); DeBuffObj[4].transform.parent = GameManager.instance.BFM.transform;
                    DeBuffObj[4] = null; OnIce = false; player.DeBuffAmount[4] = 0; player.anim.enabled = true;
                }
            }
            // About Chill
            if (player.DeBuffAmount[4] > 0)
            {
                player.DeBuffAmount[4] -= 0.05f;
                if (player.DeBuffAmount[4] <= 0)
                {
                    player.DeBuffAmount[4] = 0;
                    DeBuffObj[4].SetActive(false); DeBuffObj[4].transform.parent = GameManager.instance.BFM.transform;
                    DeBuffObj[4] = null; player.DeBuffAmount[0] -= 0.3f;
                }
            }

            yield return GameManager.DotOneSec;
        }
    }

    protected void Heal(int Amount)
    {
        player.CurHP += Amount; GameManager.instance.DM.MakeHealCount(Amount, transform);
        HPBar.fillAmount = player.CurHP / (float)player.MaxHP;
        if (IsPlayer) GameManager.instance.UM.HpChange();
        else if (!IsSummon) { player.MyBatch.HPBar.fillAmount = player.CurHP / (float)player.MaxHP; }
    }


    protected GameObject[] DeBuffObj = new GameObject[5];
    protected virtual void GetDamage(BulletInfo Info, Transform DamageFrom)
    {
        if (player.Unbeat) return;
        int GetDamage = Info.ReturnDamage(player.InitDefense * (1 + player.DefenseRatio + GameManager.instance.PlayerStatus.defense + player.ReinforceAmount[1]));
        GameManager.instance.UM.DamageUp(2, NormalInfo.DealFrom, GetDamage);
        player.CurHP -= GetDamage;
        if (player.CurHP > 0 && Info.DeBuffs != null)
        {
            if (Info.DeBuffs.Ice != 0)
            {
                if (!OnIce)
                {
                    player.DeBuffAmount[4] += Info.DeBuffs.Ice;
                    if (DeBuffObj[4] == null)
                    {
                        DeBuffObj[4] = GameManager.instance.BFM.RequestForDebuff(0);
                        DeBuffObj[4].transform.parent = transform;
                        DeBuffObj[4].transform.localPosition = new Vector3(0, player.sprite.sprite.bounds.size.y * 0.6f, 0);
                        DeBuffObj[4].gameObject.SetActive(true);
                        player.DeBuffAmount[0] += 0.3f;
                    }
                    if (player.DeBuffAmount[4] >= 10)
                    {
                        DeBuffObj[4].SetActive(false); DeBuffObj[4].transform.parent = GameManager.instance.BFM.transform;
                        DeBuffObj[4] = null;

                        DeBuffObj[4] = GameManager.instance.BFM.RequestForDebuff(1, player.sprite.bounds.size.x, player.sprite.bounds.size.y);
                        DeBuffObj[4].transform.parent = transform;
                        DeBuffObj[4].transform.localPosition = Vector3.zero;
                        DeBuffObj[4].gameObject.SetActive(true);
                        OnIce = true; player.DeBuffLast[4] = 1; player.DeBuffAmount[0] -= 0.3f;
                        player.anim.enabled = false;
                    }
                }
                else player.CurHP -= Mathf.FloorToInt(GetDamage * Info.DeBuffs.Ice * 0.1f);
            }
        }

        if (player.CurHP > player.MaxHP) player.CurHP = player.MaxHP;
        else if (player.CurHP <= 0)
        {
            player.CurHP = 0;
            gameObject.SetActive(false);
            GameManager.instance.UM.BatchOrder.Remove(name[0] - '0');
            if (IsPlayer) GameManager.instance.UM.GameFail();
            else if (!IsSummon) player.MyBatch.ReBatch();
        }
        else
        {
            HPBar.fillAmount = player.CurHP / (float)player.MaxHP;
            if (IsPlayer) GameManager.instance.UM.HpChange();
            else if (!IsSummon) { player.MyBatch.HPBar.fillAmount = player.CurHP / (float)player.MaxHP; }
            if (GetDamage > 0) StartCoroutine(NockBack_Player());
        }
    }

    IEnumerator NockBack_Player()
    {
        CanHit = false;
        player.sprite.color = Color.gray;
        yield return new WaitForSeconds(0.2f);
        player.sprite.color = Color.white;
        CanHit = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Area") && !GameManager.instance.ES.IsPosFixed)
        {
            player.rigid.MovePosition(GameManager.instance.player.Self.transform.position * 1.5f - transform.position * 0.5f);
        }
    }

    protected virtual void OnEnable()
    {
        player.CurHP = player.MaxHP;
        HPBar.fillAmount = 1;
        player.AttackSpeed = player.MaxAttackSpeed;
        player.anim.enabled = true;
        player.anim.SetFloat("AttackSpeed", player.MaxAttackSpeed + GameManager.instance.PlayerStatus.attackspeed);
        player.sprite.color = Color.white;
        CanMove = false; player.Unbeat = false;
        NormalInfo.DealFrom = player.Id;

        if (CurFollow[0] != -1) GameManager.instance.ES.TargetChange[CurFollow[0]][CurFollow[1]][player.Id] = false;
        CurFollow[0] = -1; TargetChangeCall = true;

        if (!IsPlayer)
        {
            player.anim.SetBool("IsAttack", false);
            if (!IsSummon)
            {
                player.MyBatch.HPBar.fillAmount = 1;
            }
        }
        else player.Dir = Vector2.zero;
        if (GameManager.instance.BFM != null) StartCoroutine(BuffCheck());
    }
}
