using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class BaseInfra : MonoBehaviour
{
    public int serial;

    [SerializeField] RectTransform BuildText;
    [SerializeField] TMP_Text PercentText;

    RectTransform rect;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }


    void AddEvent(EventTrigger eventTrigger, EventTriggerType Type, Action<PointerEventData> Event)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = Type;
        entry.callback.AddListener((data) => { Event((PointerEventData)data); });
        eventTrigger.triggers.Add(entry);
    }

}
