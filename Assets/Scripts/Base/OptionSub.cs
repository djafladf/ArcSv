using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
public class OptionSub : MonoBehaviour
{
    [SerializeField] GameObject Child;
    [SerializeField] UnityEvent<BaseEventData> eventData;
    [SerializeField] Color OnColor,OutColor;
    Image im;

    void AddEvent(EventTrigger eventTrigger, EventTriggerType Type, Action<PointerEventData> Event)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = Type;
        entry.callback.AddListener((data) => { Event((PointerEventData)data); });
        eventTrigger.triggers.Add(entry);
    }

    private void Awake()
    {
        if (!TryGetComponent<EventTrigger>(out var ET)) { gameObject.AddComponent<EventTrigger>(); ET = GetComponent<EventTrigger>(); }
        AddEvent(ET, EventTriggerType.PointerEnter, OnPointer); AddEvent(ET, EventTriggerType.PointerExit, OutPointer);
        if (eventData != null) AddEvent(ET, EventTriggerType.PointerClick, OnClick);
        im = GetComponent<Image>();
    }

    void OnPointer(PointerEventData data)
    {
        if (Child != null) Child.gameObject.SetActive(true);
        im.color = OnColor;
    }

    void OutPointer(PointerEventData data)
    {
        if (Child != null) Child.gameObject.SetActive(false);
        im.color = OutColor;
    }

    void OnClick(PointerEventData data)
    {
        if (eventData != null) eventData.Invoke(data);
    }

    private void OnDisable()
    {
        OutPointer(null);
    }
}
