using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Default
{
    public class UseController : MonoBehaviour
    {
        public float range = 2.5f;
        public LayerMask mask;

        private bool lastPress = false;

        void LateUpdate()
        {
            if (!isActiveAndEnabled)
                return;

            //Get Input
            bool useKey = InputManager.PressingUse();

            //Get useable GameObject and maybe use it
            RaycastHit hit;
            if (useKey && !lastPress && Physics.Raycast(transform.position, transform.forward, out hit, range, mask))
            {
                GameObject hitObject = hit.collider.gameObject;
                if (hitObject.CompareTag("Useable"))
                {
                    Useable useable = hitObject.GetComponent<Useable>();

                    if (useable == null)
                    {
                        Debug.LogError("Can't find 'Useable' script.");
                    }
                    else
                    {
                        if (useKey && !lastPress)
                            useable.Use();
                    }
                }
            }

            lastPress = useKey;
        }
    }

    public interface Useable
    {
        public abstract void Use();
    }
}