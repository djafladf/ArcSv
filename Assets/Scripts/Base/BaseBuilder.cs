using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
public class BaseBuilder : MonoBehaviour
{
    [SerializeField] Grid gr;
    [SerializeField] List<Tilemap> Hals;
    [SerializeField] List<Tilemap> Rooms;
    [SerializeField] List<RuleTile> Tiles;
    [SerializeField] RectTransform Dragger;
    [SerializeField] List<Tile> OnBuild;

    [SerializeField] Transform RoomGrid;
    [SerializeField] GameObject RoomSet;

    BaseManager _base;

    bool IsHal = true;
    int CurBuildType = 0;
    int CurTile = -1;

    /// <summary>
    /// </summary>
    /// <param name="type">0 : build, 1 : del, 2: move, 3: expand</param>
    public void SetMod(int type, Vector2Int s = default, Vector2Int e = default)
    {
        CurBuildType = type; AllowInt_S = s; if (e == default) AllowInt_E = new Vector2Int(_base.cor, _base.row);
        //if (type == 2)
        //{
        //    Cur = GameManager.instance._base.Infos[GameManager.instance._base.CurSelected].room.GetComponent<RectTransform>();
        //    RectTransformUtility.ScreenPointToLocalPointInRectangle(GameManager.instance._base.rect, Input.mousePosition, null, out var inp);
        //    CurGrid = SubGrid = new Vector2Int(Mathf.FloorToInt((inp.x - 95) * rGridSize) + 16, Mathf.FloorToInt((inp.y - 195) * rGridSize) + 9);
        //    Cur.anchoredPosition = new Vector2((CurGrid.x - GameManager.instance._base.Infos[GameManager.instance._base.CurSelected].midgrid.x) * GridSize + 95,
        //    (CurGrid.y - GameManager.instance._base.Infos[GameManager.instance._base.CurSelected].midgrid.y) * GridSize + 195);
        //}
    }
    Vector3Int TileStart;

    void Start()
    {
        _base = GameManager.instance._base;
        TileStart = gr.WorldToCell(new Vector3(-14, -10, 0));
        if (!TryGetComponent<EventTrigger>(out var ET)) { gameObject.AddComponent<EventTrigger>(); ET = GetComponent<EventTrigger>(); }
        AddEvent(ET, EventTriggerType.PointerDown, PointerDown);
        AddEvent(ET, EventTriggerType.PointerUp, PointerUp);
        AddEvent(ET, EventTriggerType.Drag, OnPoint);
        SetMod(0);

        //gameObject.SetActive(false);
    }

