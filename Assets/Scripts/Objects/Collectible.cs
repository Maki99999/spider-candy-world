using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : CollectableBehaviour
{
    public string collectibleName = "gem";

    public GameObject collectedObject;

    public void Collect()
    {
        CollectiblesManager.instance.AddCollectible(collectibleName, 1);

        Instantiate(collectedObject, transform.position, Quaternion.identity, transform.parent);
        gameObject.SetActive(false);
    }
}
