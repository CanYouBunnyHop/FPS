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
    public bool canCancelReloadWithFire;
    public bool canCancelReloadWithAltFire;
    //[Header("Settings")]
    //public LayerMask enemyMask;
    [Header("Recoil")]
    public AnimationCurve recoilX;
    public AnimationCurve recoilY;
    public AnimationCurve recoilZ;

    [Header("Buffer")]
    public float fireBuffer = 0.2f;
    [Header("Select fire")]
    public FireMode defaultFireMode;
    public FireMode altFireMode;
    public bool allowDoubleFire;
    public enum FireMode
    {
       SemiAuto,
       FullAuto,
       BurstFire,
       Charge,
    }
}
