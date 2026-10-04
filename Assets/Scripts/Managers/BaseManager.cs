using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using UnityEngine.Purchasing;
using static Unity.Burst.Intrinsics.X86.Avx;

public class BaseManager : MonoBehaviour
{
    // OnBuild
    public GameObject Option_Build;
    [SerializeField] BaseBuilder _builder;
    [SerializeField] List<GameObject> InfraSet;

    [HideInInspector] public List<InfraInfo> Infos;
    public int CurSelected;

    [HideInInspector] public int[,] Occupied;
    [HideInInspector] public int[,] DoorAt;
    [HideInInspector] public RectTransform rect;


    // About Grid
    [HideInInspector] readonly public float GridSize = 90;
    [HideInInspector] public float rGridSize;
    [HideInInspector] readonly public float Startx = -900;
    readonly public float Starty = -630;

    [HideInInspector] readonly public int cor = 23, row = 13;
    private void Awake()
    {
        rGridSize = 1f / GridSize;
        GameManager.instance._base = this;
        Occupied = new int[cor, row]; DoorAt = new int[cor, row]; for (int x = 0; x < cor; x++) for (int y = 0; y < row; y++) { Occupied[x, y] = -1; DoorAt[x, y] = -1; }
        Infos = new List<InfraInfo>();
        rect = GetComponent<RectTransform>();
        Routes.Add(new List<int>()); RoutesGrid.Add(new Vector2Int(-1, 0));
        WorkQue = new List<ConcurrentQueue<(int, int)>>(); for (int i = 0; i < workN; i++) WorkQue.Add(new ConcurrentQueue<(int, int)>());
    }


    public Vector2 Grid2Vector(Vector2Int gd,bool IsWorld = false)
    {
        var sub = new Vector2(gd.x * GridSize + Startx, gd.y * GridSize + Starty);
        if(IsWorld) RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, sub, null, out sub);
        return sub;
    }

    public Vector2Int Vector2Grid(Vector2 vec) 
    {
        return new Vector2Int(Mathf.FloorToInt((vec.x - Startx) * rGridSize), Mathf.FloorToInt((vec.y - Starty) * rGridSize));
    }
    
    public Vector2Int Point2Grid()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, Input.mousePosition, null, out var CurInput);
        return new Vector2Int(Mathf.FloorToInt((CurInput.x - Startx) * rGridSize), Mathf.FloorToInt((CurInput.y - Starty) * rGridSize));
    }



    public void Register(Vector2Int St, Vector2Int Ed,GameObject obj)
    {
        
        var tmp = new InfraInfo();


        /*
         *  2  3
         *  0  1
         */

        tmp.points[0] = new Vector2Int(Mathf.Min(St.x, Ed.x), Mathf.Min(St.y, Ed.y)); tmp.points[1] = new Vector2Int(Mathf.Max(St.x, Ed.x), Mathf.Max(St.y, Ed.y));

        if (Mathf.Abs(St.x - Ed.x) < 1 || Mathf.Abs(St.y - Ed.y) < 1) tmp.type = 1;

        // Inst
        tmp.midgrid = new Vector2Int(Mathf.FloorToInt((tmp.points[1].x - tmp.points[0].x + 1) * 0.5f), Mathf.FloorToInt((tmp.points[1].y - tmp.points[0].y) * 0.5f));
        tmp.size = tmp.points[1] - tmp.points[0];
        tmp.id = Infos.Count; Infos.Add(tmp);
    }

    public void ShowOption(int ind)
    {
        CurSelected = ind; Option_Build.SetActive(false); Option_Build.SetActive(true);
    }

    public void SetMove()
    {
        _builder.SetMod(2, Infos[CurSelected].points[0],new Vector2Int(cor - 1 -Infos[CurSelected].points[1].x, row - 1  - Infos[CurSelected].points[1].y));
    }

    public void SetDelete()
    {
        _builder.SetMod(1, Infos[CurSelected].points[0], Infos[CurSelected].points[1]);
    }

    public void SetExpand()
    {
        _builder.SetMod(3);
    }

    public void MoveInfra()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, Input.mousePosition, null, out var CurInput);
        var St = Vector2Grid(CurInput) - Infos[CurSelected].midgrid;
        var Ed = new Vector2Int(St.x + Infos[CurSelected].size.x, St.y + Infos[CurSelected].size.y);
        int x, y;
        for ( y = St.y; y <= Ed.y; y++) for ( x = St.x; x <= Ed.x; x++) if (Occupied[x, y] != -1 && Occupied[x, y] != Infos[CurSelected].id) { Infos[CurSelected].room.GetComponent<RectTransform>().anchoredPosition = new Vector2(Infos[CurSelected].points[0].x * 80 + 95, Infos[CurSelected].points[0].y * 80 + 195); return; }
        for ( y = Infos[CurSelected].points[0].y; y <= Infos[CurSelected].points[1].y; y++) for ( x = Infos[CurSelected].points[0].x; x <= Infos[CurSelected].points[1].x; x++) Occupied[x, y] = -1;
        for ( y = St.y; y <= Ed.y; y++) for ( x = St.x; x <= Ed.x; x++) Occupied[x, y] = Infos[CurSelected].id;
        Infos[CurSelected].points[0] = St; Infos[CurSelected].points[1] = Ed;

        for (y = 0; y <= 1; y++) for (x = 0; x <= 1; x++) { Infos[CurSelected].points[x + y * 2].x = x == 0 ? St.x : Ed.x; Infos[CurSelected].points[x + y * 2].y = y == 0 ? St.y : Ed.y;  }
    }

    public void RemoveInfra()
    {
        for (int y = Infos[CurSelected].points[0].y; y <= Infos[CurSelected].points[1].y; y++) for (int x = Infos[CurSelected].points[0].x; x <= Infos[CurSelected].points[1].x; x++) Occupied[x, y] = -1;
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

    // OnWork
    [HideInInspector] public List<ConcurrentQueue<(int, int)>> WorkQue;     // Room Id, Route Id
    [HideInInspector] public List<Vector2Int> RoutesGrid = new List<Vector2Int>();
    [HideInInspector] public List<List<int>> Routes = new List<List<int>>();
    [HideInInspector] public int workN = 3;

    public void AddWork(int type,int id,int Routeid)
    {
        WorkQue[type].Enqueue((id,Routeid));
    }

    public bool FindRoute(Queue<int> que, int cur, int DestId)
    {
        int i = Routes.Count;
        bool[] visit = new bool[i];
        int[] From = new int[i];
        float[] cost = new float[i];
        for (int g = 0; g < i; g++) { cost[g] = float.PositiveInfinity; From[g] = -1; }
        PriorityQueue pq = new PriorityQueue();
        pq.push(new ARoute(cur,0,0));

        while (!pq.isEmpty)
        {
            var k = pq.top(); pq.pop();
            if (k.id == DestId) break;
            if (visit[k.id]) continue;
            visit[k.id] = true;
            foreach(var j in Routes[k.id])
            {
                if (visit[j]) continue;
                float score = (RoutesGrid[DestId] - RoutesGrid[j]).magnitude + k.cost + 1;  // huri + cost
                if (cost[j] <= k.cost+1) continue;
                cost[j] = k.cost+1;
                From[j] = k.id;
                pq.push(new ARoute(j,k.cost+1,score));
            }
        }
        i = DestId;
        Stack<int> sub = new Stack<int>();
        while (i != cur) { if (i == -1) { return false; } sub.Push(i); i = From[i]; }
        while (sub.Count > 0) que.Enqueue(sub.Pop());
        print(String.Join("->",que));
        return true;
    }


    

    

}
