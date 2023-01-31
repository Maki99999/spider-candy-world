using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealthController : MonoBehaviour, Damageable
{
    public CanvasGroup hitIndicator;
    public AudioSource hitSound;

    public void Damage(int damageValue)
    {
        StopAllCoroutines();
        StartCoroutine(HitAnim());
    }

    private IEnumerator HitAnim()
    {
        hitSound.Play();
        var rate = 1f / 0.5f;
        for (float f = 0f; f <= 1f; f += rate * Time.deltaTime)
        {
            hitIndicator.alpha = Mathf.Lerp(1, 0, f);
            yield return null;
        }
    }
}
