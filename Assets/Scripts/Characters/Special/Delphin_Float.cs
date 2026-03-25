using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Delphin_Float : MonoBehaviour
{
    [SerializeField] Delphin main;
    [SerializeField] int ind;

    void DeadReact()
    {
        gameObject.SetActive(false);
    }
}
