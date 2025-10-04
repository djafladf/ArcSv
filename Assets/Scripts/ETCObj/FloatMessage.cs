using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class FloatMessage : MonoBehaviour
{
    TMP_Text MainText;
    RectTransform InfRect;
    private void Awake()
    {
        InfRect = GetComponent<RectTransform>();
        MainText = transform.GetChild(0).GetComponent<TMP_Text>();
    }
    private void Start()
    {
        if (GameManager.instance == null) return;
        if (GameManager.instance.FloatM == null)
        {
            GameManager.instance.FloatM = this;
            gameObject.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
#if UNITY_ANDROID || UNITY_IOS
            Vector3 MousePos = Touchscreen.current.primaryTouch.position.ReadValue();
#endif
#if UNITY_STANDALONE
        Vector3 MousePos = Input.mousePosition;
#endif
        if (Screen.width- MousePos.x < InfRect.sizeDelta.x * ratio) MousePos.x = Screen.width - InfRect.sizeDelta.x * ratio;
        if (Screen.height - MousePos.y < InfRect.sizeDelta.y * ratio) MousePos.y = Screen.height - InfRect.sizeDelta.y * ratio;
        transform.position = MousePos;
    }

    float ratio = 1;
    public void Init(string Message,float font = 50)
    {
        MainText.text = Message; MainText.fontSize = font;
        ratio = Screen.width * 0.000390625f;
        gameObject.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(InfRect);
    }
    public void TimeShow(float time, string Message)
    {
        StopAllCoroutines();
        Init(Message);
        StartCoroutine(Cor(time));
    }

    IEnumerator Cor(float time)
    {
        yield return new WaitForSeconds(time);
        gameObject.SetActive(false);
    }
}
