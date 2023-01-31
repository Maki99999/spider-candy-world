using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speedMutiplier = 1;
    public float lifetime = 10;
    public int damage = 1;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += transform.forward * Time.deltaTime * speedMutiplier;
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
