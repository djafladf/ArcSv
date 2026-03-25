using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;
using Cinemachine;
using System.Collections;
using UnityEngine.UI;
using Newtonsoft.Json;
using UnityEngine.InputSystem;
using UnityEditor;
[DefaultExecutionOrder(-5000)]
public class GameManager : MonoBehaviour
{
    public float CurVersion;
    public bool IsTest = false;

    public static GameManager instance;
    public static WaitForSeconds OneSec = new WaitForSeconds(1);
    public static WaitForSeconds TwoSec = new WaitForSeconds(2);
    public static WaitForSeconds DotHalf = new WaitForSeconds(0.05f);
    public static WaitForSeconds DotOneSec = new WaitForSeconds(0.1f);
    public static WaitForSeconds DotQuarter = new WaitForSeconds(0.25f);
    public static WaitForSeconds FrameWFS = new WaitForSeconds(0.01f);
    public static WaitForSeconds DotFiveSec = new WaitForSeconds(0.5f);

    [SerializeField]
    private float _TimeValue = 1;
    private float cnt = 1;

    private void OnValidate()
    {
        TimeValue = _TimeValue;
    }

    public float TimeValue
    {
        get { return cnt; }
        set
        {
            if(cnt != value)
            {
                SetTime(cnt, true);
                SetTime(value, false);
                cnt = value;
            }
        }
    }


    public static int StringToInt(string Var)
    {
        int outValue = 0;
        for (int i = 0; i < Var.Length; i++) outValue = outValue * 10 + (Var[i] - '0');
        return outValue;
    }

    public static List<Transform> GetNearest(float scanRange, int count, Vector3 Position, LayerMask targetLayer)
    {
        Collider2D[] targets = new Collider2D[75];
        int res = Physics2D.OverlapCircleNonAlloc(Position, scanRange, targets, targetLayer);
        List<(Transform target, float dist)> Set = new(Mathf.Min(count,res));
        for(res = res -1; res >= 0; res--)
        {
            float curDiff = Vector3.SqrMagnitude(Position - targets[res].transform.position);
            if (Set.Count < count)
            {
                Set.Add((targets[res].transform, curDiff));
                if(Set.Count == count) Set.Sort((a, b) => a.dist.CompareTo(b.dist));
            }
            else if (Set[count - 1].dist > curDiff)
            {
                Set[count - 1] = (targets[res].transform, curDiff);
                Set.Sort((a, b) => a.dist.CompareTo(b.dist));
            }
        }
        return Set.Select(x => x.target).ToList();
    }
    public static Transform GetNearest(float scanRange, Vector3 Position, LayerMask targetLayer,List<Transform> ex = null)
    {
        Collider2D[] targets = new Collider2D[75];
        int res = Physics2D.OverlapCircleNonAlloc(Position, scanRange, targets, targetLayer);
        float Min = float.MaxValue; Transform ret = null;
        for (res = res - 1; res >= 0; res--)
        {
            if (ex != null) if (ex.Contains(targets[res].transform)) continue;
            float curDiff = Vector3.SqrMagnitude(Position - targets[res].transform.position);
            if(curDiff < Min) { Min = curDiff; ret = targets[res].transform; }
        }
        return ret;
    }

    // On Loby
    [HideInInspector] public DataManager Data;
    [HideInInspector] public ShopManager Shop;

    // On Game
    [HideInInspector] public Player player;
    [HideInInspector] public BulletManager BM;
    [HideInInspector] public ItemManager IM;
    [HideInInspector] public EnemySpawner ES;
    [HideInInspector] public DamageManager DM;
    [HideInInspector] public UIManager UM;
    [HideInInspector] public BuffManager BFM;
    [HideInInspector] public GameObject Git;
    [HideInInspector] public AudioManager AudioM;

    // On Base
    [HideInInspector] public BaseManager _base;


    public SettingManager SettingM;

    //Both
    public FloatMessage FloatM;

    public Camera MainCam;
    public CinemachineVirtualCamera VC;

    // ID

    public string[] Player_ID;


    [NonSerialized] public string PlayerName = "Amiya";
    public attribute PlayerStatus;
    public EnemStat EnemyStatus;
    attribute InitPlayerStatus;

    public GameStatus gameStatus;
    public InputActionAsset PlayerInput;

    private void OnApplicationQuit()
    {
        gameStatus.LastBatch = CurPlayerID;
        string json = JsonConvert.SerializeObject(gameStatus);
        File.WriteAllText(Path.Combine(Application.persistentDataPath, $"Status.json"), json);
    }

    [HideInInspector] public int RatType = 0;

