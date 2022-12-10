using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Guns", menuName = "Weapons/Guns")]
public class GunData : ScriptableObject
{
    public string gunName;
    [Header("Stats")]
    public float damage;
    public float fireRate; //in RPM
    public float range;

    [Header("Ammo")]
    public int magSize;
    public int currentAmmo;

    [Header("Reload")]
    public float reloadSpeed;
    public bool isReloading;
    public bool canCancelReload;
    //[Header("Settings")]
    //public LayerMask enemyMask;
    [Header("Recoil")]
    public AnimationCurve recoilX;
    public AnimationCurve recoilY;
    public AnimationCurve recoilZ;

    [Header("FireMode")]
    public FireMode fireMode;
    [Header("Firemode specific stats")]
    public float burstsCount;
    public enum FireMode
    {
        SemiAuto,
        FullAuto,
        Bursts,
        Charged,
    }
}
