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
    public int LeftWork;
    public DateTime st;
    public List<Vector2Int> entrances;
    public Vector2Int[] points;
    public Vector2Int midgrid;
    [JsonIgnore] public GameObject room;
    [JsonIgnore] public BaseInfra script;

    public InfraInfo()
    {
        points = new Vector2Int[4];
        entrances = new List<Vector2Int>();
    }
}
