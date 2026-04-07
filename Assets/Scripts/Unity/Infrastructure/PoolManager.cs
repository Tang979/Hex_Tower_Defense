using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    private Dictionary<GameObject, Queue<GameObject>> poolDictionary = new Dictionary<GameObject, Queue<GameObject>>();
    private Dictionary<GameObject, Transform> parentDictionary = new Dictionary<GameObject, Transform>();

    public GameObject GetObject(GameObject prefab)
    {
        if (!poolDictionary.ContainsKey(prefab))
        {
            poolDictionary[prefab] = new Queue<GameObject>();
            GameObject poolParent = new GameObject(prefab.name + "_Pool");
            poolParent.transform.SetParent(this.transform);
            parentDictionary[prefab] = poolParent.transform;
        }

        Queue<GameObject> objectPool = poolDictionary[prefab];

        if (objectPool.Count > 0)
        {
            GameObject obj = objectPool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        else
        {
            GameObject newObj = Instantiate(prefab, parentDictionary[prefab]);
            newObj.SetActive(false);
            return newObj;
        }
    }

    public void ReturnPool(GameObject prefab, GameObject obj)
    {
        obj.SetActive(false);

        // Đề phòng lỗi: Kiểm tra xem kho có tủ này chưa
        if (!poolDictionary.ContainsKey(prefab))
        {
            poolDictionary[prefab] = new Queue<GameObject>();
        }

        if (parentDictionary.ContainsKey(prefab))
        {
            obj.transform.SetParent(parentDictionary[prefab]);
        }

        // Cất lại vào kho
        poolDictionary[prefab].Enqueue(obj);
    }

    public void Prewarm(GameObject prefab, int count)
    {
        if (prefab == null) return;
        
        if (!poolDictionary.ContainsKey(prefab))
        {
            poolDictionary[prefab] = new Queue<GameObject>();
            GameObject poolParent = new GameObject(prefab.name + "_Pool");
            poolParent.transform.SetParent(this.transform);
            parentDictionary[prefab] = poolParent.transform;
        }

        int currentCount = poolDictionary[prefab].Count;
        for (int i = 0; i < count - currentCount; i++)
        {
            GameObject newObj = Instantiate(prefab, parentDictionary[prefab]);
            newObj.SetActive(false);
            poolDictionary[prefab].Enqueue(newObj);
        }
    }
}