using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    [SerializeField] Sprite[] EXPs;
    [SerializeField] Sprite Money;
    [SerializeField] Sprite Stone;
    [SerializeField] Sprite RefinedStone;
    [SerializeField] Sprite BlackHole;
    [SerializeField] Sprite[] Flares;

    [SerializeField] Transform[] Flare;
    [SerializeField] ParticleSystem[] FlareSmoke;

    [SerializeField] GameObject ItemPref;

    [SerializeField] int MaxItem = 250;
    GameObject[] Items;
    SpriteRenderer[] ItemsSprite;
    Item[] ItemsScript;
    List<int> CreatedTiming;

    private void Awake()
    {
        GameManager.instance.IM = this;
        GameManager.instance.StartLoading();
    }

    public void Init()
    {
        CreatedTiming = new List<int>();
        Items = new GameObject[MaxItem];
        ItemsSprite = new SpriteRenderer[MaxItem];
        ItemsScript = new Item[MaxItem];

        for (int i = 0; i < MaxItem; i++)
        {
            Items[i] = Instantiate(ItemPref, transform); Items[i].name = "a";
            ItemsSprite[i] = Items[i].GetComponent<SpriteRenderer>();
            ItemsScript[i] = Items[i].GetComponent<Item>();
            ItemsScript[i].poolInd = i;
            Items[i].SetActive(false);
        }
        GameManager.instance.StartLoading();
    }

    float MaxProb = 0.99f;

    int Lastuse = 0;
    public void MakeItem(Vector3 pos, bool MustMake = false)
    {
        if (Lastuse++ >= MaxItem) Lastuse = 0;
        int First;
        if (CreatedTiming.Count >= MaxItem)
        { First = CreatedTiming[0]; CreatedTiming.RemoveAt(0); Lastuse = First; }
        else First = Lastuse;
        float Ran;
        if (MustMake) Ran = Random.Range(0f, MaxProb);
        else Ran = Random.Range(0f, 1f);

        if (Ran < MaxProb)
        {
            CreatedTiming.Add(First);
            Items[First].SetActive(true);
            Items[First].transform.position = pos + new Vector3(-0.2f + Ran * 0.002f, 0.2f - Ran * 0.002f);
        }
        if (Ran < 0.5f)
        {
            int CurExp = Mathf.FloorToInt(GameManager.instance.UM.CurMinute * 0.125f); if (CurExp > 3) CurExp = 3;
            ItemsSprite[First].sprite = EXPs[CurExp];
            ItemsScript[First].Init(0, (int)Mathf.Pow(2, CurExp));
        }
        else if (Ran < 0.55f)
        {
            ItemsSprite[First].sprite = Money;
            ItemsScript[First].Init(1, 5);
        }
        else if (Ran < 0.56f)
        {
            ItemsSprite[First].sprite = Stone;
            ItemsScript[First].Init(2, 1);
        }
        else if (Ran < 0.57f)
        {
            ItemsSprite[First].sprite = RefinedStone;
            ItemsScript[First].Init(3, 5);
        }
        else if (Ran < 0.58f)
        {
            ItemsSprite[First].sprite = BlackHole;
            ItemsScript[First].Init(4, 0);
        }
        if (Ran < MaxProb)
        {
            if (Ran < 0.582f && !FlareOn[0]) { ItemsSprite[First].sprite = Flares[0]; ItemsScript[First].Init(5, 0); FlareOn[0] = true; }
            else if (Ran < 0.584f && !FlareOn[1]) { ItemsSprite[First].sprite = Flares[1]; ItemsScript[First].Init(5, 1); FlareOn[1] = true; }
            else if (!FlareOn[2]) { ItemsSprite[First].sprite = Flares[2]; ItemsScript[First].Init(5, 2); FlareOn[2] = true; }
        }

        for (int i = 0; i < ExternalItems.Count; i++)
        {
            var r = Random.Range(0, 1f);
            if (ExternalProb[i] >= r)
            {
                foreach (var j in ExternalItems[i]) if (!j.activeSelf)
                    {
                        j.SetActive(true); j.transform.position = pos + new Vector3(-0.2f + r * 0.4f, 0.2f - r * 0.4f);
                        break;
                    }
            }
        }
    }

    List<List<GameObject>> ExternalItems = new List<List<GameObject>>();
    List<float> ExternalProb = new List<float>();

    public int MakeExternalItem(Sprite ItemSprite, int maxCount, float prob, int Target)
    {
        ExternalItems.Add(new List<GameObject>()); ExternalProb.Add(prob);
        int n = ExternalItems.Count - 1;
        for (int i = 0; i < maxCount; i++)
        {
            GameObject cnt = Instantiate(ItemPref, transform);
            cnt.name = $"{n}"; cnt.GetComponent<SpriteRenderer>().sprite = ItemSprite; cnt.GetComponent<Item>().Init(-1, 0, Target, n);
            ExternalItems[n].Add(cnt);
            cnt.SetActive(false);
        }
        return ExternalItems.Count - 1;
    }

    public void UpdateExternalItem(int num, Sprite sp = null, string name = null, float prob = -1, int Target = -1)
    {
        if (prob != -1) ExternalProb[num] = prob;
        if (sp != null || name != null || Target != -1)
        {
            foreach (var j in ExternalItems[num])
            {
                if (sp != null) j.GetComponent<SpriteRenderer>().sprite = sp;
                if (name != null) j.name = name;
                if (Target != -1) j.GetComponent<Item>().Init(-1, 0, Target);
            }
        }
    }

    public void RemoveItem(int ind)
    {
        if (CreatedTiming.Contains(ind)) CreatedTiming.RemoveAt(CreatedTiming.IndexOf(ind));
    }

    [HideInInspector] public int MagTime = 0;
    Coroutine Mag = null;

    public void MagStart()
    {
        MagTime = 5;
        if (Mag == null) Mag = StartCoroutine(MagCount());
    }

    IEnumerator MagCount()
    {
        for (int i = 0; i < MaxItem; i++) if (Items[i].activeSelf) ItemsScript[i].ApplyMag();
        while (MagTime > 0)
        {
            MagTime--;
            yield return GameManager.OneSec;
        }
        MagTime = 0;
        Mag = null;
    }

    [HideInInspector] public bool[] FlareOn = { false, false, false };
    SpriteRenderer[] FlareSprites = { null, null, null };

    public void MakeFlare(int ind) { StartCoroutine(FlareShoot(ind)); }

    IEnumerator FlareShoot(int ind)
    {
        if (FlareSprites[ind] == null) FlareSprites[ind] = Flare[ind].GetComponent<SpriteRenderer>();
        FlareSprites[ind].color = Color.white;
        Flare[ind].gameObject.SetActive(true); Flare[ind].position = GameManager.instance.player.Self.position; 
        for (int i = 0; i < 20; i++) { Flare[ind].Translate(0, 1f, 0); yield return GameManager.DotOneSec; }
        FlareSprites[ind].color = Vector4.zero; FlareSmoke[ind].Stop();
        yield return GameManager.TwoSec;
        Flare[ind].gameObject.SetActive(false);

        FlareOn[ind] = false ;
    }
}
