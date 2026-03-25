using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sora_Special : MonoBehaviour
{
    [SerializeField] Sora Sora;
    [SerializeField] Sprite sp;

    Dictionary<GameObject, bool> IsOn = new();

    private void Awake()
    {
        StartCoroutine(MakeBuff());
        foreach (var j in GameManager.instance.Prefs) IsOn[j] = false;
    }

    IEnumerator MakeBuff()
    {
        while (gameObject.activeSelf)
        {
            foreach(var j in IsOn)
            {
                if(j.Value) GameManager.instance.GetScript(j.Key).SetBuff(Sora.NormalInfo);
                yield return GameManager.DotOneSec;
            }
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.CompareTag("Player_Hide"))
        {
            IsOn[collision.gameObject] = true;
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.CompareTag("Player_Hide"))
        {
            IsOn[collision.gameObject] = false;
        }
    }
}
