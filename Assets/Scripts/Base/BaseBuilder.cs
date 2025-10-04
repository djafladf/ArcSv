using System;
using UnityEngine;
using UnityEngine.EventSystems;
public class BaseBuilder : MonoBehaviour, IPointerMoveHandler
{
    [SerializeField] GameObject AreaSet;

    int CurBuildType = 0;

    float rGridSize;

    RectTransform _parent;
    RectTransform Cur = null;
    float GridSize = 80;

    /// <summary>
    /// </summary>
    /// <param name="type">0 : build, 1 : del, 2: move, 3: change</param>
    public void SetMod(int type, Vector2Int s = default, Vector2Int e = default)
    {
        CurBuildType = type; AllowInt_S = s; if (e == default) AllowInt_E = new Vector2Int(29, 14);
        if (type == 2)
        {
            Cur = GameManager.instance._base.Infos[GameManager.instance._base.CurSelected].room.GetComponent<RectTransform>();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(GameManager.instance._base.rect, Input.mousePosition, null, out var inp);
            CurGrid = SubGrid = new Vector2Int(Mathf.FloorToInt((inp.x - 95) * rGridSize) + 16, Mathf.FloorToInt((inp.y - 195) * rGridSize) + 9);
            Cur.anchoredPosition = new Vector2((CurGrid.x - GameManager.instance._base.Infos[GameManager.instance._base.CurSelected].midgrid.x) * GridSize + 95,
            (CurGrid.y - GameManager.instance._base.Infos[GameManager.instance._base.CurSelected].midgrid.y) * GridSize + 195);
        }
    }

