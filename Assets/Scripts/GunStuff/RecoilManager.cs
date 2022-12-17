using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecoilManager : MonoBehaviour
{
    public float    X;
    public float    Y;
    void Update()
    {
        transform.localRotation = Quaternion.Euler(X,Y,0);
    }

    public void Recoil()
    {
        
    }
}
