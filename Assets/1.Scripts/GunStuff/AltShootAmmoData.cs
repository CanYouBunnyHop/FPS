using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AltAmmoData", menuName = "Weapons/AltAmmoData")]
public class AltShootAmmoData : ScriptableObject
{
    [Tooltip("Rounds per minute")] public float fireRate;
    public int MagSize, CurrentAmmo;
}
