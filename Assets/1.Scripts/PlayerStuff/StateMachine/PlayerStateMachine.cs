using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    void Update()
    {
        
    }
    void FixedUpdate()
    {

    }
    IEnumerator LateFixedUpdate()
    {
        while(true)
        {
            yield return new WaitForFixedUpdate();
        }
    }
}
