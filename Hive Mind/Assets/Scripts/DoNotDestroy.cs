using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoNotDestroy : MonoBehaviour
{
    [SerializeField] string tag;

    void Awake()
    {
        GameObject[] obj = GameObject.FindGameObjectsWithTag(tag);
        if (obj.Length > 1)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }
}