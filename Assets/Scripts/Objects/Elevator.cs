using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elevator : MonoBehaviour
{
    public bool isBroken = false;
    public Animator anim;

    private bool busy = false;

    public void PressedButton()
    {
        if (!busy)
            StartCoroutine(CallElevator());
    }

    IEnumerator CallElevator()
    {
        busy = true;
        yield return new WaitForSeconds(2f);
        anim.SetBool("Open", true);
        yield return new WaitForSeconds(10f);
        anim.SetBool("Open", false);

    }
}
