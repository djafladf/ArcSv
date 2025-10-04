using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using UnityEngine.Purchasing;
using static Unity.Burst.Intrinsics.X86.Avx;

public class BaseManager : MonoBehaviour
{
    public GameObject Option_Build;
    [SerializeField] BaseBuilder _builder;
    [SerializeField] List<GameObject> InfraSet;

    [HideInInspector] public List<InfraInfo> Infos;
    public int CurSelected;

    [HideInInspector] public int[,] Occupied;
    [HideInInspector] public RectTransform rect;

    public void Register(Vector2Int St, Vector2Int Ed)
    {
        
        var tmp = new InfraInfo();
        tmp.points[0] = new Vector2Int(Mathf.Min(St.x, Ed.x), Mathf.Min(St.y, Ed.y)); tmp.points[1] = new Vector2Int(Mathf.Max(St.x, Ed.x), Mathf.Min(St.y, Ed.y));
        tmp.points[2] = new Vector2Int(Mathf.Min(St.x, Ed.x), Mathf.Max(St.y, Ed.y)); tmp.points[3] = new Vector2Int(Mathf.Max(St.x, Ed.x), Mathf.Max(St.y, Ed.y));
        tmp.st = DateTime.Now;
        if (Mathf.Abs(St.x - Ed.x) < 1 || Mathf.Abs(St.y - Ed.y) < 1) tmp.type = 1;
        var sub = Instantiate(InfraSet[0], transform).GetComponent<RectTransform>();
        sub.anchoredPosition = new Vector2(tmp.points[0].x * 80 + 95, tmp.points[0].y * 80 + 195);
        int x = (tmp.points[1].x - tmp.points[0].x + 1), y = (tmp.points[2].y - tmp.points[0].y + 1);
        sub.sizeDelta = new Vector2((tmp.points[1].x - tmp.points[0].x + 1) * 80, (tmp.points[2].y - tmp.points[0].y + 1) * 80);
        tmp.midgrid = new Vector2Int(Mathf.FloorToInt((tmp.points[1].x - tmp.points[0].x + 1) * 0.5f), Mathf.FloorToInt((tmp.points[2].y - tmp.points[0].y) * 0.5f));
        tmp.room = sub.gameObject; tmp.id = Infos.Count; tmp.script = sub.GetComponent<BaseInfra>(); Infos.Add(tmp);
        for (y = tmp.points[0].y; y <= tmp.points[3].y; y++) for (x = tmp.points[0].x; x <= tmp.points[1].x; x++) Occupied[x, y] = tmp.id;
    }

    public void ShowOption(int ind)
    {
        CurSelected = ind; Option_Build.SetActive(false); Option_Build.SetActive(true);
    }

    public void SetMove()
    {
        _builder.SetMod(2, Infos[CurSelected].points[0],new Vector2Int(29 -Infos[CurSelected].points[3].x, 14 - Infos[CurSelected].points[3].y));
    }

    public void SetDelete()
    {
        _builder.SetMod(1, Infos[CurSelected].points[0], Infos[CurSelected].points[3]);
    }

    public void SetExpand()
    {
        _builder.SetMod(3);
    }

