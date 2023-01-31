using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VentEntrance : MonoBehaviour, Useable
{
    public AudioSource audioSource;
    public AudioClip rattleSound;
    public AudioClip openingSound;
    public Transform coverClosed;
    public Transform coverOpened;

    private Vector3 pos1;

    private void Start()
    {
        pos1 = coverClosed.localPosition;
    }

    public void Use()
    {
        StopAllCoroutines();

        if (InventoryController.instance.HasItem("Screwdriver"))
        {
            coverClosed.gameObject.SetActive(false);
            coverOpened.gameObject.SetActive(true);
            audioSource.clip = openingSound;
            audioSource.Play();
            GetComponent<Collider>().enabled = false;
        }
        else
        {
            audioSource.clip = rattleSound;
            audioSource.Play();
            StartCoroutine(Rattle());
        }
    }

    private IEnumerator Rattle()
    {
        var pos2 = pos1 - coverClosed.forward * 0.01f;

        coverClosed.localPosition = pos2;
        yield return new WaitForSeconds(0.15f);
        coverClosed.localPosition = pos1;
        yield return new WaitForSeconds(0.15f);
        coverClosed.localPosition = pos2;
        yield return new WaitForSeconds(0.15f);
        coverClosed.localPosition = pos1;
    }
}
