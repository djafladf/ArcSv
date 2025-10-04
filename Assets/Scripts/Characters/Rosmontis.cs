using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class Rosmontis : PlayerSetting
{
    [SerializeField] GameObject[] SubWeapons;
    [SerializeField] Sprite[] Sprites;
    [SerializeField] BulletLine BL;
    [SerializeField] GameObject FloatWeapon;
    [SerializeField] GameObject AttackSound;
    [SerializeField] List<AudioSource> AttackSounds;

    Vector3 VectorSub = new Vector3(-2, 15,0);

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        NormalInfo.DeBuffs = new DeBuff(last: 5, defense: 0.3f);
    }

    IEnumerator Attacks()
    {
        Vector3 DirSub = -VectorSub.normalized;
        while (true)
        {
            if (OnIce) yield return GameManager.DotOneSec;
            var Targets = GameManager.GetNearest(scanRange, ProjNum, transform.position, targetLayer);
            if (Targets.Count != 0)
            {
                
                Transform j;
                for (int i = 0; i < ProjNum; i++)
                {
                    AttackSounds[i].Play();
                    j = Targets[Random.Range(0, Targets.Count)];


                    GameManager.instance.BM.MakeEffect(0.4f, j.position + VectorSub, DirSub, 25, Sprites[0], AlphaChange:false,BL: BL);
                    StartCoroutine(LateDamage(0.5f, j.position));
                    yield return new WaitForSeconds(0.1f);
                }
            }
            yield return new WaitForSeconds((3 - GameManager.instance.PlayerStatus.attackspeed) * 0.5f);
        }
    }

    IEnumerator LateDamage(float Time,Vector3 pos)
    {
        NormalInfo.Damage = (int)((1 + GameManager.instance.PlayerStatus.attack + player.AttackRatio + player.ReinforceAmount[0]) * DamageRatio * 10);
        yield return new WaitForSeconds(Time);
        GameManager.instance.BM.MakeMeele( NormalInfo, 0.4f, pos, Vector3.zero, 0, false, Sprites[2]);
    }

    protected override void EndBatch()
    {
        CanMove = true;
        StartCoroutine(Attacks());
        if (player.WeaponLevel >= 7) FloatWeapon.SetActive(true);
        FloatWeapon.transform.position = Vector3.zero;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        FloatWeapon.SetActive(false);
        //GameManager.instance.SetTime(5f,false);
    }

    float DamageRatio = 3f;

    int ProjNum = 1;

    protected override int WeaponLevelUp(bool IsUp = true)
    {
        switch (player.WeaponLevel++)
        {
            case 1: ProjNum++; AttackSounds.Add(Instantiate(AttackSound,transform).GetComponent<AudioSource>()); break;
            case 2: DamageRatio += 0.75f; break;
            case 3: NormalInfo.DeBuffs.Defense += 0.2f; break;
            case 4: ProjNum++; AttackSounds.Add(Instantiate(AttackSound, transform).GetComponent<AudioSource>()); break;
            case 5: DamageRatio += 1.25f; break;
            case 6: FloatWeapon.SetActive(true); break;
        }
        return player.WeaponLevel;
    }

    void OnMove(InputValue value)
    {
        player.Dir = value.Get<Vector2>();
    }

    void OnPause()
    {
        if (!GameManager.instance.SettingM.gameObject.activeSelf) GameManager.instance.UM.GamePause();
    }

    void OnUnit1()
    {
        for (int i = 0; i < GameManager.instance.Players.Length - 1; i++)
        {
            GameManager.instance.Players[i + 1].MyBatch.AllowFollow(0);
            GameManager.instance.Players[i + 1].MyBatch.AllowMove(0);
        }
    }

    void OnUnit2()
    {
        for (int i = 0; i < GameManager.instance.Players.Length - 1; i++)
        {
            GameManager.instance.Players[i + 1].MyBatch.AllowFollow(1);
            GameManager.instance.Players[i + 1].MyBatch.AllowMove(1);
        }
    }

    void OnUnit3()
    {
        for (int i = 0; i < GameManager.instance.Players.Length - 1; i++)
        {
            GameManager.instance.Players[i + 1].MyBatch.AllowFollow(2);
            GameManager.instance.Players[i + 1].MyBatch.AllowMove(2);
        }
    }
}
