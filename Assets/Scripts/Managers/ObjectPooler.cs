using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class ObjectPoolItem
{
    public string tag;
    public GameObject objectToPool;
    public int amountToPool;
    public bool shouldExpand;
}

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance;
    [SerializeField] private List<ObjectPoolItem> itemsToPool;
    private Dictionary<string, List<GameObject>> pool;
    private void OnEnable()
    {
        LevelLoader.OnLevelChanged += ResetPool;
    }

    private void OnDestroy()
    {
        LevelLoader.OnLevelChanged -= ResetPool;
    }
    void Awake()
    {
        Instance = this;
        pool = new Dictionary<string, List<GameObject>>();
        foreach (ObjectPoolItem item in itemsToPool)
        {
            pool.Add(item.tag, new List<GameObject>());
            for (int i = 0; i < item.amountToPool; i++)
            {
                GameObject obj = (GameObject)Instantiate(item.objectToPool);
                obj.SetActive(false);
                pool[item.tag].Add(obj);
            }
        }
    }

    private void ResetPool(int l)
    {
        if (pool == null) return;
        foreach (List<GameObject> items in pool.Values)
        {
            foreach (GameObject item in items)
            {
                item.SetActive(false);
            }
        }
    }

    public GameObject GetPooledObject(string tag)
    {
        GameObject candidate = pool[tag].FirstOrDefault((go) => !go.activeInHierarchy);
        if (candidate != null)
        {
            return candidate;
        }
        foreach (ObjectPoolItem item in itemsToPool)
        {
            if (item.tag == tag)
            {
                if (item.shouldExpand)
                {
                    GameObject obj = (GameObject)Instantiate(item.objectToPool);
                    obj.SetActive(false);
                    pool[item.tag].Add(obj);
                    return obj;
                }
            }
        }
        return null;
    }
}
