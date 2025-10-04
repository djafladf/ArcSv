using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Wafarin :PlayerSetting
{
    [SerializeField] Sprite Bullet;
    [SerializeField] BulletLine BL;
    [SerializeField] GameObject Pond;
    [SerializeField] GameObject Wing;

    [SerializeField] List<GameObject> Bat_Attack;
    [SerializeField] List<GameObject> Bat_Heal;
    List<Wafarin_Bat> Attack_Script = new List<Wafarin_Bat>();
    List<Wafarin_Bat> Heal_Script = new List<Wafarin_Bat>();
 
    protected override void Awake()
    {
        base.Awake();
        player.SubEffects.Add(Wing.transform.GetChild(0).GetComponent<SpriteRenderer>());
        player.SubEffects.Add(Wing.transform.GetChild(1).GetComponent<SpriteRenderer>());
        
        
    }
    protected override void Start()
    {
        base.Start();
        for (int i = 0; i < 10; i++) { Bat_Attack.Add(Instantiate(Bat_Attack[0])); Bat_Heal.Add(Instantiate(Bat_Heal[0])); }
        foreach (var i in Bat_Attack) { Attack_Script.Add(i.GetComponent<Wafarin_Bat>()); i.transform.parent = GameManager.instance.BM.transform; i.transform.localScale = Vector3.one * 0.5f; }
        foreach (var i in Bat_Heal) { Heal_Script.Add(i.GetComponent<Wafarin_Bat>()); i.transform.parent = GameManager.instance.BM.transform; i.transform.localScale = Vector3.one * 0.5f; }
    }

    int Attack_Last = 0;
    int Heal_Last = 0;
    Vector3 up = new Vector3(0, 2, 0);

    List<Transform> Targets;
    List<Transform> HealTargets = new List<Transform>();
    void AttackSet()
    {
        Targets = GameManager.GetNearest(AttackRange, SummonNum, transform.position, targetLayer);
        HealTargets.Clear();
        float[] HealVar = { 1, 1 }; int[] HealInd = { -1, -1 };
        for(int i = 0; i < GameManager.instance.Prefs.Length; i++)
        {
            if (!GameManager.instance.Prefs[i].activeSelf) continue;
            float rat = GameManager.instance.Players[i].CurHP / GameManager.instance.Players[i].MaxHP; //if (rat == 1) continue;
            if (rat < HealVar[0]) { HealVar[0] = rat; HealVar[1] = HealVar[0]; HealInd[0] = i; HealInd[1] = HealInd[0]; }
            else if (rat < HealVar[1]) { HealVar[1] = rat; HealInd[1] = i; }
        }

        if (HealInd[0] != -1) HealTargets.Add(GameManager.instance.Prefs[HealInd[0]].transform);
        if (HealInd[1] != -1) HealTargets.Add(GameManager.instance.Prefs[HealInd[1]].transform);

        if (Targets.Count == 0 && HealTargets.Count == 0) AttackEnd();
    }

    protected override void AttackMethod()
    {
        if(Targets.Count != 0) for(int i=0;i < SummonNum; i++)
        {
            Bat_Attack[Attack_Last].SetActive(true); Bat_Attack[Attack_Last].transform.position = transform.position + new Vector3(Random.Range(-3,3),Random.Range(0,3));
            if(i==0) Attack_Script[Attack_Last].Init(Targets[0]); 
            else Attack_Script[Attack_Last].Init(Targets.Count==2 ? Targets[1] : Targets[0]);
            Attack_Last++; if (Attack_Last == Bat_Attack.Count) Attack_Last = 0;
        }

        if(HealTargets.Count != 0) for(int i = 0; i < SummonNum; i++)
            {
                Bat_Heal[Heal_Last].SetActive(true); Bat_Heal[Heal_Last].transform.position = transform.position + new Vector3(Random.Range(-3, 3), Random.Range(0, 3));
                if (i == 0) Heal_Script[Heal_Last].Init(HealTargets[0]);
                else Heal_Script[Heal_Last].Init(HealTargets.Count == 2 ? HealTargets[1] : HealTargets[0]);
                Heal_Last++; if (Heal_Last == Bat_Heal.Count) Heal_Last = 0;
            }

        

        if (player.WeaponLevel >= 7 && !Pond.activeSelf)
        {
            Vector3 cnt = TargetPos.position; cnt.z = 20; Pond.transform.position = cnt;
            Pond.SetActive(true);
        }
    }
    

    protected override void EndBatch()
    {
        base.EndBatch();
        if (player.WeaponLevel >= 7) Wing.SetActive(true);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        Pond.SetActive(false);
        Wing.SetActive(false);
    }

    [HideInInspector] public float HealRatio = 2f;
    [HideInInspector] public float DamageRatio = 5f;
    int SummonNum = 1;

    protected override int WeaponLevelUp(bool IsUp = true)
    {
        if (player.WeaponLevel <= 6)
        {
            switch (player.WeaponLevel++)
            {
                case 1: HealRatio = 2.1f; break;
                case 2: DamageRatio += 5f; break;
                case 3: SummonNum = 2; break;
                case 4: HealRatio = 2.3f; break;
                case 5: DamageRatio += 10; break;
                case 6: Wing.gameObject.SetActive(true); break;
            }
        }
        return player.WeaponLevel;
    }
}
