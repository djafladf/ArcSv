using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffManager : MonoBehaviour
{
    [SerializeField] Sprite Chill;
    [SerializeField] Sprite Freeze;
    [SerializeField] Sprite Slow;
    [SerializeField] Sprite DefenseDown;
    [SerializeField] GameObject DebuffObject;

    GameObject[] DeBuffs;
    SpriteRenderer[] Sprites;

    private void Awake()
    {
        GameManager.instance.BFM = this;
        GameManager.instance.StartLoading();
    }

    public void Init()
    {
        DeBuffs = new GameObject[200];
        Sprites = new SpriteRenderer[200];
        for(int i = 0; i < 200; i++)
        {
            DeBuffs[i] = Instantiate(DebuffObject, transform);
            Sprites[i] = DeBuffs[i].GetComponent<SpriteRenderer>();
        }
        GameManager.instance.StartLoading();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="type">
    /// 0 : ����, 1 : ����, 2 : ����, 3 : �� �ı�
    /// </param>
    /// <returns></returns>
    public GameObject RequestForDebuff(int type, float Size_X = 0, float Size_Y = 0)
    {
        for (int i = 0; i < 200; i++)
        {
            if (!DeBuffs[i].activeSelf) 
            {
                DeBuffs[i].transform.localScale = Vector3.one;
                Sprites[i].sortingOrder = 2;
                switch (type)
                {
                    case 0: Sprites[i].sprite = Chill; break;
                    case 1:
                        Sprites[i].sortingOrder = 5;
                        Sprites[i].sprite = Freeze;
                        DeBuffs[i].transform.localScale = new Vector3(Size_X / Freeze.bounds.size.x, Size_Y / Freeze.bounds.size.y, 1);
                        break;
                    case 2:
                        Sprites[i].sprite = Slow; break;
                    case 3:
                        Sprites[i].sprite = DefenseDown; break;
                }

                return DeBuffs[i];
            }
        }
        return null;
    }

}
