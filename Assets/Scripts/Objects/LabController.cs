using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LabController : MonoBehaviour
{
    public Animator anim;
    public GameObject prison;
    public Animator monsterAnim;
    public GameObject areaController;

    [Space(10)]
    public AudioSource[] audioSources;
    public AudioClip[] audioClips;
    public AudioSource audioSourceSpecial;
    public AudioSource audioSourceMonster;
    public AudioClip[] monsterStepSound;

    [Space(10)]
    public GameObject tutorialGuy;
    public Transform tutorialGuyLookAt;
    public GameObject tutorialGuyExplosion;
    public GameObject handGun;

    private bool triggered = false;
    private bool waiting = false;
    private int currentAudioSource;

    private void OnTriggerEnter(Collider other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            triggered = true;
            StartCoroutine(Cutscene());
        }
    }

    public void PlaySound(AudioClip clip)
    {
        AudioSource audio = audioSources[currentAudioSource];
        currentAudioSource = (currentAudioSource + 1) % audioSources.Length;

        Vector2 randomPos = Random.onUnitSphere;
        Vector3 randomPos3 = new Vector3(Mathf.Abs(randomPos.x) * -1, 0, randomPos.y);
        audio.transform.position = Default.PlayerController.instance.transform.position + randomPos3;

        audio.clip = clip;
        audio.pitch = Random.Range(0.7f, 1f);
        audio.Play();
    }

    private IEnumerator Cutscene()
    {
        prison.SetActive(true);
        anim.SetTrigger("Alert");
        yield return new WaitForSeconds(3f);

        for (int i = 0; i < 1; i++)//TODO: 30
        {
            yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));

            if (i == 12)
                audioSourceSpecial.Play();

            PlaySound(audioClips[Random.Range(0, audioClips.Length)]);
        }

        yield return new WaitForSeconds(5f);
        audioSourceMonster.gameObject.SetActive(true);
        audioSourceMonster.Play();

        yield return new WaitForSeconds(3f);
        monsterAnim.SetBool("Eat", true);
        Default.PlayerController.instance.focusedObject = monsterAnim.transform;
        anim.SetTrigger("Off");
        areaController.SetActive(true);

        yield return new WaitForSeconds(3f);
        monsterAnim.SetBool("Eat", false);
        Default.PlayerController.instance.focusedObject = null;
        prison.SetActive(false);

        yield return ChasePlayer();
        yield return Tutorial();
        yield return ChasePlayer();
    }

    private IEnumerator ChasePlayer()
    {
        int poseCount = 16;
        int lastPose = 0;

        Transform monster = audioSourceMonster.transform;

        waiting = true;
        while (waiting)
        {
            var lookPos = Default.PlayerController.instance.transform.position - monster.position;
            lookPos.y = 0;
            var rotation = Quaternion.LookRotation(lookPos);
            monster.rotation = rotation;
            monster.position += monster.forward * 0.8f;

            int newPose = Random.Range(0, poseCount);
            if (lastPose == newPose)
                newPose = (newPose + 1) % poseCount;
            monsterAnim.SetInteger("Pose", newPose + 1);
            lastPose = newPose;

            audioSourceMonster.PlayOneShot(monsterStepSound[Random.Range(0, monsterStepSound.Length)]);
            yield return new WaitForSeconds(.1f);
        }
    }

    private IEnumerator Tutorial()
    {
        Default.PlayerController.instance.SetFrozen(true);
        yield return Default.PlayerController.instance.LookAt(monsterAnim.transform.position, 1);

        yield return new WaitForSeconds(5f);

        tutorialGuyExplosion.SetActive(true);
        yield return new WaitForSeconds(.3f);
        tutorialGuy.SetActive(true);
        yield return Default.PlayerController.instance.LookAt(tutorialGuyLookAt.position, 1);

        yield return new WaitForSeconds(2f);//TODO: speech
        tutorialGuyExplosion.SetActive(false);
        handGun.SetActive(true);
        yield return new WaitForSeconds(2f);

        tutorialGuyExplosion.SetActive(true);
        yield return new WaitForSeconds(.3f);
        tutorialGuy.SetActive(false);

        Default.PlayerController.instance.SetFrozen(false);
        yield return new WaitForSeconds(1f);
        tutorialGuyExplosion.SetActive(false);
    }

    public void CaughtPlayer()
    {
        waiting = false;
        StartCoroutine(Tutorial()); //TODO: remove line
    }

}
