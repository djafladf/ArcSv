using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextChanger : MonoBehaviour
{
    [SerializeField] bool ReverseType;
    [SerializeField] List<string> Texts;
    [SerializeField] float ChangeTime;

    WaitForSeconds wfs;
    TMP_Text text;
    private void Awake()
    {
        wfs = new WaitForSeconds(ChangeTime);
        text = GetComponent<TMP_Text>(); text.text = Texts[0];
        StartCoroutine(Changer());
    }

    int d = 1;
    int ind = 0;

    IEnumerator Changer()
    {
        while (true)
        {
            yield return wfs;
            ind += d; text.text = Texts[ind];
            if (ReverseType && (ind == 0 | ind == Texts.Count - 1)) d *= -1;
            if (!ReverseType && ind == Texts.Count - 1) ind = -1;
        }
    }
}
