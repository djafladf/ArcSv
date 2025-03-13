using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Red : PlayerSetting
{
    [SerializeField] Material Mat_Hide_Set;
    [SerializeField] Material Mat_Normal;
    [SerializeField] Transform TargetSprite;
    [SerializeField] Transform Trail;
    [SerializeField] Sprite AttackIm;
    [SerializeField] Sprite SpecialAttackIm;

    [SerializeField] GameObject GageBar;
    [SerializeField] List<GameObject> ReinForceEffect;
    [SerializeField] List<GameObject> ExecuteEffect;
    Image Gage;

    Material Mat_Hide;

    CapsuleCollider2D Cap;
    bool OnHide = false;

    protected override void Awake()
    {
        base.Awake();
        Cap = GetComponent<CapsuleCollider2D>();
        Mat_Hide = new Material(Mat_Hide_Set);
        Gage = GageBar.transform.GetChild(0).GetComponent<Image>();    
        for (int i = 0; i < 39; i++) ExecuteEffect.Add(Instantiate(ExecuteEffect[0], transform));
    }

    BulletInfo SpecInfo = new BulletInfo();
    protected override void Start()
    {
        base.Start();
        SpecInfo.DealFrom = NormalInfo.DealFrom;
        SpecInfo.ExecuteRatio = 0.1f;
        SpecInfo.DeadTrigger = Execute;
        NormalInfo.DeadTrigger = Execute;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!CanMove || OnIce) StopHide();
        else if (!player.Dir.Equals(Vector2.zero) && !OnHide && !OnReinforce) HideCor = StartCoroutine(HideStart());
    }

    protected override void FindTarget()
    {
        if (OnReinforce) {
            base.FindTarget();
            if (player.WeaponLevel >= 7 && TargetPos != null) { transform.position = TargetPos.position + Vector3.up * 0.1f; player.anim.SetTrigger("Spec"); Attack(); }
            return; 
        }
        RaycastHit2D[] targets = Physics2D.CircleCastAll(transform.position, scanRange, Vector2.zero, 0, targetLayer);
        Transform res = null;
        float diff = 0;

        foreach (RaycastHit2D target in targets)
        {
            float curDiff = Vector3.Distance(transform.position, target.transform.position);
            if (curDiff > diff)
            {
                diff = curDiff; res = target.transform;
            }
        }

        if (res != null)
        {
            // Set Target
            TargetPos = res; TargetSprite.transform.parent = TargetPos;
            TargetSprite.gameObject.SetActive(true);
            TargetSprite.transform.localPosition = new Vector2(0, TargetPos.GetComponent<SpriteRenderer>().sprite.bounds.size.y * 0.2f + 1);
            TargetSprite.localScale = Vector2.one;

            CurFollow[0] = TargetPos.name[0] - 1; CurFollow[1] = TargetPos.name[1] - 1; TargetChangeCall = false;
            GameManager.instance.ES.TargetChange[CurFollow[0]][CurFollow[1]][player.Id] = true;
            player.Dir = (TargetPos.position - transform.position).normalized;
            
            if (player.WeaponLevel < 0) { if (Vector3.Distance(transform.position, TargetPos.position) <= AttackRange) Attack(); }
            else { transform.position = TargetPos.position + Vector3.up * 0.1f; player.anim.SetTrigger("Spec"); Attack(); }
        }
        else
        {
            TargetSprite.gameObject.SetActive(false); TargetSprite.transform.parent = transform;
            player.Dir = Vector2.zero; TargetPos = null;
            if (CurFollow[0] != -1) GameManager.instance.ES.TargetChange[CurFollow[0]][CurFollow[1]][player.Id] = false;
            CurFollow[0] = -1; CurFollow[1] = -1; TargetChangeCall = true;
        }
    }

    void StopHide()
    {
        if (HideCor != null) StopCoroutine(HideCor);
        player.sprite.material = Mat_Normal; Cap.enabled = true; OnHide = false; gameObject.layer = 7; tag = "Player";
    }

    protected override void AttackMethod()
    {
        NormalInfo.Damage = (int)((1 + GameManager.instance.PlayerStatus.attack + player.AttackRatio + player.ReinforceAmount[0]) * 15);
        GameManager.instance.BM.MakeMeele(
            NormalInfo, 0.5f, TargetPos.position, -player.Dir, 0, false, AttackIm);
    }

    void SpecTrigger()
    {
        if (gameObject.activeSelf) StartCoroutine(SpecAttack());
    }


    IEnumerator SpecAttack()
    {
        SpecInfo.Damage = (int)((1 + GameManager.instance.PlayerStatus.attack + player.AttackRatio + player.ReinforceAmount[0]) * 25);
        GameManager.instance.BM.MakeMeele(SpecInfo, 1f, transform.position, Vector2.up, 0, false, SpecialAttackIm);
        yield return GameManager.DotOneSec;
        GameManager.instance.BM.MakeMeele(SpecInfo, 1f, transform.position, Vector2.down, 0, false, SpecialAttackIm);
    }

    protected override void AttackEnd()
    {
        if (OnReinforce) { base.AttackEnd(); return; }

        if (TargetChangeCall)
        {
            if (CurFollow[0] != -1) GameManager.instance.ES.TargetChange[CurFollow[0]][CurFollow[1]][player.Id] = false;
            player.anim.SetBool("IsAttack", false); CanMove = true; CurFollow[0] = - 1; CurFollow[1] = - 1;
            
        }
        else
        {
            if (Vector3.Distance(transform.position, TargetPos.position) <= AttackRange)
            {
                player.Dir = (TargetPos.position - transform.position).normalized;
                FlipAnim();
            }
            else
            {
                player.anim.SetBool("IsAttack", false); CanMove = true;
            }
        }
    }

    Coroutine HideCor = null;

    IEnumerator HideStart()
    {
        OnHide = true; gameObject.layer = 9;
        player.sprite.material = Mat_Hide;
        Mat_Hide.SetFloat("_AlphaWeight", 1f);
        Mat_Hide.SetFloat("_BlurRadius", 0);
        tag = "Player_Hide";
        for (int i = 1; i <= 5; i++)
        {
            yield return GameManager.DotOneSec;
            Mat_Hide.SetFloat("_AlphaWeight", 1 - 0.14f * i);
            Mat_Hide.SetFloat("_BlurRadius", 0.6f * i);
        }
        HideCor = null;
    }

    int KillCount = 0;

    public void Execute(Transform a, int b)
    {
        var CurEffect = ExecuteEffect[KillCount % 40]; CurEffect.SetActive(true); CurEffect.transform.position = a.position;
        if (OnReinforce) return;
        KillCount += 1;
        Gage.fillAmount = KillCount * 0.02f;
        if (KillCount >= 50) { KillCount = 0; if (gameObject.activeSelf) StartCoroutine(ExecuteMod()); }
    }

    Color C1 = new Color(1, 0.5f, 0);
    Color C2 = new Color(0.8f, 0, 0);
    IEnumerator ExecuteMod()
    {
        OnReinforce = true;
        StopHide();
        Gage.color = C2;
        NormalInfo.ExecuteRatio = 0.1f;
        NormalInfo.DeadTrigger = Execute;
        foreach (var j in ReinForceEffect) j.SetActive(true);

        for (int i = 0; i < 10; i++)
        {
            Gage.fillAmount = (10 - i) * 0.1f;
            yield return GameManager.OneSec;
        }

        foreach (var j in ReinForceEffect) j.SetActive(false);
        Gage.color = C1;
        OnReinforce = false;
        NormalInfo.ExecuteRatio = 0;
        NormalInfo.DeadTrigger = null;
    }

    bool OnReinforce = false;


    void SetEyePos(string Pos)
    {
        var Parse = Pos.Split(",");
        float x = float.Parse(Parse[0]), y = float.Parse(Parse[1]);
        if (player.sprite.flipX) x *= -1;
        Trail.localPosition = new Vector2(x, y);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        player.sprite.material = Mat_Normal; gameObject.layer = 7; tag = "Player";
        OnReinforce = false;
        Gage.fillAmount = 0; KillCount = 0; Gage.color = C1; NormalInfo.ExecuteRatio = 0;
        
        NormalInfo.DeadTrigger = null;
    }

    private void OnDisable()
    {
        TargetSprite.gameObject.SetActive(false);
    }
}
