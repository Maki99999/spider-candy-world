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

    void Update()
    {
        if (Input.GetAxis("Primary") > 0 && Time.time > nextShootTime)
        {
            anim.SetTrigger("Shoot");
            sound.Play();
            nextShootTime = Time.time + cooldown;

            var inst = Instantiate(projectile, projectileSpawn.position, projectileSpawn.rotation, ProjectileCollector.instance.transform);
            Physics.IgnoreCollision(inst.GetComponent<Collider>(), Default.PlayerController.instance.GetComponent<Collider>());
        }
    }
}
