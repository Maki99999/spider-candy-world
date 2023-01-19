using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomVideoStart : MonoBehaviour
{
    public UnityEngine.Video.VideoPlayer video;

    private void Start()
    {
        video.frame = Random.Range(0, (int)video.frameCount);
    }
}
