using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parat : Enemy
{

    [SerializeField] ParticleSystem Ps1;
    [SerializeField] ParticleSystem Ps2;
    bool FirstOn = true;
    protected override void OnEnable()
    {
        if (FirstOn) { FirstOn = false; return; }
        base.OnEnable();
        int z = Random.Range(0, 4);
        if (z <= 1)
            transform.position = GameManager.instance.player.Self.position + new Vector3(Random.Range(-5, 5), (z == 0 ? -5 : 5));
        else
            transform.position = GameManager.instance.player.Self.position + new Vector3((z == 0 ? -5 : 5), Random.Range(-5, 5));

        MoveAble = false;
        Ps1.Play();
        Ps2.Play();
    }


    void EndReBatch()
    {
        MoveAble = true;
        Ps1.Stop();
        Ps2.Stop();
    }
}
