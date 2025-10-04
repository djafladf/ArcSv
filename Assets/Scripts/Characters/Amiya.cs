using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Amiya : PlayerSetting
{
    [SerializeField] Sprite Bullet1;
    [SerializeField] Sprite Bullet2;
    [SerializeField] GameObject Weapon;
    [SerializeField] GameObject Weapon2;
    [SerializeField] BulletLine BL;
    [SerializeField] BulletLine BLT;
    [SerializeField] List<ParticleSystem> PTS;

    [SerializeField] List<AudioSource> AttackSounds;

    [SerializeField] List<Transform> Razer;

    AudioSource source;

    Sprite BulletIm;
    BulletLine CurApplyLine;
    protected override void Awake()
    {
        base.Awake();
        source = GetComponent<AudioSource>(); AttackSounds.Add(PTS[0].GetComponent<AudioSource>());
        player.SubEffects.Add(Weapon.GetComponent<SpriteRenderer>());
        player.SubEffects.Add(Weapon2.GetComponent<SpriteRenderer>());
        
        BulletIm = Bullet1;
        CurApplyLine = BL;
    }

    BulletInfo SpecInfo = new BulletInfo();

    protected override void Start()
    {
        base.Start();
        StartCoroutine(Test());
    }

    int LastUse = 0;

    IEnumerator Test()
    {
        while (true)
        {
            if (OnIce) yield return GameManager.DotOneSec;
            NormalInfo.Damage = (int)((1 + GameManager.instance.PlayerStatus.attack + player.AttackRatio + player.ReinforceAmount[0]) * DamageRatio * 10);
            var Targets = GameManager.GetNearest(scanRange, ProjNum, transform.position, targetLayer);
            if (Targets.Count != 0)
            {
                Transform j = Targets[0];
                for (int i = 0; i < ProjNum * ProjRatio; i++)
                {
                    AttackSounds[i].Play();
                    Vector3 RandomSub = new Vector3(Random.Range(-1f, 1f), Random.Range(0f, 1f)) + transform.position;
                    j = Targets[Random.Range(0, Targets.Count)];
                    PTS[i].gameObject.transform.position = RandomSub;
                    PTS[i].Play();
                    GameManager.instance.BM.MakeBullet(NormalInfo, Penetrate, RandomSub,
                        (j.position - transform.position).normalized, BulletSpeed, false, BulletIm,
                        BL: CurApplyLine);
                    yield return new WaitForSeconds(0.1f);
                }

                if (player.WeaponLevel >= 7)
                {
                    SpecInfo.Damage = (int)(NormalInfo.Damage * 0.1f);
                    Vector3 RandomSub = new Vector3(Random.Range(-1f, 1f), Random.Range(0f, 1f)) + transform.position;
                    Vector2 DirSub = (j.position - RandomSub).normalized; 
                    Razer[2 * LastUse].position = RandomSub; Razer[2 * LastUse + 1].rotation = Quaternion.FromToRotation(Vector2.right, DirSub); Razer[2 * LastUse].gameObject.SetActive(true);
                    LastUse++; if (LastUse >= 6) LastUse = 0;
                }
            }
            yield return new WaitForSeconds(0.25f * (2 - GameManager.instance.PlayerStatus.attackspeed));
        }
    }

    int ProjNum = 1;
    int ProjRatio = 1;
    float DamageRatio = 2f;
    int Penetrate = 0;
    float BulletSpeed = 15;




    protected override int WeaponLevelUp(bool IsUp = true)
    {
        switch (player.WeaponLevel++)
        {
            case 1: Penetrate++; break;
            case 2: DamageRatio += 0.5f; break;
            case 3: ProjNum++; PTS.Add(Instantiate(PTS[0],transform.GetChild(0))); AttackSounds.Add(PTS[1].GetComponent<AudioSource>()); break;
            case 4: Penetrate += 2; break;
            case 5: DamageRatio += 1f; break;
            case 6:
                source.Play(); BulletIm = Bullet2;
                Penetrate = 50; ProjNum++; ProjRatio = 2; BulletIm = Bullet2; BulletSpeed = 30; Weapon2.SetActive(true); for (int i = 0; i < 4; i++) { PTS.Add(Instantiate(PTS[0], transform.GetChild(0))); AttackSounds.Add(PTS[i+2].GetComponent<AudioSource>());}

                SpecInfo.Copy(NormalInfo);
                Razer[0].parent = GameManager.instance.BM.transform;

                for (int i = 0; i < 6; i++) { Transform cnt = Instantiate(Razer[0].gameObject).transform; Razer.Add(cnt); Razer.Add(cnt.transform.GetChild(0)); }
                for (int i = 0; i < 6; i++) { Razer[2 * i + 1].name = $"{GameManager.instance.BM.RegistExBullet(SpecInfo)}"; Razer[2 * i].parent = GameManager.instance.BM.transform; }
                break;
        }
        return player.WeaponLevel;
    }

    void OnMove(InputValue value)
    {
        player.Dir = value.Get<Vector2>();
    }

    void OnPause()
    {
        if(!GameManager.instance.SettingM.gameObject.activeSelf) GameManager.instance.UM.GamePause();
    }

    void OnUnit1()
    {
        for(int i = 0; i < GameManager.instance.Players.Length-1; i++)
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

