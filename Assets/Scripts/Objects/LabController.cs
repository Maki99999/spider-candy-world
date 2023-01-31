using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LabController : MonoBehaviour
{
    public Animator anim;
    public GameObject prison;
    public Animator monsterAnim;
    public GameObject areaController;
    public GameObject monsterHitIndicator;
    public GameObject teleporter;

    [Space(10)]
    public AudioSource[] audioSources;
    public AudioClip[] audioClips;
    public AudioSource audioSourceSpecial;
    public AudioSource audioSourceMonster;
    public AudioClip[] monsterStepSound;
    public AudioClip monsterDeadSound;

    [Space(10)]
    public MeDialogue tutorialGuy;
    public Transform tutorialGuyLookAt;
    public GameObject tutorialGuyExplosion;
    public Animator handGun;
    public AudioClip[] tutorialVoices;
    public AudioClip tutorialVoiceShort;

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
        audio.transform.position = PlayerController.instance.transform.position + randomPos3;

        audio.clip = clip;
        audio.pitch = Random.Range(0.7f, 1f);
        audio.Play();
    }

    private IEnumerator Cutscene()
    {
        prison.SetActive(true);
        anim.SetTrigger("Alert");
        yield return new WaitForSeconds(3f);

        for (int i = 0; i < 25; i++)
        {
            yield return new WaitForSeconds(Random.Range(0.2f, 0.7f));

            if (i == 12)
                audioSourceSpecial.Play();

            PlaySound(audioClips[Random.Range(0, audioClips.Length)]);
        }

        yield return new WaitForSeconds(5f);
        audioSourceMonster.gameObject.SetActive(true);
        audioSourceMonster.Play();

        yield return new WaitForSeconds(3f);
        monsterAnim.SetBool("Eat", true);
        PlayerController.instance.focusedObject = monsterAnim.transform;
        anim.SetTrigger("Off");
        areaController.SetActive(true);

        yield return new WaitForSeconds(3f);
        monsterAnim.SetBool("Eat", false);
        PlayerController.instance.focusedObject = null;
        prison.SetActive(false);

        yield return ChasePlayer();
        yield return Tutorial();
        yield return ChasePlayer();
        while (isActiveAndEnabled)
        {
            yield return TutorialShort();
            yield return ChasePlayer();
        }
    }

    private IEnumerator ChasePlayer()
    {
        int poseCount = 16;
        int lastPose = 0;

        Transform monster = audioSourceMonster.transform;

        waiting = true;
        while (waiting && isActiveAndEnabled)
        {
            var lookPos = PlayerController.instance.transform.position - monster.position;
            lookPos.y = 0;
            var rotation = Quaternion.LookRotation(lookPos);
            monster.rotation = rotation;
            monster.position += monster.forward * 0.55f;

            int newPose = Random.Range(0, poseCount);
            if (lastPose == newPose)
                newPose = (newPose + 1) % poseCount;
            monsterAnim.SetInteger("Pose", newPose + 1);
            lastPose = newPose;

            audioSourceMonster.PlayOneShot(monsterStepSound[Random.Range(0, monsterStepSound.Length)]);
            yield return new WaitForSeconds(.1f);
        }
    }

    private IEnumerator TutorialShort()
    {
        PlayerController.instance.SetFrozen(true);
        yield return PlayerController.instance.LookAt(monsterAnim.transform.position, 1);

        tutorialGuyExplosion.SetActive(true);
        yield return new WaitForSeconds(.3f);
        tutorialGuy.transform.parent.gameObject.SetActive(true);
        yield return PlayerController.instance.LookAt(tutorialGuyLookAt.position, 1);
        yield return new WaitForSeconds(0.5f);
        tutorialGuyExplosion.SetActive(false);

        AudioSource tutGuyAudSource = tutorialGuy.audioSource;
        tutGuyAudSource.clip = tutorialVoiceShort;
        tutGuyAudSource.Play();
        yield return tutorialGuy.DialogueWait();

        tutorialGuyExplosion.SetActive(true);
        yield return new WaitForSeconds(.3f);
        tutorialGuy.transform.parent.gameObject.SetActive(false);

        PlayerController.instance.SetFrozen(false);
        yield return new WaitForSeconds(2f);
        tutorialGuyExplosion.SetActive(false);
    }

    private IEnumerator Tutorial()
    {
        PlayerController.instance.SetFrozen(true);
        yield return PlayerController.instance.LookAt(monsterAnim.transform.position, 1);

        yield return new WaitForSeconds(5f);

        tutorialGuyExplosion.SetActive(true);
        yield return new WaitForSeconds(.3f);
        tutorialGuy.transform.parent.gameObject.SetActive(true);
        yield return PlayerController.instance.LookAt(tutorialGuyLookAt.position, 1);

        AudioSource tutGuyAudSource = tutorialGuy.audioSource;
        tutGuyAudSource.clip = tutorialVoices[0];
        tutGuyAudSource.Play();
        yield return tutorialGuy.DialogueWait();

        tutorialGuyExplosion.SetActive(false);
        handGun.GetComponent<FingerGun>().enabled = true;
        tutGuyAudSource.clip = tutorialVoices[1];
        tutGuyAudSource.Play();
        yield return new WaitForSeconds(.8f);
        yield return tutorialGuy.DialogueWait();

        handGun.SetTrigger("Shoot");
        tutGuyAudSource.clip = tutorialVoices[2];
        tutGuyAudSource.Play();
        yield return new WaitForSeconds(1.2f);
        yield return tutorialGuy.DialogueWait();

        tutorialGuyExplosion.SetActive(true);
        yield return new WaitForSeconds(.3f);
        tutorialGuy.transform.parent.gameObject.SetActive(false);

        PlayerController.instance.SetFrozen(false);
        yield return new WaitForSeconds(2f);
        tutorialGuyExplosion.SetActive(false);
    }

    public void CaughtPlayer()
    {
        waiting = false;
    }

    public void MonsterHit()
    {
        StartCoroutine(MonsterHitIndicator());
    }

    public void MonsterDeath()
    {
        monsterAnim.SetBool("Dead", true);
        areaController.SetActive(false);
        AmbientManager.instance.ChangeAmbientColor(AmbientManager.instance.defaultColor);
        audioSourceMonster.PlayOneShot(monsterDeadSound);
        teleporter.SetActive(true);
        enabled = false;
    }

    private IEnumerator MonsterHitIndicator()
    {
        monsterHitIndicator.SetActive(true);
        yield return new WaitForSeconds(0.4f);
        monsterHitIndicator.SetActive(false);
    }
}
