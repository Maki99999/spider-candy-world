using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour
{
    public Transform target;
    public Rigidbody rb;

    public float moveSpeed = 10f;
    public float rotateSpeed = 10f;
    public float lifetime = 20f;
    public int damage = 1;

    private bool dropDown = false;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        if (target != null)
        {
            Vector3 direction = (target.position - rb.position);
            if (direction.sqrMagnitude < 4f)
            {
                rb.useGravity = true;
                dropDown = true;
            }

            if (!dropDown)
            {
                direction.Normalize();
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, lookRotation, Time.fixedDeltaTime * rotateSpeed));
            }

            rb.MovePosition(rb.position + transform.forward * moveSpeed * Time.fixedDeltaTime);
        }
    }

    private void OnCollisionStay(Collision other)
    {
        if (other.gameObject.CompareTag("Damageable"))
        {
            Damageable damageable = other.gameObject.GetComponent<Damageable>();
            if (damageable == null)
            {
                damageable = other.gameObject.GetComponentInParent<Damageable>();
                if (damageable == null)
                {
                    damageable = other.gameObject.GetComponentInChildren<Damageable>();
                    if (damageable == null)
                        Debug.LogError("Is tagged as 'Damageable' but cannot find Damageable script! " + "(" + other.gameObject.name + ")");
                }
            }
            damageable.Damage(damage);
        }

        Destroy(gameObject);
    }
}
