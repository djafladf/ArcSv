using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class OptionBase : MonoBehaviour
{
    void AddEvent(EventTrigger eventTrigger, EventTriggerType Type, Action<PointerEventData> Event)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = Type;
        entry.callback.AddListener((data) => { Event((PointerEventData)data); });
        eventTrigger.triggers.Add(entry);
    }

    RectTransform rect;

    private void Awake()
    {
        if (!TryGetComponent<EventTrigger>(out var ET)) { gameObject.AddComponent<EventTrigger>(); ET = GetComponent<EventTrigger>(); }
        AddEvent(ET, EventTriggerType.PointerExit, Close);
        rect = GetComponent<RectTransform>();
    }

    void Close(PointerEventData data)
    {
        gameObject.SetActive(false);
        data.Use();
    }

    private void OnEnable()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.GetComponent<RectTransform>(), Input.mousePosition, null, out var CurInput);
        rect.anchoredPosition = new Vector2(CurInput.x + 1280, CurInput.y + 720);
    }
}
