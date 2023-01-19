using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaController : MonoBehaviour
{
    [SerializeField] private AudioClip[] randomAmbientClips;
    [SerializeField] private Vector2 secondsBtwnAmbientClips;
    [SerializeField, ColorUsage(true, true)] private Color ambientColor = new Color(1.31950796f, 1.31950796f, 1.31950796f, 1);
    [SerializeField] private AudioClip[] musicClips;
    [SerializeField] private float speedMultiplier = 1f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            UpdateArea();
        }
    }

    private void UpdateArea()
    {
        MusicManager.instance.ChangeMusic(musicClips);
        AmbientManager.instance.ChangeAmbientClips(randomAmbientClips, secondsBtwnAmbientClips);
        AmbientManager.instance.ChangeAmbientColor(ambientColor);
        Default.PlayerController.instance.speedMultiplier = speedMultiplier;
    }
}