    void Awake()
    {
        InitPlayerStatus = PlayerStatus;
        if (instance == null) { 
            instance = this; 
            DontDestroyOnLoad(gameObject);

            // Load GameStatus
            string path = Path.Combine(Application.persistentDataPath, $"Status.json");
            if (File.Exists(path)) gameStatus = JsonConvert.DeserializeObject<GameStatus>(File.ReadAllText(path));
            else { gameStatus = new GameStatus(); gameStatus.LastBatch.Add(0); }

            CurPlayerID = gameStatus.LastBatch;
#if UNITY_ANDROID || UNITY_IOS
            Application.targetFrameRate = 60;
        float Rat = 2560f / 1440f;
        float Rat2 = (float)Screen.width / Screen.height;
            float RatRat = Rat / Rat2;
            Camera.main.transform.localScale *= (RatRat + (RatRat < 0.95f ? 0.15f : 0));
            if (Rat < Rat2)
            {
                transform.GetChild(2).GetComponent<CanvasScaler>().matchWidthOrHeight = 1;
                
                RatType = 1;
            }
#endif

        }
        else if (instance != this) Destroy(gameObject);
        //else Destroy(gameObject);
        
        //LoadAssets();
    }
#if UNITY_ANDROID || UNITY_IOS
    private void Start()
    {
        
    }
#endif

    public void ApplyKeyOption()
    {

        var MoveBind = PlayerInput.FindAction("Move");
        int keyboardInd = 0;

        while (!MoveBind.bindings[keyboardInd].path.StartsWith("<Keyboard>")) keyboardInd++;

        for (int i = 0; i < 4; i++) MoveBind.ApplyBindingOverride(keyboardInd+i, gameStatus.MoveKey[i]);

        PlayerInput.FindAction("Pause").ApplyBindingOverride(0, gameStatus.PauseKey);
        
        for (int i = 0; i < 3; i++) PlayerInput.FindAction($"Unit{i+1}").ApplyBindingOverride(0, gameStatus.UnitKey[i]);
    }

    //[HideInInspector] 
    public Player[] Players;
    public GameObject[] Prefabs;
    public PlayerSetting[] Scripts;
    public Dictionary<GameObject, int> ObjToInd;

    public PlayerSetting GetScript(GameObject refObj = null,int ind = -1)
    {
        if (refObj != null) return Scripts[ObjToInd[refObj]];
        else if (ind != -1) return Scripts[ind];
        else return null;
    }


    [HideInInspector] public GameObject[] Prefs;
    public List<int> CurPlayerID;
    public int PlayerInd = 0;

    public void AddSummonInfo(GameObject obj,Player pl,bool IsPriority = false)
    {
        obj.name = $"{Prefs.Length}";

        var cnt = Prefs.ToList(); cnt.Add(obj);
        Prefs = cnt.ToArray();
        UM.IsPriorityAttack.Add(IsPriority);

        var tmp = Players.ToList(); tmp.Add(player);
        Players = tmp.ToArray();
    }

    public List<Sprite> LoadingSprites;
    [SerializeField] Image LoadedImage;
    public void StartGame()
    {
        if (CurPlayerID[0] == -1) return;
        player = Data.Infos[0].player;
        FirstLoading = 6; LastLoading = 7;
        TimeSet.Clear(); Time.timeScale = 1;
        StartCoroutine(LoadingAct("MainGame", LoadingSprites[1],false));
    }

    public void EndGame()
    {
        PlayerStatus = InitPlayerStatus;
        TimeSet.Clear(); Time.timeScale = 1;
        StartCoroutine(LoadingAct("MainLoby", LoadingSprites[0],true));
    }

    Color CntAlpha = new Color(0, 0, 0, 0.1f);
    IEnumerator LoadingAct(string SceneName,Sprite Loadings, bool AutoEnd)
    {
        LoadedImage.sprite = Loadings; LoadedImage.gameObject.SetActive(true);
        for(int i = 0; i < 10; i++)
        {
            yield return new WaitForSeconds(0.05f);
            LoadedImage.color += CntAlpha;
        }
        SceneManager.LoadScene(SceneName);
        if (AutoEnd) StartCoroutine(LoadingEndAct());
    }

