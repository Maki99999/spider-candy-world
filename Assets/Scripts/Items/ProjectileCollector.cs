using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileCollector : MonoBehaviour
{
    public static ProjectileCollector instance { get; private set; }

    private void Awake()
    {
        instance = this;
    }
}