    public void MoveInfra()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, Input.mousePosition, null, out var CurInput);
        var St = new Vector2Int(Mathf.FloorToInt((CurInput.x - 95) * 0.0125f) + 16 - Infos[CurSelected].midgrid.x, Mathf.FloorToInt((CurInput.y - 195) * 0.0125f) + 9 - Infos[CurSelected].midgrid.y);
        var Ed = new Vector2Int(St.x + Infos[CurSelected].points[1].x - Infos[CurSelected].points[0].x, St.y + Infos[CurSelected].points[2].y - Infos[CurSelected].points[0].y);
        int x, y;
        for ( y = St.y; y <= Ed.y; y++) for ( x = St.x; x <= Ed.x; x++) if (Occupied[x, y] != -1 && Occupied[x, y] != Infos[CurSelected].id) { Infos[CurSelected].room.GetComponent<RectTransform>().anchoredPosition = new Vector2(Infos[CurSelected].points[0].x * 80 + 95, Infos[CurSelected].points[0].y * 80 + 195); return; }
        for ( y = Infos[CurSelected].points[0].y; y <= Infos[CurSelected].points[3].y; y++) for ( x = Infos[CurSelected].points[0].x; x <= Infos[CurSelected].points[1].x; x++) Occupied[x, y] = -1;
        for ( y = St.y; y <= Ed.y; y++) for ( x = St.x; x <= Ed.x; x++) Occupied[x, y] = Infos[CurSelected].id;
        for (y = 0; y <= 1; y++) for (x = 0; x <= 1; x++) { Infos[CurSelected].points[x + y * 2].x = x == 0 ? St.x : Ed.x; Infos[CurSelected].points[x + y * 2].y = y == 0 ? St.y : Ed.y;  }
    }

    public void RemoveInfra()
    {
        for (int y = Infos[CurSelected].points[0].y; y <= Infos[CurSelected].points[3].y; y++) for (int x = Infos[CurSelected].points[0].x; x <= Infos[CurSelected].points[1].x; x++) Occupied[x, y] = -1;
        Destroy(Infos[CurSelected].room); Infos.RemoveAt(CurSelected); for (int i = 0; i < Infos.Count; i++) Infos[i].id = i;
    }

    public void ExpandInfra(Vector2Int St, Vector2Int Ed)
    {
        Vector2Int[] Sub = new Vector2Int[2]; 
        Sub[0] = new Vector2Int(Mathf.Min(St.x, Ed.x), Mathf.Min(St.y, Ed.y)); Sub[1] = new Vector2Int(Mathf.Max(St.x, Ed.x), Mathf.Max(St.y, Ed.y));
        Vector2Int[] Sub_x = new Vector2Int[4], Sub_y = new Vector2Int[4];
        Sub_x[0].x = Sub_x[2].x = Infos[CurSelected].points[0].x - 1; Sub_x[1].x = Sub_x[3].x = Infos[CurSelected].points[1].x + 1;
        Sub_x[0].y = Sub_x[1].y = Infos[CurSelected].points[0].y; Sub_x[2].y = Sub_x[3].y = Infos[CurSelected].points[2].y;

        Sub_y[0].x = Sub_y[2].x = Infos[CurSelected].points[0].x; Sub_y[1].x = Sub_y[3].x = Infos[CurSelected].points[1].x;
        Sub_y[0].y = Sub_y[1].y = Infos[CurSelected].points[0].y - 1; Sub_y[2].y = Sub_y[3].y = Infos[CurSelected].points[2].y+1;

        int j_x = 0, j_y = 0;
        for (int i = 0; i < 4; i++) 
        { 
            if (Sub[0].x <= Sub_x[i].x && Sub_x[i].x <= Sub[1].x && Sub[0].y <= Sub_x[i].y && Sub_x[i].y <= Sub[1].y) j_x++;
            if (Sub[0].x <= Sub_y[i].x && Sub_y[i].x <= Sub[1].x && Sub[0].y <= Sub_y[i].y && Sub_y[i].y <= Sub[1].y) j_y++;
        }

        if (j_x < (Sub[0].x == Infos[CurSelected].points[0].x && Sub[1].x == Infos[CurSelected].points[1].x ? 0 : 2) 
            || j_y < (Sub[0].y == Infos[CurSelected].points[0].y && Sub[1].y == Infos[CurSelected].points[2].y ? 0 : 2)) return;

        Infos[CurSelected].points[0].x = Infos[CurSelected].points[2].x = Mathf.Min(Sub[0].x, Infos[CurSelected].points[0].x);
        Infos[CurSelected].points[1].x = Infos[CurSelected].points[3].x = Mathf.Max(Sub[1].x, Infos[CurSelected].points[1].x);
        Infos[CurSelected].points[0].y = Infos[CurSelected].points[1].y = Mathf.Min(Sub[0].y, Infos[CurSelected].points[0].y);
        Infos[CurSelected].points[2].y = Infos[CurSelected].points[3].y = Mathf.Max(Sub[1].y, Infos[CurSelected].points[2].y);
        var cnt = Infos[CurSelected].room.GetComponent<RectTransform>();
        cnt.sizeDelta = new Vector2((Infos[CurSelected].points[1].x - Infos[CurSelected].points[0].x + 1) * 80,(Infos[CurSelected].points[2].y - Infos[CurSelected].points[0].y + 1) * 80);
        cnt.anchoredPosition = new Vector2(Infos[CurSelected].points[0].x * 80 + 95, Infos[CurSelected].points[0].y * 80 + 195);
        Infos[CurSelected].midgrid = new Vector2Int(Mathf.FloorToInt((Infos[CurSelected].points[1].x - Infos[CurSelected].points[0].x + 1) * 0.5f), Mathf.FloorToInt((Infos[CurSelected].points[2].y - Infos[CurSelected].points[0].y) * 0.5f));
        for (int y = Infos[CurSelected].points[0].y; y <= Infos[CurSelected].points[3].y; y++) for (int x = Infos[CurSelected].points[0].x; x <= Infos[CurSelected].points[1].x; x++) Occupied[x, y] = Infos[CurSelected].id;
    }

    public void DeleteInfra(Vector2Int St, Vector2Int Ed)
    {
        Vector2Int[] Sub = new Vector2Int[4];
        Sub[2] = new Vector2Int(Mathf.Min(St.x, Ed.x), Mathf.Max(St.y, Ed.y)); Sub[3] = new Vector2Int(Mathf.Max(St.x, Ed.x), Mathf.Max(St.y, Ed.y));
        Sub[0] = new Vector2Int(Mathf.Min(St.x, Ed.x), Mathf.Min(St.y, Ed.y)); Sub[1] = new Vector2Int(Mathf.Max(St.x, Ed.x), Mathf.Min(St.y, Ed.y));
        int l = 0;
        for (int i = 0; i < 4; i++) if (Sub[i] == Infos[CurSelected].points[i]) l++;
        if (l < 2) { GameManager.instance.FloatM.TimeShow(1, "방은 <color=red>직사각형</color>이여야 합니다."); return; }
        if ((Infos[CurSelected].points[1].x - Infos[CurSelected].points[0].x) < 1 || (Infos[CurSelected].points[2].y - Infos[CurSelected].points[0].y + 1) < 1) 
        { GameManager.instance.FloatM.TimeShow(1, "방은 <color=red>2x2 이상</color>이여야 합니다."); return; }


        if (Sub[0].x != Infos[CurSelected].points[0].x | Sub[1].x != Infos[CurSelected].points[1].x)
        {
            if (Sub[1].x < Infos[CurSelected].points[1].x) Infos[CurSelected].points[0].x = Infos[CurSelected].points[2].x = Sub[1].x + 1;  // Del L or R
            else Infos[CurSelected].points[1].x = Infos[CurSelected].points[3].x = Sub[0].x - 1;
        }

        if (Sub[0].y != Infos[CurSelected].points[0].y | Sub[2].y != Infos[CurSelected].points[2].y)
        {
            if (Sub[2].y < Infos[CurSelected].points[2].y) Infos[CurSelected].points[0].y = Infos[CurSelected].points[1].y = Sub[2].y + 1;
            else Infos[CurSelected].points[2].y = Infos[CurSelected].points[3].y = Sub[0].y - 1;
        }

        for (int y = Sub[0].y; y <= Sub[3].y; y++) for (int x = Sub[0].x; x <= Sub[1].x; x++) Occupied[x, y] = -1;
        Infos[CurSelected].room.GetComponent<RectTransform>().sizeDelta = new Vector2((Infos[CurSelected].points[1].x - Infos[CurSelected].points[0].x + 1) * 80,
            (Infos[CurSelected].points[2].y - Infos[CurSelected].points[0].y + 1) * 80);
        Infos[CurSelected].midgrid = new Vector2Int(Mathf.FloorToInt((Infos[CurSelected].points[1].x - Infos[CurSelected].points[0].x + 1) * 0.5f), Mathf.FloorToInt((Infos[CurSelected].points[2].y - Infos[CurSelected].points[0].y) * 0.5f));
    }
    private void Awake()
    {
        GameManager.instance._base = this;
        Occupied = new int[30, 15]; for (int x = 0; x < 30; x++) for (int y = 0; y < 15; y++) Occupied[x, y] = -1;
        Infos = new List<InfraInfo>();
        rect = GetComponent<RectTransform>();
    }

}
