using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagnetField : MonoBehaviour
{
    [SerializeField] List<Transform> Rounder;
    List<SpriteRenderer> spr = new List<SpriteRenderer>();
    float[] Thetas = { 270 * Mathf.Deg2Rad, 30 * Mathf.Deg2Rad, 150 * Mathf.Deg2Rad };

    private void Start()
    {
        
        for (int i = 0; i < 3; i++) 
        {
            Rounder[i].localPosition = new Vector3(Mathf.Cos(Thetas[i]) * 2.8f, Mathf.Sin(Thetas[i]) * 2.3f);
            spr.Add(Rounder[i].GetComponent<SpriteRenderer>());
        }
    }

    private void Update()
    {
        speed = Mathf.Deg2Rad * Time.deltaTime * speedvar;
        for (int i = 0; i < 3; i++)
        {
            Thetas[i] += speed; if (Thetas[i] >= PI2) Thetas[i] -= PI2;
            if (Thetas[i] >= 0 && Thetas[i] <= Mathf.PI) spr[i].sortingOrder = 4;
            else spr[i].sortingOrder = 5;
                Rounder[i].localPosition = new Vector3(Mathf.Cos(Thetas[i]) * 2.8f, Mathf.Sin(Thetas[i]) * 2.3f);
        }
    }
    public float speedvar = 10;
    float speed = Mathf.Deg2Rad;
    float PI2 = Mathf.PI*2;
}
