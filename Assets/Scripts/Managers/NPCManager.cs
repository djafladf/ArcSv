using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NPCManager : MonoBehaviour
{
    // Name, Sub, Explain, Cost, Success, Fail
    [SerializeField] List<TMP_Text> Texts;
    [SerializeField] List<Sprite> Heads_Une;
    [SerializeField] List<Sprite> Heads_Swa;
    [SerializeField] Image Head;
}