    void Awake()
    {
        GridSize = 80; rGridSize = 1f / GridSize;
        _parent = transform.parent.GetComponent<RectTransform>();
        if (!TryGetComponent<EventTrigger>(out var ET)) { gameObject.AddComponent<EventTrigger>(); ET = GetComponent<EventTrigger>(); }
        AddEvent(ET, EventTriggerType.PointerDown, PointerDown);
        AddEvent(ET, EventTriggerType.PointerUp, PointerUp);
        AddEvent(ET, EventTriggerType.Drag, OnPoint);
        SetMod(0);
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
        if (data.button == PointerEventData.InputButton.Left)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_parent, Input.mousePosition, null, out var CurInput);
            StGrid = new Vector2Int(Mathf.FloorToInt((CurInput.x - 95) * rGridSize) + 16, Mathf.FloorToInt((CurInput.y - 195) * rGridSize) + 9);
            SubGrid = StGrid;
            if (GameManager.instance._base.Occupied[StGrid.x, StGrid.y] != -1 && CurBuildType == 0) return;
            if (StGrid.x < AllowInt_S.x || StGrid.x > AllowInt_E.x || StGrid.y < AllowInt_S.y || StGrid.y > AllowInt_E.y) return;
            Cur = Instantiate(AreaSet, _parent).GetComponent<RectTransform>();
            Cur.anchoredPosition = new Vector2(StGrid.x * GridSize + 95, StGrid.y * GridSize + 195);
        }
        else if (data.button == PointerEventData.InputButton.Right)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_parent, Input.mousePosition, null, out var CurInput);
            StGrid = new Vector2Int(Mathf.FloorToInt((CurInput.x - 95) * rGridSize) + 16, Mathf.FloorToInt((CurInput.y - 195) * rGridSize) + 9);
            if (GameManager.instance._base.Occupied[StGrid.x, StGrid.y] != -1) GameManager.instance._base.ShowOption(GameManager.instance._base.Occupied[StGrid.x, StGrid.y]);
        }
    }

    void PointerUp(PointerEventData data)
    {
        
        if (data.button != PointerEventData.InputButton.Left || Cur == null) return;
        if (CurBuildType == 0) { if (Cur.sizeDelta.x != 0) GameManager.instance._base.Register(StGrid, CurGrid); }
        else if (CurBuildType == 1) GameManager.instance._base.DeleteInfra(StGrid, CurGrid);
        else if (CurBuildType == 2) { GameManager.instance._base.MoveInfra(); SetMod(0); return; }
        else if (CurBuildType == 3) { GameManager.instance._base.ExpandInfra(StGrid, CurGrid); }
        SetMod(0);
        Destroy(Cur.gameObject);
    }

    void OnPoint(PointerEventData data)
    {
        if (CurBuildType == 2) return;
        if (data.button != PointerEventData.InputButton.Left || Cur == null) return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_parent, Input.mousePosition, null, out var CurInput);
        CurGrid = new Vector2Int(Mathf.FloorToInt((CurInput.x - 95) * rGridSize) + 16, Mathf.FloorToInt((CurInput.y - 195) * rGridSize) + 9);
        if (StGrid.x < AllowInt_S.x || StGrid.x > AllowInt_E.x || StGrid.y < AllowInt_S.y || StGrid.y > AllowInt_E.y) return;
        if (SubGrid == CurGrid) return;
        if (CurGrid.x != SubGrid.x)
        {
            int minY = Mathf.Min(StGrid.y, CurGrid.y), maxY = Mathf.Max(StGrid.y, CurGrid.y);
            int sx = Math.Sign(CurGrid.x - SubGrid.x);
            if (CurBuildType != 3) for (int x = SubGrid.x + sx; x != CurGrid.x + sx; x += sx) for (int y = minY; y <= maxY; y++) { if (GameManager.instance._base.Occupied[x, y] != -1 ^ (CurBuildType != 0)) return; }
            else for (int x = SubGrid.x + sx; x != CurGrid.x + sx; x += sx) for (int y = minY; y <= maxY; y++) if (GameManager.instance._base.Occupied[x, y] != -1 & GameManager.instance._base.Occupied[x, y] != GameManager.instance._base.CurSelected) return;
        }
        if (CurGrid.y != SubGrid.y)
        {
            int minX = Mathf.Min(StGrid.x, CurGrid.x), maxX = Mathf.Max(StGrid.x, CurGrid.x);
            int sy = Math.Sign(CurGrid.y - SubGrid.y);
            if (CurBuildType != 3) for (int y = SubGrid.y + sy; y != CurGrid.y + sy; y += sy) for (int x = minX; x <= maxX; x++) { if (GameManager.instance._base.Occupied[x, y] != -1 ^ (CurBuildType != 0)) return; }// °ãÄ§
            else for (int y = SubGrid.y + sy; y != CurGrid.y + sy; y += sy) for (int x = minX; x <= maxX; x++) if (GameManager.instance._base.Occupied[x, y] != -1 & GameManager.instance._base.Occupied[x, y] != GameManager.instance._base.CurSelected) return;
        }

        SubGrid = CurGrid;
        var SubPivot = new Vector2(CurGrid.x < StGrid.x ? 1 : 0, CurGrid.y < StGrid.y ? 1 : 0);

        if (Cur.pivot != SubPivot) { Cur.anchoredPosition = new Vector2(StGrid.x * GridSize + 95 + (SubPivot.x == 1 ? 80 : 0), StGrid.y * GridSize + 195 + (SubPivot.y == 1 ? 80 : 0)); Cur.pivot = SubPivot; }

        Cur.sizeDelta = new Vector2(Mathf.Abs(CurGrid.x - StGrid.x) * GridSize + GridSize,
            Mathf.Abs(CurGrid.y - StGrid.y) * GridSize + GridSize);
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (CurBuildType != 2) return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_parent, Input.mousePosition, null, out var CurInput);
        CurGrid = new Vector2Int(Mathf.FloorToInt((CurInput.x - 95) * rGridSize) + 16, Mathf.FloorToInt((CurInput.y - 195) * rGridSize) + 9);
        //if (CurGrid.x < AllowInt_S.x || CurGrid.x > AllowInt_E.x || CurGrid.y < AllowInt_S.y || CurGrid.y > AllowInt_E.y) return;
        if (CurGrid == SubGrid) return;
        SubGrid = CurGrid;
        Cur.anchoredPosition = new Vector2((CurGrid.x - GameManager.instance._base.Infos[GameManager.instance._base.CurSelected].midgrid.x)*GridSize + 95, 
            (CurGrid.y - GameManager.instance._base.Infos[GameManager.instance._base.CurSelected].midgrid.y)*GridSize + 195);
    }
}
