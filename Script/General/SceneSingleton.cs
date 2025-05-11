using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ASingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject prefab = Resources.Load(typeof(T).Name) as GameObject;
                if (prefab == null)
                {
                    Debug.LogError($"Prefab with name {typeof(T).Name} not found in Resources.");
                    return null;
                }
                GameObject singleton = Instantiate(prefab);
                _instance = singleton.GetComponent<T>();
                if (_instance == null)
                {
                    Debug.LogError($"Component of type {typeof(T).Name} not found in prefab.");
                }
            }
            
            return _instance;
        }
    }
    
    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
        }
    }
}

public class SceneSingleton<T> : ASingleton<T> where T : MonoBehaviour
{
    protected override void Awake()
    {
        base.Awake();
    }
}