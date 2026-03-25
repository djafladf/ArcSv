using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ripple : MonoBehaviour
{
    [SerializeField] CustomRenderTexture CRT;
    [SerializeField] Material RipMat;
    Vector4[] Pos = new Vector4[16];

    private void Start()
    {
        StartCoroutine(Test());
    }


    IEnumerator Test()
    {
        CRT.Initialize();
        while (true)
        {
            yield return GameManager.DotOneSec;
            var sub = new Vector4(Random.Range(0, 2), Random.Range(0, 2), 1, 0);
            RipMat.SetVector("_SpawnPos", sub);
            yield return GameManager.DotOneSec;
            RipMat.SetVector("_SpawnPos", Vector4.zero);
        }
    }
}
