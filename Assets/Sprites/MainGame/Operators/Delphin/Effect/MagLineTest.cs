using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MagLineTest : MonoBehaviour
{
    public Transform target;
    [SerializeField]LineRenderer line;

    private void Awake()
    {
        StartCoroutine(Test());
    }


    IEnumerator Test()
    {
        while (true)
        {
            int l = Random.Range(5, 8);
            line.positionCount = l;

            Vector3 start = transform.position;
            Vector3 end = target.position;

            Vector3 dir = (end - start);
            float len = dir.magnitude;

            Vector3 n = new Vector3(-dir.y, dir.x, 0f).normalized;

            float seed = Random.value * 1000f;

            // 시작/끝 고정
            line.SetPosition(0, start);
            line.SetPosition(l - 1, end);

            for (int i = 1; i < l - 1; i++)
            {
                float u = (float)i / (l - 1); // 0~1

                float offset = (Mathf.PerlinNoise(seed, u * 10f + Time.time * 2f) - 0.5f) * 2f;

                float amp = Random.Range(0.1f, 0.5f); // 너가 쓰던 진폭
                Vector3 p = Vector3.Lerp(start, end, u) + n * (offset * amp);

                line.SetPosition(i, p);
            }

            yield return GameManager.DotOneSec;
        }
    }

    static float Hash01(float x)
    {
        // 0~1
        return Mathf.Repeat(Mathf.Sin(x) * 43758.5453f, 1f);
    }
    static float Hash11(float x)
    {
        // -1~1
        return Hash01(x) * 2f - 1f;
    }

    private void Update()
    {
        
    }
}