    void AddEvent(EventTrigger eventTrigger, EventTriggerType Type, Action<PointerEventData> Event)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = Type;
        entry.callback.AddListener((data) => { Event((PointerEventData)data); });
        eventTrigger.triggers.Add(entry);
    }

    Vector2 SubVector;

    Vector2Int AllowInt_S;
    Vector2Int AllowInt_E;

    Vector2Int StGrid;
    Vector2Int SubGrid;
    Vector2Int CurGrid;

    void PointerDown(PointerEventData data)
    {
        if (CurBuildType == 2) return;
        StGrid = _base.Point2Grid();
        if (data.button == PointerEventData.InputButton.Left)
        {
            SubGrid = StGrid;
            if (_base.Occupied[StGrid.x, StGrid.y] != -1 && CurBuildType == 0) return;
            if (StGrid.x < AllowInt_S.x || StGrid.x > AllowInt_E.x || StGrid.y < AllowInt_S.y || StGrid.y > AllowInt_E.y) return;
            Dragger.anchoredPosition = new Vector2(StGrid.x * 1.5f, StGrid.y * 1.5f); Dragger.sizeDelta = new Vector2(1.5f, 1.5f); Dragger.gameObject.SetActive(true);
            OnDrag = true;
        }
        //else if (data.button == PointerEventData.InputButton.Right) if (GameManager.instance._base.Occupied[StGrid.x, StGrid.y] != -1)  GameManager.instance._base.ShowOption(GameManager.instance._base.Occupied[StGrid.x, StGrid.y]);
    }
    readonly float wGridSize = 1.5f;

    void PointerUp(PointerEventData data)
    {
        if (data.button != PointerEventData.InputButton.Left || !OnDrag) return;
        switch (CurBuildType)
        {
            case 0:
                GameObject rej = null;
                if (IsHal)
                {
                    rej = Instantiate(RoomSet, RoomGrid); Hals.Add(rej.GetComponent<Tilemap>());
                    int mx = Mathf.Min(CurGrid.x, StGrid.x), my = Mathf.Min(CurGrid.y, StGrid.y);
                    int Mx = Mathf.Max(CurGrid.x, StGrid.x), My = Mathf.Max(CurGrid.y, StGrid.y);
                    Tilemap ct = Hals[Hals.Count - 1];
                    
                    for (; my <= My; my++) for (int x = mx; x <= Mx; x++) ct.SetTile(new Vector3Int(x + TileStart.x, my + TileStart.y), OnBuild[0]); 
                }
                _base.Register(StGrid, CurGrid,rej);
                break;
            case 1: 
                break;
            case 2: 
                break;
            case 3: 
                break;
        }
        if (CurBuildType == 0) { }
        else if (CurBuildType == 1) GameManager.instance._base.DeleteInfra(StGrid, CurGrid);
        else if (CurBuildType == 2) { GameManager.instance._base.MoveInfra(); SetMod(0); return; }
        else if (CurBuildType == 3) { GameManager.instance._base.ExpandInfra(StGrid, CurGrid); }
        Dragger.gameObject.SetActive(false);
        SetMod(0);
    }

    //public void OnPointerMove(PointerEventData eventData)
    //{
    //    if (CurBuildType != 2) return;
    //    CurGrid = GameManager.instance._base.Point2Grid();
    //    if (CurGrid == SubGrid) return;
    //    SubGrid = CurGrid;
    //    Cur.anchoredPosition = new Vector2((CurGrid.x - GameManager.instance._base.Infos[GameManager.instance._base.CurSelected].midgrid.x) * GridSize + Startx,
    //        (CurGrid.y - GameManager.instance._base.Infos[GameManager.instance._base.CurSelected].midgrid.y) * GridSize + Starty);
    //}

    bool OnDrag = false;
    void OnPoint(PointerEventData data)
    {
        if (!OnDrag) return;
        if (CurBuildType == 2)
        {

        }
        else
        {
            if (data.button != PointerEventData.InputButton.Left) return;
            if (StGrid.x < AllowInt_S.x || StGrid.x > AllowInt_E.x || StGrid.y < AllowInt_S.y || StGrid.y > AllowInt_E.y) return;
            CurGrid = _base.Point2Grid();
            if (SubGrid == CurGrid) return;
            if (CurGrid.x != SubGrid.x)
            {
                int minY = Mathf.Min(StGrid.y, CurGrid.y), maxY = Mathf.Max(StGrid.y, CurGrid.y);
                int sx = Math.Sign(CurGrid.x - SubGrid.x);
                if (CurBuildType == 3) { for (int x = SubGrid.x + sx; x != CurGrid.x + sx; x += sx) for (int y = minY; y <= maxY; y++) if (_base.Occupied[x, y] != -1 & _base.Occupied[x, y] != _base.CurSelected) return; }
                else { for (int x = SubGrid.x + sx; x != CurGrid.x + sx; x += sx) for (int y = minY; y <= maxY; y++) { if (_base.Occupied[x, y] != -1 ^ (CurBuildType != 0)) return; } }
            }
            if (CurGrid.y != SubGrid.y)
            {
                int minX = Mathf.Min(StGrid.x, CurGrid.x), maxX = Mathf.Max(StGrid.x, CurGrid.x);
                int sy = Math.Sign(CurGrid.y - SubGrid.y);
                if (CurBuildType != 3) for (int y = SubGrid.y + sy; y != CurGrid.y + sy; y += sy) for (int x = minX; x <= maxX; x++) { if (GameManager.instance._base.Occupied[x, y] != -1 ^ (CurBuildType != 0)) return; }// °ãÄ§
                else for (int y = SubGrid.y + sy; y != CurGrid.y + sy; y += sy) for (int x = minX; x <= maxX; x++) if (GameManager.instance._base.Occupied[x, y] != -1 & GameManager.instance._base.Occupied[x, y] != GameManager.instance._base.CurSelected) return;
            }
            SubGrid = CurGrid;
            Dragger.anchoredPosition = new Vector2(Mathf.Min(StGrid.x,CurGrid.x) * 1.5f, Mathf.Min(StGrid.y,CurGrid.y) * 1.5f);
            Dragger.sizeDelta = new Vector2(Mathf.Abs(StGrid.x - CurGrid.x) * 1.5f + 1.5f,Mathf.Abs(StGrid.y - CurGrid.y) * 1.5f + 1.5f);
        }
    }


}
