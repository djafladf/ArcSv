using UnityEngine;
using UnityEngine.EventSystems;

public class HandleObj : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] ParticleSystem PS;

    // ���콺�� Handle ���� �ö��� ��
    public void OnPointerEnter(PointerEventData eventData)
    {
        PS.Play();
    }

    // ���콺�� Handle���� ����� ��
    public void OnPointerExit(PointerEventData eventData)
    {
        PS.Stop();
    }
}
