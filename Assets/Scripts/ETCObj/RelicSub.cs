using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RelicSub : MonoBehaviour
{
    protected EventTrigger ET;


    string Info;

    int specialType = 0;
    float specialCount = 0;

    public void Init(Sprite myIm,Sprite Im, string name,string info, string extra, int SpecialType = 0,float SpecialCount = 0)
    {
        GetComponent<Image>().sprite = myIm;
        transform.GetChild(0).GetComponent<Image>().sprite = Im;
        Info = $"<size=125%>{name}\n</size><color=#8D8681>{extra}</color>\n{info}";
        specialCount = SpecialCount; specialType = SpecialType; 
    }

    void AddEvent(EventTrigger eventTrigger, EventTriggerType Type, Action<PointerEventData> Event)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = Type;
        entry.callback.AddListener((data) => { Event((PointerEventData)data); });
        eventTrigger.triggers.Add(entry);
    }

    void ButtonInit(EventTrigger eventTrigger, Action<PointerEventData> OnPointer, Action<PointerEventData> OutPointer)
    {
        //data.pointerId : left -> -1, right -> -2, Wheel -> -3;
        if (OnPointer != null) AddEvent(eventTrigger, EventTriggerType.PointerEnter, OnPointer);

        if (OutPointer != null) AddEvent(eventTrigger, EventTriggerType.PointerExit, OutPointer);
    }


    protected virtual void Awake()
    {
        if (GetComponent<EventTrigger>() == null) gameObject.AddComponent<EventTrigger>();
        ET = GetComponent<EventTrigger>();
        ButtonInit(ET, OnPointer, OutPointer);
    }


    protected virtual void OnPointer(PointerEventData data) 
    {
        if (specialType == 0) GameManager.instance.FloatM.Init(Info, 35);
        else 
            GameManager.instance.FloatM.Init($"{Info}<size=75%>(ÇöÀç {100 * specialCount * GameManager.instance.UM.StackObj[specialType-1]}%)", 35);
    }

    protected virtual void OutPointer(PointerEventData data)
    {
        GameManager.instance.FloatM.gameObject.SetActive(false);
    }
}
