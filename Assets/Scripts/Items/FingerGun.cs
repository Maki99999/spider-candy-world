using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FingerGun : MonoBehaviour
{
    public GameObject projectile;
    public Transform projectileSpawn;
    public float cooldown;
    public Animator anim;
    public AudioSource sound;

    private float nextShootTime = 0;

    private void Update()
    {
        if (!PlayerController.instance.IsFrozen() && Input.GetAxis("Primary") > 0 && Time.time > nextShootTime)
        {
            anim.SetTrigger("Shoot");
            sound.Play();
            nextShootTime = Time.time + cooldown;

            var inst = Instantiate(projectile, projectileSpawn.position, projectileSpawn.rotation, ProjectileCollector.instance.transform);
            Physics.IgnoreCollision(inst.GetComponent<Collider>(), PlayerController.instance.GetComponent<Collider>());
        }
    }

    private void OnEnable()
    {
        anim.SetBool("Hidden", false);
    }

    private void OnDisable()
    {
        anim.SetBool("Hidden", true);
    }
}
