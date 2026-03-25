using System;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewPlayer", menuName = "ScriptableObjects/Player")]
public class Player : ScriptableObject
{
    [NonSerialized] public bool IsPlayer = false;

    [NonSerialized] public Transform Self;
    [NonSerialized] public Vector2 Dir;
    [SerializeField] public string name_k;

    // Status(SerializeAble)
    [SerializeField] public float speed;
    [SerializeField] public int InitHP;
    [SerializeField] public int InitSP;
    [SerializeField] public int InitDefense;
    [SerializeField] public int Cost;
    [SerializeField] public int ReBatchTime;


    // Status(NonSerializeAble)
    [NonSerialized] public PlayerSetting Scripts;
    [NonSerialized] public int MaxHP;
    [NonSerialized] public int CurHP;
    [NonSerialized] public int MaxSp;
    [NonSerialized] public int CurSP;
    [NonSerialized] public int CurReinforce;

    [NonSerialized] public List<SpriteRenderer> SubEffects = new List<SpriteRenderer>();

    [NonSerialized] public Vector3 WeaponPos;
    [NonSerialized] public Vector3 FlipWeaponPos;
    [NonSerialized] public SpriteRenderer WeaponSprite;

    [NonSerialized] public Rigidbody2D rigid;
    [NonSerialized] public SpriteRenderer sprite;
    [NonSerialized] public Animator anim;

    [NonSerialized] public int WeaponLevel;
    [NonSerialized] public int Id;

    // Status Ratio
    [NonSerialized] public float AttackSpeed = 1f;
    [NonSerialized] public float MaxAttackSpeed = 1f;

    [NonSerialized] public float HealRatio = 1f;
    [NonSerialized] public float DefenseRatio = 0f;
    [NonSerialized] public float HPRatio = 0f;
    [NonSerialized] public float AttackRatio = 0f;
    [NonSerialized] public float SpeedRatio = 0f;

    // About Option
    [NonSerialized] public bool IsFollow = false;
    [NonSerialized] public bool AllowFollow = true;
    [NonSerialized] public bool AllowMove = true;
    [NonSerialized] public bool ChangeOccur = false;
    [NonSerialized] public OperatorBatchTool MyBatch;

    [NonSerialized] public bool Unbeat = false;

    // Buff
    // 공, 방, 속, 공속
    [NonSerialized]public float[] ReinforceAmount = { 0, 0, 0, 0 };
    [NonSerialized]public float[] ReinForceLast = { 0, 0, 0, 0 };

    // Debuff
    // 4 : IceRatio
    [NonSerialized] public float[] DeBuffAmount = { 0, 0, 0, 0 ,0};
    [NonSerialized] public float[] DeBuffLast = { 0, 0, 0, 0 ,0};
}

