using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpController : MonoBehaviour
{
    public static PickUpController Instance { get; private set; }

    private bool inCloseUp = false;
    private bool movingToCloseUp = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (inCloseUp)
        {
            var axisHorizontal = Input.GetAxisRaw("Horizontal");
            var axisVertical = Input.GetAxisRaw("Vertical");


        }
    }

    public void PickUpCloseUp()
    {
        PlayerController.instance.SetFrozen(true);
        inCloseUp = true;
        movingToCloseUp = true;
        StartCoroutine(MoveToCloseUp());
    }

    private IEnumerator MoveToCloseUp()
    {
        yield return null;
    }
}
