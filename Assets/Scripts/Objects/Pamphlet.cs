using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pamphlet : MonoBehaviour, Useable
{
    public Renderer staticPage;
    public Renderer movingPageFront;
    public Renderer movingPageBack;

    public Material[] pages;

    private Collider coll;
    private bool inAnimation;
    private int currentPage = 0;

    private void Start()
    {
        coll = GetComponent<Collider>();

        movingPageFront.gameObject.SetActive(false);
        if (pages.Length > 0)
        {
            staticPage.materials = new Material[] { pages[currentPage], pages[currentPage] };
        }
        else
            coll.enabled = false;
    }

    public void Use()
    {
        if (!inAnimation)
        {
            StartCoroutine(TurnPage());
        }
    }

    private IEnumerator TurnPage()
    {
        coll.enabled = false;
        inAnimation = true;

        int nextPage = (currentPage + 1) % pages.Length;
        staticPage.materials = new Material[] { pages[currentPage], pages[nextPage] };;
        movingPageFront.material = pages[currentPage];
        movingPageBack.material = pages[nextPage];
        currentPage = nextPage;
        movingPageFront.gameObject.SetActive(true);
        var transf = movingPageFront.transform;

        float rate = 1 / 1.5f;
        for (float f = 0f; f <= 1f; f += rate * Time.deltaTime)
        {
            transf.localEulerAngles = new Vector3(Mathf.SmoothStep(-1, -179, f), 0, 90);
            yield return null;
        }

        staticPage.materials = new Material[] { pages[currentPage], pages[currentPage] };
        movingPageFront.gameObject.SetActive(false);
        coll.enabled = true;
        inAnimation = false;
    }
}
