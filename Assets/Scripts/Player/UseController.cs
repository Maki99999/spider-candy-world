using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UseController : MonoBehaviour
{
    public float range = 2.5f;
    public LayerMask mask;

    public GameObject uiIndicator;

    private bool lastPress = false;

    private void LateUpdate()
    {
        if (!isActiveAndEnabled)
            return;

        //Get Input
        bool useKey = InputManager.PressingUse();

        //Get useable GameObject and maybe use it
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, range, mask))
        {
            GameObject hitObject = hit.collider.gameObject;
            if (hitObject.CompareTag("Useable"))
            {
                Useable[] useables = hitObject.GetComponents<Useable>();

                if (useables.Length == 0)
                {
                    Debug.LogError("Can't find 'Useable' script.");
                }
                else
                {
                    if (!uiIndicator.activeSelf)
                        uiIndicator.SetActive(true);

                    if (useKey && !lastPress)
                    {
                        foreach (var useable in useables)
                            useable.Use();
                    }
                }
            }
            else
            {
                uiIndicator.SetActive(false);
            }
        }
        else
        {
            uiIndicator.SetActive(false);
        }

        lastPress = useKey;
    }

    private void OnDisable()
    {
        if (uiIndicator != null)
            uiIndicator.SetActive(false);
    }
}

public interface Useable
{
    public abstract void Use();
}
