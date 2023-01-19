using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepController : MonoBehaviour
{
    [SerializeField] private Mode mode = Mode.MANUAL;
    [Space(10)]
    public PlayerController playerController;
    public CharacterController characterController;
    public GameObject footstepSound;
    public AudioClip[] audioClips;
    public float audioPitchVariation = 0.15f;

    [Space(10)]
    public float distanceBtwnFootstepsSqrt = 1;
    private float currDistanceSqrt = 0;
    private Vector3 lastPosition = Vector3.zero;

    [Space(10)]
    public float timeBtwnFootsteps = 1f;
    private float lastTimeStopped = 0f;

    [Space(10)]
    private float lastGroundedTime = 0f;
    private float lastAirTime = 0f;
    private float lastSoundTime = 0f;

    private List<AudioSource> footstepAudioSources = new List<AudioSource>();

    private void Awake()
    {
        lastPosition = characterController.transform.position;
        footstepAudioSources.Add(footstepSound.GetComponent<AudioSource>());
    }

    void Update()
    {
        if (characterController.isGrounded)
        {
            if (lastGroundedTime + 0.2f < Time.time)
                PlayFootstepSound();

            lastGroundedTime = Time.time;
        }
        else
            lastAirTime = Time.time;

        if (mode != Mode.MANUAL)
        {
            float distanceMovedSinceLastFrame = (lastPosition - characterController.transform.position).sqrMagnitude;
            if (characterController.isGrounded)
            {
                currDistanceSqrt += distanceMovedSinceLastFrame;
                if (currDistanceSqrt >= distanceBtwnFootstepsSqrt)
                {
                    currDistanceSqrt = currDistanceSqrt % distanceBtwnFootstepsSqrt;
                    if (mode == Mode.DISTANCE)
                        PlayFootstepSound();
                }
            }

            if (mode == Mode.TIME)
            {
                if (characterController.isGrounded && Time.time > lastSoundTime +
                        (timeBtwnFootsteps / playerController.speedCurrent) && lastTimeStopped +
                        (timeBtwnFootsteps / playerController.speedCurrent) < Time.time)
                    PlayFootstepSound();
            }

            if (distanceMovedSinceLastFrame <= 0.00000000001f)
                lastTimeStopped = Time.time;

            lastPosition = characterController.transform.position;
        }

    }

    public void PlayFootstepSoundFromAnimation()
    {
        if (mode == Mode.MANUAL)
            PlayFootstepSound();
    }

    private void PlayFootstepSound()
    {
        if (!isActiveAndEnabled || Time.time - lastSoundTime < 0.1f || !characterController.isGrounded)
            return;
        lastSoundTime = Time.time;

        AudioSource idleAudioSource = null;
        foreach (AudioSource audioSource in footstepAudioSources)
        {
            if (!audioSource.isPlaying)
            {
                idleAudioSource = audioSource;
                break;
            }
        }

        if (idleAudioSource == null)
        {
            GameObject newAudioSource = Instantiate(footstepSound, footstepSound.transform.position, footstepSound.transform.rotation, footstepSound.transform.parent);
            idleAudioSource = newAudioSource.GetComponent<AudioSource>();
            footstepAudioSources.Add(idleAudioSource);
        }

        idleAudioSource.clip = audioClips[Random.Range(0, audioClips.Length)];
        idleAudioSource.pitch = 1 + Random.Range(-audioPitchVariation, audioPitchVariation);
        idleAudioSource.Play();
    }

    private enum Mode
    {
        MANUAL,
        DISTANCE,
        TIME
    }
}
