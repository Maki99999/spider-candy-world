using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyGuard : MonoBehaviour
{
    public GameObject idle;
    public GameObject transition;
    public GameObject block;

    [Space(10)]
    public SpriteRenderer spriteRenderer;
    public Sprite[] transitionSprites;

    [Space(10)]
    public Vector2 distanceSqrMinMax = new Vector2(1, 12);

    void Update()
    {
        Vector3 playerPos = PlayerController.instance.transform.position;
        float distanceSqr = (playerPos - transform.position).sqrMagnitude;
        if (playerPos.y > 2f &&
            distanceSqr < distanceSqrMinMax.y)
        {
            if (distanceSqr < distanceSqrMinMax.x)
            {
                idle.SetActive(false);
                block.SetActive(true);
                transition.SetActive(false);
            }
            else
            {
                idle.SetActive(false);
                block.SetActive(false);
                transition.SetActive(true);

                float percent = 1f - Mathf.Clamp01((distanceSqr - distanceSqrMinMax.x) / (distanceSqrMinMax.y - distanceSqrMinMax.x));
                spriteRenderer.sprite = transitionSprites[Mathf.RoundToInt(percent * (transitionSprites.Length - 1))];
            }
        }
        else
        {
            idle.SetActive(true);
            block.SetActive(false);
            transition.SetActive(false);
        }
    }
}
