using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86.Avx;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] GameObject[] EnemyTypes;
    [SerializeField] public GameObject BossSet;
    [SerializeField] int[] PoolSize;
    public bool IsTest;

    List<List<GameObject>> Pool = new();
    [HideInInspector] public Dictionary<GameObject, Enemy> InstanceTo = new();

    [HideInInspector] public int CurActive = 0;

    Vector3[] SpawnArea;
    int SpawnAreaSize;

    [System.Serializable]
    public class StageInfo
    {
        public List<SpawnInfo> spawninfo;   
    }

    public enum EnemyId
    {
        // 1 Stage
        YellowWorm, RedWorm,    Yoma,       FootMan,    DualMan,   ShieldMan,    BowMan,    Magician, Skulslr, Parat,
        Cannon,     Revenger,   BoomSpider, HeavyAr,    HiCaster,  HostMan,      HostThrow, HostAr,   Faust,   Mephisto,
        SnowMan,    SnowCaster, SnowSniper, ArtsMs,     BoomDrone, DefenseDrone, FrostNova
    }

    [System.Serializable]
    public class SpawnInfo
    {
        public EnemyId id;
        public int start;
        public int end;
        public float respawn;
        public bool IsBoss = false;
        public bool IsLast = false;
    }

    [SerializeField] List<StageInfo> Stages;
    int CurStage = 0;
    int[] LastUse;
    bool[] IsLast;
    public Action<GameObject>[] TargetChangeAct = new Action<GameObject>[5];
    public Dictionary<GameObject, bool[]> TargetChange = new Dictionary<GameObject, bool[]>();

    private void Awake()
    {
        if (GameManager.instance.IsTest != IsTest)
        {
            Destroy(this);
            return;
        }
        GameManager.instance.ES = this;
        GameManager.instance.StartLoading();
    }

    public void Init(int Stage)
    { 
        // Set Spawn Area
        SpawnArea = new Vector3[100];
        
        int i = 0;
        for (int x = -25; x <= 25; x+=2) SpawnArea[i++] = new Vector3(x, 15,0);
        for (int x = 15; x >= -15; x-=2) { SpawnArea[i++] = new Vector3(-25, x,0); SpawnArea[i++] = new Vector3(25, x,0); }
        for (int x = -25; x <= 25; x+=2) SpawnArea[i++] = new Vector3(x, -15,0);
        SpawnAreaSize = i;
        MakeNewPref(0, StageInits[0]);

        int EnemyCount = Enum.GetValues(typeof(EnemyId)).Length;

        if (GameManager.instance.IsTest) EnemyCount = 1;

        IsLast = new bool[EnemyCount]; LastUse = new int[EnemyCount];
        for (i = 0; i < EnemyCount; i++) 
        { 
            IsLast[i] = false;
            //for(int j = 0; j < PoolSize[i]; j++) TargetChange[i].Add( new bool[]{false,false,false,false,false} );
        }

        MaxSpawn = (int)(MaxSpawn * (1+GameManager.instance.EnemyStatus.spawn));

        GameManager.instance.StartLoading();
    }
    //[SerializeField] int InitRange;

    [SerializeField]
    List<int> StageInits;

    public void MakeNewPref(int st, int ed)
    {
        if (st == ed && st != 0) return;
        Enemy Script;
        GameObject Obj;
        for (int i = st; i <= ed; i++)
        {
            Pool.Add(new List<GameObject>());
            if (EnemyTypes[i] == null) continue;
            for (int y = 0; y < PoolSize[i]; y++)
            {
                Obj = Instantiate(EnemyTypes[i], transform); Pool[i].Add(Obj);
                Script = Obj.GetComponent<Enemy>();
                InstanceTo.Add(Obj,Script); TargetChange[Obj] = new bool[] { false, false, false, false, false };
                Obj.SetActive(false);
            }
        }
        GameManager.instance.UM.BossShaft.gameObject.SetActive(false);
    }

    public void StartStage()
    {
        BossSet.SetActive(false);
        if (CurStage >= Stages.Count) GameManager.instance.UM.GameClear();
        else
        {
            if(CurStage != 0)MakeNewPref(StageInits[CurStage]+1, StageInits[CurStage+1]);
            foreach (SpawnInfo cnt in Stages[CurStage].spawninfo) StartCoroutine(Spawn(cnt));
            for (int i = 0; i < StageInits[CurStage]; i++) if (IsLast[i]) 
                {
                    for (int x = 0; x < PoolSize[i]; x++) Destroy(Pool[i][x]);
                    PoolSize[i] = 0;
                    Pool[i] = null;
                    IsLast[i] = false;
                }
            CurStage++;
        }
    }

    int MaxSpawn = 60;

    IEnumerator Spawn(SpawnInfo Info)
    {
        yield return new WaitForSeconds(Info.start);
        Info.respawn *= (1 - GameManager.instance.EnemyStatus.spawn);
        int SpawnTimes = Mathf.FloorToInt((Info.end - Info.start) / Info.respawn);
        WaitForSeconds SpawnGap = new WaitForSeconds(Info.respawn);
        bool IsSpawned;
        if (Info.IsBoss) { GameManager.instance.BossStage(); /*BossSet.SetActive(true);*/ }

        int Id = (int)Info.id;
        Enemy Script;
        for (int i = 0; i <= SpawnTimes; i++)
        {
            IsSpawned = false;
            Vector3 cnt;
            if (IsPosFixed) cnt = SpawnArea[Random.Range(0, SpawnAreaSize - 1)] + FixedPos;
            else cnt = SpawnArea[Random.Range(0, SpawnAreaSize - 1)] + GameManager.instance.player.Self.position;
            cnt.z = 1;
            for (int z = 0; z < PoolSize[Id]; z++)
            {
                if (++LastUse[Id] >= PoolSize[Id]) LastUse[Id] = 0;
                if (Pool[Id][LastUse[Id]].activeSelf) continue;
                Pool[Id][LastUse[Id]].transform.position = cnt;
                Pool[Id][LastUse[Id]].SetActive(true);
                IsSpawned = true;
                break;
            }

            if (!IsSpawned && PoolSize[Id] < MaxSpawn)
            {
                var tmp = Instantiate(EnemyTypes[Id], transform);
                Pool[Id].Add(tmp);
                Script = tmp.GetComponent<Enemy>();
                InstanceTo.Add(tmp,tmp.GetComponent<Enemy>());
                Pool[Id][PoolSize[Id]++].transform.position = cnt; TargetChange[tmp] = new bool[] { false, false, false, false, false };
            }
            if (Info.IsBoss) break;
            yield return SpawnGap;
        }
        if (Info.IsLast) IsLast[Id] = true;
    }

    List<Coroutine> spawncall = new List<Coroutine>();
    public void ExternalSpawnCall(int ind,int Times, float Gap)
    {
        StartCoroutine(ExtraSpawn(ind,Times,Gap));
    }

    public GameObject TakeOffObj(int ind)
    {
        bool IsSpawned = false;
        foreach (var pool in Pool[ind]) if (!pool.activeSelf)
            {
                pool.SetActive(true);
                IsSpawned = true;
                return pool;
            }
        if (!IsSpawned)
        {
            GameObject tmp = Instantiate(EnemyTypes[ind], transform); tmp.SetActive(false);
            var Script = tmp.GetComponent<Enemy>(); 
            InstanceTo.Add(tmp, Script); TargetChange[tmp] = new bool[] { false, false, false, false, false };
            Pool[ind].Add(tmp); PoolSize[ind]++;
            return tmp;
        }
        return null;
    }

    IEnumerator ExtraSpawn(int Id,int Times,float Gap)
    {
        WaitForSeconds SpawnGap = new WaitForSeconds(Gap);
        bool IsSpawned;
        while (Times-- != 0)
        {
            IsSpawned = false;
            Vector3 cnt;
            if(IsPosFixed) cnt = SpawnArea[Random.Range(0, SpawnAreaSize - 1)] + FixedPos;
            else cnt = SpawnArea[Random.Range(0, SpawnAreaSize - 1)] + GameManager.instance.player.Self.position;
            cnt.z = 1;
            for (int z = 0; z < PoolSize[Id]; z++)
            {
                if (++LastUse[Id] >= PoolSize[Id]) LastUse[Id] = 0;
                if (Pool[Id][LastUse[Id]].activeSelf) continue;
                Pool[Id][LastUse[Id]].transform.position = cnt;
                Pool[Id][LastUse[Id]].SetActive(true);
                IsSpawned = true;
                break;
            }
            if (!IsSpawned && PoolSize[Id] < MaxSpawn)
            {
                var tmp = Instantiate(EnemyTypes[Id], transform);
                var Script = tmp.GetComponent<Enemy>();
                InstanceTo.Add(tmp, tmp.GetComponent<Enemy>());
                Pool[Id].Add(tmp); Pool[Id][PoolSize[Id]++].transform.position = cnt; TargetChange[tmp] = new bool[] { false, false, false, false, false };
            }
            yield return SpawnGap;
        }
    }

    [HideInInspector] public bool IsPosFixed = false;
    Vector3 FixedPos;

    public void SetCurrentSpawnPos()
    {
        IsPosFixed = true;
        FixedPos = GameManager.instance.player.Self.position;
    }

    public void ReleaseSpawnPos()
    {
        IsPosFixed = false;
    }

    public void StopCor()
    {
        StopAllCoroutines();
    }

    public Vector3 ReBatchCall(Vector3 Pos)
    {
        return SpawnArea[Random.Range(0,SpawnAreaSize)];
    }
}
