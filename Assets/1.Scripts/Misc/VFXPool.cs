using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXPool : ObjectPool<GameObject>
{
    public static VFXPool VFX_Pool {get; private set;}
    void Start()
    {
        InstantiateItems(this.transform);
        VFX_Pool = this;
    }

    public override void InstantiateItems(Transform p)
    {
        base.InstantiateItems(p);
    }

    public override object GetItem()
    {
        return base.GetItem();
    }
    public override IEnumerator ReturnItemToPool(GameObject _item)
    {
        return base.ReturnItemToPool(_item);
    }
}
