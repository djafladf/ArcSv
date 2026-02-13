using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BasePawn : MonoBehaviour
{
    bool OnIdle = true;
    int curRoom = 0;

    List<int> WorkPriorityQue;
    List<Vector2Int> WorkQue = new List<Vector2Int>();      // RoomID, WorkType

    Image im;
    Animator anim;

    private void Awake()
    {
        WorkPriorityQue = new List<int>{ 0, 1, 2};
        im = GetComponent<Image>();
        anim = GetComponent<Animator>();
        //Test

        CurGrid = new Vector2Int(-1, 0);
        CurDir = Vector2.zero;
        StartCoroutine(FindWork());
    }

    Queue<int> Dest = new Queue<int>();

    float speed = 1;

    Vector2Int CurGrid;
    [SerializeField] Vector2 CurDir;
    Vector2 SubDir;
    Vector3 CurDest;
    int CurDestGrid;

    void FixedUpdate()
    {
        if(CurDir != Vector2.zero)
        {
            transform.Translate(CurDir*speed * Time.deltaTime * 10,Space.World);
            if(Vector3.Magnitude(CurDest - transform.position) <= 5.5)
            {
                CurGrid = GameManager.instance._base.RoutesGrid[CurDestGrid];
                if (Dest.Count > 0) SetNextDir();
                else
                {
                    curRoom = GameManager.instance._base.Infos[WorkQue[0].x].routesid[1];
                    WorkQue.Clear();
                    CurDir = Vector2.zero; anim.SetBool("OnWalk", false); OnIdle = true;
                    StartCoroutine(FindWork());
                }
            }
        }
    }

    void SetNextDir()
    {
        var s = Dest.Dequeue();
        
        CurDestGrid = s; CurGrid = GameManager.instance._base.Vector2Grid(transform.position,false);
        CurDest = GameManager.instance._base.GridToVector(GameManager.instance._base.RoutesGrid[s],false);
        print($"{CurGrid} -> {s} : {GameManager.instance._base.RoutesGrid[s]}");
        var tmp = GameManager.instance._base.RoutesGrid[s] - CurGrid;
        SubDir = new Vector2(tmp.x == 0 ? 0 : Mathf.Sign(tmp.x), tmp.y == 0 ? 0 : Mathf.Sign(tmp.y));
        if (SubDir.x == 1 && transform.eulerAngles.y == 0) StartCoroutine(SpinFunc());
        else if (SubDir.x == -1 && transform.eulerAngles.y == 180) StartCoroutine(SpinFunc());
        else { CurDir = SubDir; anim.SetBool("OnWalk", true); }
        }

    IEnumerator SpinFunc()
    {
        var wfs = new WaitForSeconds(0.02f);
        for (int i = 0; i < 10; i++) { transform.Rotate(0, 18, 0); yield return wfs; }
        transform.rotation = Quaternion.Euler(0, SubDir.x == 1 ? 180 : 0, 0);
        CurDir = SubDir;
        anim.SetBool("OnWalk", true);
    }

    IEnumerator FindWork()
    {
        int l;
        while (OnIdle)
        {
            yield return GameManager.DotOneSec;
            foreach (var j in WorkPriorityQue)
            {
                if (!GameManager.instance._base.WorkQue[j].TryDequeue(out var res)) continue;
                print($"{curRoom} -> {res.Item2}");
                GameManager.instance._base.FindRoute(Dest, curRoom, res.Item2);
                WorkQue.Add(new Vector2Int(res.Item1,j)); OnIdle = false;
                SetNextDir();
                break; 
            }
            if (OnIdle) { }
        }
    }

}
