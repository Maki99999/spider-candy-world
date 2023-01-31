using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUp : MonoBehaviour, Useable
{
    public string pickupName;
    public Transform pickupTransform;
    private bool pickedUp = false;

    public void Use()
    {
        if (pickedUp)
            return;

        pickedUp = true;
        InventoryController.instance.AddItem(pickupTransform, pickupName);
    }
}