    IEnumerator LoadingEndAct()
    { 
        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForSeconds(0.05f);
            LoadedImage.color -= CntAlpha;
        }
        LoadedImage.gameObject.SetActive(false);
    }

    int FirstLoading = 6;
    int LastLoading = 7;
    public void StartLoading()
    {
        if (--FirstLoading <= 0)
        {
            LastLoading--;
            if(LastLoading == 6) LoadAsset_Game();
            else if (LastLoading == 0)
            {
                ApplyKeyOption();
                StartCoroutine(LoadingEndAct());
                PlayerObj.SetActive(true);
                ES.StartStage();
            }
        }
    }
    GameObject PlayerObj;
    
    private void LoadAsset_Game()
    {
        // SetManager
        Git = DM.transform.parent.GetChild(0).gameObject;

        var BatchName = CurPlayerID.Select(index => Player_ID[index]).ToArray();
        int LL = BatchName.Length;

        // Get Operators
        Players = new Player[LL];
        Prefs = new GameObject[LL];
        Scripts = new PlayerSetting[LL];
        ObjToInd = new Dictionary<GameObject, int>();
        for (int i = 0; i < LL; i++) 
        {
            var CurId = CurPlayerID[i];
            Players[i] = ScriptableObject.Instantiate(Data.Infos[CurId].player); Players[i].Id = i;  Players[i].CurReinforce = Mathf.FloorToInt(gameStatus.Exceed[CurId] *0.1f); 
            Prefs[i] = Instantiate(Prefabs[CurPlayerID[i]],DM.transform.parent);
            ObjToInd[Prefs[i]] = i;
            Scripts[i] = Prefs[i].GetComponent<PlayerSetting>();
            Scripts[i].player = Players[i]; Scripts[i].ExternInit();
        }
        /*await AddressablesLoader.InitAssets(BatchName, "Operator_Pref", Prefs, DM.transform.parent);*/
        PlayerObj = Prefs[0];


        // Init

        PlayerStatus.attack = gameStatus.Stat[0] * 0.05f - 0.05f;
        PlayerStatus.defense = gameStatus.Stat[1] * 0.02f - 0.02f;
        PlayerStatus.hp = gameStatus.Stat[2] * 0.04f - 0.04f;
        PlayerStatus.speed = gameStatus.Stat[3] * 0.05f - 0.05f;
        PlayerStatus.pickup = gameStatus.Stat[4] * 0.1f + 0.9f;
        PlayerStatus.attackspeed = gameStatus.Stat[5] * 0.02f - 0.02f;
        PlayerStatus.cost = gameStatus.Stat[7] * 0.05f + 0.95f;
        PlayerStatus.exp = gameStatus.Stat[8] * 0.05f + 0.95f;
        PlayerStatus.power = 0;
        PlayerStatus.heal = 0;
        PlayerStatus.selection = 3;

        EnemyStatus.attack = gameStatus.Enem[0] * 0.1f;
        EnemyStatus.defense = gameStatus.Enem[1] * 0.05f;
        EnemyStatus.hp = gameStatus.Enem[2] * 0.1f;
        EnemyStatus.speed = gameStatus.Enem[3] * 0.1f;
        EnemyStatus.spawn = gameStatus.Enem[4] * 0.1f;
        EnemyStatus.boss = gameStatus.Enem[5] * 0.05f;

        PlayerStatus.GoodsEarn = gameStatus.Enem.Sum() * 0.1f + 1;
        
        IM.Init(); BM.Init(); ES.Init(1); DM.Init(); BFM.Init();
        UM.Init(LL, CurPlayerID.Select(index => Data.WeaponSub[index]).ToList(), Players, Prefs, CurPlayerID.Select(index => Data.Infos[index]).ToArray(), PlayerInd);

    }

    // ���۷����� ����
    public void RequestOfWeapon(Func<bool, int> func, int id)
    {
        if (UM.WeaponLevelUps == null)
        {
            UM.WeaponLevelUps = new Func<bool,int>[CurPlayerID.Count];
        }
        UM.WeaponLevelUps[id] = func;
    }

    List<float> TimeSet = new List<float>();
    public void SetTime(float var, bool IsRemove)
    {
        if (IsRemove)
        {
            TimeSet.Remove(var);
            if (TimeSet.Count == 0) Time.timeScale = 1;
            else Time.timeScale = TimeSet[0];
        }
        else
        {
            if (TimeSet.Count == 0) Time.timeScale = var;
            else if (var < TimeSet[0]) Time.timeScale = var;
            TimeSet.Add(var);
            TimeSet.Sort();
        }
    }

    public bool IsBoss = false;
    public void BossStage()
    {
        ES.StopCor();
        IsBoss = true;
    }

    public void BossEnd()
    {
        ES.StartStage();
        UM.BossShaft.gameObject.SetActive(false);
        IsBoss = false;
    }

    public void CalcBuffAmount(ref float BuffVar,ref float BuffTime, float GetTime, float GetVar)
    {
        //if(BuffVar == )
    }
    

    // --------------------------------------------------------------------------------------------



    // ETC Func -------------------------------

    /*private async void GetExternalAsset<T>(string _label, List<T> _createdObjs, Transform _parent) where T : Object
    {
        await AddressablesLoader.InitAssets(_label, _createdObjs, _parent);
    }

    private async void GetExternalAsset<T>(string[] name, string _label, List<T> _createdObjs, Transform _parent) where T : Object
    {
        await AddressablesLoader.InitAssets(name, _label, _createdObjs, _parent);
    }*/

    /*private async void GetExternalAsset<T>(string[] name, string _label, bool IsIm, List<T> _createdObjs) where T : Object
    {
        await AddressablesLoader.InitAssets(name, _label,IsIm, _createdObjs);
    }
*/
}