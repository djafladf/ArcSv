using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    // 0 : ��ġ, 1 : ��, 2 : ����, 3 : ��, 4 : �ڼ�
    int type;
    int amount;
    int target;
    public int poolInd;
    public bool IsMoving;
    string AreaTag = "GainArea";
    Rigidbody2D rigid;

    public void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    public void Init(int type, int amount, int Target = 0,int AreaCount = -1)
    {
        this.type = type; this.amount = amount; IsMoving = false; speed = 10; target = Target; if (AreaCount != -1) AreaTag = $"ExternalArea{AreaCount}";
    }

    float speed; 

    public void FixedUpdate()
    {
        if (IsMoving)
        {
            Vector2 Dir = (GameManager.instance.Players[target].Self.position - transform.position).normalized;
            rigid.MovePosition(rigid.position + Dir * speed * Time.fixedDeltaTime);
            speed *= 1.05f; if (speed > 30) speed = 30;
        }
        else if (GameManager.instance.IM.MagTime != 0 && type != -1) IsMoving = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (type != -1)
        {
            if (collision.CompareTag(AreaTag) && !IsMoving) IsMoving = true;
            else if (collision.CompareTag("Player"))
            {
                IsMoving = false;
                gameObject.SetActive(false);
                GameManager.instance.IM.RemoveItem(poolInd);
                switch (type)
                {
                    case 0: GameManager.instance.UM.ExpUp(amount); break;
                    case 4: GameManager.instance.IM.MagStart(); break;
                    default: GameManager.instance.UM.GoodsUp(type - 1, amount); break;
                }
            }
        }
        else
        {
            if (collision.CompareTag(AreaTag) && !IsMoving) IsMoving = true;
        }
    }

    private void OnEnable()
    {
        IsMoving = false;
    }
}
