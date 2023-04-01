using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class decalRandomRotation : MonoBehaviour
{
    void OnEnable()
    {
        float randomZ = Random.Range(-180, 180);
        transform.localRotation = Quaternion.Euler(90, 0, randomZ);//new Quaternion(90, 0, randomZ, 0).eulerAngles;
    }
}
