using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetAlphaHitTestMinimumThreshold : MonoBehaviour
{
    void Start()
    {
        GetComponent<Image>().alphaHitTestMinimumThreshold = 0.5f;
    }
}
