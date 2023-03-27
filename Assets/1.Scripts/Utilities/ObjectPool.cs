using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ObjectPool<T> : MonoBehaviour where T : Object
{
    public int numberOfInstances;
    public T item;
    public float decayTime;
    [HideInInspector]public Queue<T> items = new Queue<T>();
    private Transform parent;

    void Awake()
    {
        parent = this.transform;
    }
    public virtual void InstantiateItems(Transform parent)
    {
        for(int x = 0; x < numberOfInstances; x ++)
        {
            var instance = Instantiate(item, transform.position, transform.rotation);
            EnqueueGameObject(instance);
        }
    }
    ///<summary> Automatically queue return after delay </summary>///
    public virtual object GetItem()
    {
        var itemQ = items.Dequeue();
        if(itemQ is null) //if no object is left in the pool, instantiate more to make up for the numbers
        {
            var instance = Instantiate(item, transform.position, transform.rotation);
            itemQ = instance;
        }

        var oitem = itemQ as GameObject;
        oitem.SetActive(true);

        //var itemQ  when itemQ is GameObject;
        StartCoroutine(ReturnItemToPool(itemQ));
        return itemQ; //return the queue
    }
    public virtual IEnumerator ReturnItemToPool(T _item)
    {
        yield return new WaitForSeconds(decayTime);
        EnqueueGameObject(_item);
    }
    protected void EnqueueGameObject(T _item)
    {
        items.Enqueue(_item);

        var oitem = _item as GameObject;
        oitem.SetActive(false);
        oitem.transform.SetParent(parent);
    }
}
