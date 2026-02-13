using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InfraInfo
{
    public int id;
    // 0 : 건설 중, 1 : 통로, 2 : 
    public int type;
    public Vector2Int[] points;
    public List<Vector2Int> doors;
    public List<int> routesid;
    public Vector2Int midgrid;
    [JsonIgnore] public GameObject room;
    [JsonIgnore] public BaseInfra script;

    public InfraInfo()
    {
        points = new Vector2Int[4];
        doors = new List<Vector2Int>();
        routesid = new List<int>();
    }
}

public struct ARoute
{
    public int id;
    public float cost;
    public float sum;
    public ARoute(int i, float c, float h) { id = i; cost = c; sum = h; }
    public static bool operator <(ARoute a, ARoute b) => a.sum < b.sum;
    public static bool operator >(ARoute a, ARoute b) => a.sum > b.sum;
    public static bool operator <=(ARoute a, ARoute b) => a.sum <= b.sum;
    public static bool operator >=(ARoute a, ARoute b) => a.sum >= b.sum;
}

public class PriorityQueue
{
    List<ARoute> ele = new List<ARoute>();
    public int Count => ele.Count;
    public bool isEmpty => ele.Count == 0;
    public void Clear() => ele.Clear();
    public ARoute top()
    {
        if(ele.Count == 0) throw new InvalidOperationException("empty");
        return ele[0];
    }

    public void push(ARoute x) 
    { 
        ele.Add(x);
        int i = ele.Count - 1;
        while (true)
        {
            int p = (i - 1) >> 1;
            if (i <= 0 || ele[i] < ele[p]) break;
            (ele[i], ele[p]) = (ele[p], ele[i]);
            i = p;
        }
    }
    public ARoute pop()
    {
        if (ele.Count == 0) throw new InvalidOperationException("empty");
        ARoute top = ele[0];
        int lr = ele.Count - 1;
        ele[0] = ele[lr]; ele.RemoveAt(lr);
        if(ele.Count>0)
        {
            int i = 0;
            while (true)
            {
                int l = i * 2 + 1, r = l + 1, m = i;
                if (l < ele.Count && ele[l] < ele[m]) m = l;
                if (r < ele.Count && ele[r] < ele[m]) m = r;
                if (m == i) break;
                (ele[i], ele[m]) = (ele[m], ele[i]); i = m;
            }
        }
        return top;
    }
}
