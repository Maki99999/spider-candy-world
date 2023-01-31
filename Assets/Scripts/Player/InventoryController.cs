using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    public static InventoryController instance;

    public Transform itemHoldPosition;
    private List<string> items = new List<string>();

    private void Awake()
    {
        instance = this;
    }

    public void AddItem(Transform item, string itemName)
    {
        StartCoroutine(SmoothPickup(item, itemName));
    }

    private IEnumerator SmoothPickup(Transform item, string itemName, float seconds = 1f)
    {
        Vector3 oldPos = item.position;
        Quaternion oldRot = item.rotation;

        float rate = 1f / seconds;
        float fSmooth;
        for (float f = 0f; f <= 1f; f += rate * Time.deltaTime)
        {
            fSmooth = Mathf.SmoothStep(0f, 1f, f);

            item.position = Vector3.Lerp(oldPos, itemHoldPosition.position, fSmooth);
            item.rotation = Quaternion.Lerp(oldRot, itemHoldPosition.rotation, fSmooth);

            yield return null;
        }

        items.Add(itemName);
        item.gameObject.SetActive(false);
    }

    public bool HasItem(string itemName)
    {
        return items.Contains(itemName);
    }

    public void RemoveItem(string itemName)
    {
        items.Remove(itemName);
    }
}
