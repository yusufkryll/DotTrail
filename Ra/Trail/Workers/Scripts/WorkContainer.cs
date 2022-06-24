using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorkContainer : MonoBehaviour
{
    public List<Worker> workers;
    public List<KVP<string, GameObject>> references; 

    private void Start()
    {
        foreach (var worker in workers)
        {
            worker.container = this;
            worker.Start();
        }
    }

    private void Update()
    {
        foreach (var worker in workers)
        {
            if(!worker.enabled) continue;
            worker.Update();
        }
    }
    
    public T GetWorker<T>() where T : Worker
    {
        var firstOrDefault = workers.FirstOrDefault(x => x.GetType() == typeof(T));
        return firstOrDefault as T;
    }

    public GameObject Which(string referenceName)
    {
        var firstOrDefault = references.FirstOrDefault(x => x.Key.Equals(referenceName));
        return firstOrDefault?.Value;
    }
}

[Serializable]
public class KVP<K, V>
{
    public K Key;
    public V Value;
}