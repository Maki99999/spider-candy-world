using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{
    public MeDialogue meDialogue;
    public Collider playerBarrier;
    public Transform chessTransf;
    public Animator chessAnim;

    [Space(15)]
    public Transform playerDancePos;
    public Transform meDancePos;
    public Transform camDancePos;
    public CanvasGroup danceCanvasGroup;
    public GameObject musicIntro;
    public GameObject musicMain;
    public GameObject[] notesMe;
    public GameObject[] notesPlayer;
    public GameObject[] indicatorsMe;
    public GameObject[] indicatorsPlayer;
    public AudioSource danceAudioSource;
    public AudioClip[] soundsMe;
    public AudioClip[] soundsPlayer;
    public TMPro.TMP_Text scoreMe;
    public TMPro.TMP_Text scorePlayer;

    [Space(15)]
    public Transform mePlatform;
    public FingerGun fingerGun;
    public GameObject gunProjectile;
    public Transform projectileSpawnPos;
    public Transform laser;
    public AudioSource meHitSound;
    public GameObject meHitIndicator;
    public AudioSource laserSound;
    public GameObject missileProjectile;

    [Space(15)]
    public Transform knifeArea;
    public Transform cutsceneCam;
    public UnityEngine.Playables.PlayableDirector cutsceneTimeline;

    [Space(30)]
    public AudioClip soundTelepFound;
    public AudioClip soundTelepReq;
    public AudioClip soundChessGame;
    public AudioClip soundChessToDance;
    public AudioClip soundDanceToBattle;
    public AudioClip soundBattleEnd;
    public AudioClip soundJustDie;

    private Transform me;
    private Coroutine randomPoseCoroutine;
    private bool inBattle = false;
    private Coroutine meHitAnim;

    private void Start()
    {
        me = meDialogue.transform.parent;
    }

    public void Begin()
    {
        StartCoroutine(JustGotHere());
    }

    private IEnumerator JustGotHere()
    {
        PlayerController.instance.SetFrozen(true);

        meDialogue.audioSource.clip = soundTelepFound;
        meDialogue.audioSource.Play();
        var lookAt = StartCoroutine(PlayerController.instance.LookAt(meDialogue.lookAtPos.position));
        yield return meDialogue.DialogueWait();
        yield return lookAt;

        PlayerController.instance.SetFrozen(false);
    }

    public void RequestTeleporter()
    {
        StartCoroutine(PreChess());
    }

    private IEnumerator PreChess()
    {
        PlayerController.instance.SetFrozen(true);

        meDialogue.audioSource.clip = soundTelepReq;
        meDialogue.audioSource.Play();
        var lookAt = StartCoroutine(PlayerController.instance.LookAt(meDialogue.lookAtPos.position));
        yield return meDialogue.DialogueWait();
        yield return lookAt;

        CameraController.instance.shakingEffect = true;

        float rate = 1 / 6f;
        float smoothF;
        Vector3 oldPlayerPos = PlayerController.instance.transform.position;
        Quaternion oldPlayerRot = Quaternion.Euler(PlayerController.instance.GetRotation());
        Vector3 newPlayerPos = transform.position + Vector3.up * 0.08f + Vector3.back * 1;
        Quaternion newPlayerRot = Quaternion.Euler(30, 0, 0);
        for (float f = 0f; f <= 1f; f += rate * Time.deltaTime)
        {
            smoothF = Mathf.SmoothStep(0, 1, f);

            PlayerController.instance.TeleportPlayer(Vector3.Lerp(oldPlayerPos, newPlayerPos, smoothF), Quaternion.Lerp(oldPlayerRot, newPlayerRot, smoothF).eulerAngles);
            me.localPosition = Vector3.Lerp(Vector3.zero, Vector3.forward * 1, smoothF);
            chessTransf.localPosition = Vector3.Lerp(Vector3.down * 0.25f, Vector3.zero, f);

            yield return null;
        }
        for (float f = 0f; f <= 1f; f += rate * Time.deltaTime)
        {
            chessTransf.localPosition = Vector3.Lerp(Vector3.zero, Vector3.up * 0.75f, f);
            yield return null;
        }

        CameraController.instance.shakingEffect = false;

        chessAnim.SetTrigger("Play");
        meDialogue.audioSource.clip = soundChessGame;
        meDialogue.audioSource.Play();
        yield return meDialogue.DialogueWait();

        meDialogue.audioSource.clip = soundChessToDance;
        meDialogue.audioSource.Play();
        lookAt = StartCoroutine(PlayerController.instance.LookAt(meDialogue.lookAtPos.position));
        var dialogue = StartCoroutine(meDialogue.DialogueWait());

        for (float f = 0f; f <= 1f; f += .5f * Time.deltaTime)
        {
            chessTransf.localPosition = Vector3.Lerp(Vector3.up * 0.75f, Vector3.down * 0.25f, f);
            yield return null;
        }
        yield return dialogue;
        yield return lookAt;

        yield return DanceBattle();
    }

    private IEnumerator DanceBattle()
    {
        PlayerController.instance.TeleportPlayer(playerDancePos);
        me.position = meDancePos.position;
        CameraController.instance.AddCamera(camDancePos, null, null, false, true);
        musicIntro.SetActive(true);

        yield return new WaitForSeconds(3f);
        for (float f = 0f; f <= 1f; f += .25f * Time.deltaTime)
        {
            danceCanvasGroup.alpha = f;
            yield return null;
        }
        yield return new WaitForSeconds(3f);

        musicIntro.SetActive(false);
        musicMain.SetActive(true);

        yield return DanceBattleStage(0);
        yield return DanceBattleStage(1);
        yield return DanceBattleStage(2);

        musicIntro.SetActive(true);
        musicMain.SetActive(false);

        yield return new WaitForSeconds(3f);
        for (float f = 0f; f <= 1f; f += .5f * Time.deltaTime)
        {
            danceCanvasGroup.alpha = 1 - f;
            yield return null;
        }
        musicIntro.SetActive(false);
        CameraController.instance.RemoveCamera(camDancePos);

        yield return Battle();
    }

    private IEnumerator DanceBattleStage(int level)
    {
        int notesCount = 15 + level * level * 20;
        float length = 10 + level;
        float[] notesTiming = new float[notesCount];
        int[] notes = new int[notesCount];
        for (int i = 0; i < notesCount; i++)
        {
            notesTiming[i] = (i + 1) * (length / notesCount);//Random.Range(0f, length);
            notes[i] = Random.Range(0, 4);
        }
        System.Array.Sort(notesTiming);

        yield return new WaitForSeconds(notesTiming[0]);
        StartCoroutine(Note(notes[0], false));
        for (int i = 1; i < notesCount; i++)
        {
            yield return new WaitForSeconds(notesTiming[i] - notesTiming[i - 1]);
            StartCoroutine(Note(notes[i], false));
        }

        yield return new WaitForSeconds(4f);
        musicIntro.SetActive(true);
        musicMain.SetActive(false);
        yield return new WaitForSeconds(3f);
        musicIntro.SetActive(false);
        musicMain.SetActive(true);

        yield return new WaitForSeconds(notesTiming[0]);
        StartCoroutine(Note(notes[0], true));
        for (int i = 1; i < notesCount; i++)
        {
            yield return new WaitForSeconds(notesTiming[i] - notesTiming[i - 1]);
            StartCoroutine(Note(notes[i], true));
        }

        yield return new WaitForSeconds(4f);
        musicIntro.SetActive(true);
        musicMain.SetActive(false);
        yield return new WaitForSeconds(3f);
        musicIntro.SetActive(false);
        musicMain.SetActive(true);
    }

    private IEnumerator Note(int noteType, bool playersTurn)
    {
        GameObject[] notes = playersTurn ? notesPlayer : notesMe;
        Transform note = Instantiate(notes[noteType], notes[noteType].transform.parent).transform;

        while (note.localPosition.y > -230)
        {
            note.position += Vector3.down * 200 * Time.deltaTime;
            yield return null;
        }

        var indicators = playersTurn ? indicatorsPlayer : indicatorsMe;
        var indicator = indicators[Random.Range(0, indicators.Length)];
        if (playersTurn)
        {
            danceAudioSource.PlayOneShot(soundsPlayer[noteType]);
            Destroy(Instantiate(indicator, indicator.transform.parent), 3f);
            scorePlayer.text = "" + (System.Int32.Parse(scorePlayer.text) + Random.Range(100, 1000));
        }
        else
            StartCoroutine(MeNoteDelayed(danceAudioSource, soundsMe[noteType], indicator, Random.Range(0.1f, 0.3f)));

        if (randomPoseCoroutine != null)
            StopCoroutine(randomPoseCoroutine);
        randomPoseCoroutine = StartCoroutine(RandomDancePos(playersTurn));

        while (note.localPosition.y > -350)
        {
            note.position += Vector3.down * 200 * Time.deltaTime;
            yield return null;
        }
        Destroy(note.gameObject);
    }

    private IEnumerator RandomDancePos(bool playersTurn)
    {
        if (playersTurn)
        {
            var anim = PlayerController.instance.GetComponentInChildren<Animator>();
            anim.SetBool("Dance", false);
            yield return null;
            anim.SetBool("Dance", true);
            yield return new WaitForSeconds(0.5f);
            anim.SetBool("Dance", false);
        }
        else
        {
            meDialogue.meRenderer.material = meDialogue.speakingMaterials[Random.Range(0, meDialogue.speakingMaterials.Length)];
            yield return new WaitForSeconds(0.5f);
            meDialogue.meRenderer.material = meDialogue.idleMaterial;
        }
    }

    private IEnumerator MeNoteDelayed(AudioSource audioSource, AudioClip audioClip, GameObject indicator, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        audioSource.PlayOneShot(audioClip);
        Destroy(Instantiate(indicator, indicator.transform.parent), 3f);
        scoreMe.text = "" + (System.Int32.Parse(scoreMe.text) + Random.Range(1, 10));
    }

    private IEnumerator Battle()
    {
        meDialogue.audioSource.clip = soundDanceToBattle;
        meDialogue.audioSource.Play();
        var dialogueWait = StartCoroutine(meDialogue.DialogueWait());

        Vector3 meStartPos = me.position;
        Vector3 meEndPos = transform.position + transform.forward * 3f;
        Vector3 playerStartPos = PlayerController.instance.transform.position;
        Vector3 playerEndPos = transform.position - transform.forward * 3f + transform.up * 0.08f;
        Vector3 playerStartRot = PlayerController.instance.GetRotation();
        Vector3 playerEndRot = Quaternion.LookRotation(transform.forward, transform.up).eulerAngles;
        float smoothF;
        for (float f = 0f; f <= 1f; f += .5f * Time.deltaTime)
        {
            smoothF = Mathf.SmoothStep(0, 1, f);
            me.position = Vector3.Lerp(meStartPos, meEndPos, smoothF);
            PlayerController.instance.TeleportPlayer(Vector3.Lerp(playerStartPos, playerEndPos, smoothF));
            PlayerController.instance.SetRotationLerp(playerStartRot, playerEndRot, smoothF);
            yield return null;
        }
        yield return dialogueWait;

        inBattle = true;
        mePlatform.gameObject.SetActive(true);
        PlayerController.instance.SetFrozen(false);
        fingerGun.enabled = true;
        var randomMePoses = StartCoroutine(RandomMePoses());

        while (inBattle)
        {
            switch (Random.Range(0, 3))
            {
                case 1:
                    yield return AttackCenterAndLaser();
                    break;
                case 2:
                    yield return AttackLeftAndRightAndMissiles();
                    break;
                default:
                    yield return AttackMoveAndShoot();
                    break;
            }
        }

        StopCoroutine(randomMePoses);
        PlayerController.instance.SetFrozen(true);

        meDialogue.audioSource.clip = soundBattleEnd;
        meDialogue.audioSource.Play();
        dialogueWait = StartCoroutine(meDialogue.DialogueWait());

        meStartPos = me.position;
        meEndPos = transform.position + transform.forward * 1.5f;
        playerStartPos = PlayerController.instance.transform.position;
        playerEndPos = transform.position - transform.forward * 1.5f + transform.up * 0.08f;
        playerStartRot = PlayerController.instance.GetRotation();
        playerEndRot = Quaternion.LookRotation(transform.forward, transform.up).eulerAngles;
        for (float f = 0f; f <= 1f; f += .5f * Time.deltaTime)
        {
            smoothF = Mathf.SmoothStep(0, 1, f);
            me.position = Vector3.Lerp(meStartPos, meEndPos, smoothF);
            PlayerController.instance.TeleportPlayer(Vector3.Lerp(playerStartPos, playerEndPos, smoothF));
            PlayerController.instance.SetRotationLerp(playerStartRot, playerEndRot, smoothF);
            yield return null;
        }
        yield return dialogueWait;
        mePlatform.gameObject.SetActive(false);
        fingerGun.enabled = false;
        playerBarrier.gameObject.SetActive(false);
        PlayerController.instance.SetFrozen(false);
    }

    public IEnumerator RandomMePoses()
    {
        Renderer meRenderer = meDialogue.meRenderer;
        Material[] speakingMaterials = meDialogue.speakingMaterials;

        while (inBattle)
        {
            meRenderer.material = speakingMaterials[Random.Range(0, speakingMaterials.Length)];
            yield return new WaitForSeconds(Random.Range(0.2f, 0.5f));
        }
        meRenderer.material = meDialogue.idleMaterial;
    }

    private IEnumerator AttackMoveAndShoot()
    {
        var movement = StartCoroutine(MoveInInfininitySymbol());

        yield return new WaitForSeconds(Random.Range(2.5f, 3f));
        float endTime = Time.time + 7.5f;
        while (Time.time < endTime && inBattle)
        {
            var inst = Instantiate(gunProjectile, projectileSpawnPos.position, projectileSpawnPos.rotation, ProjectileCollector.instance.transform);
            Physics.IgnoreCollision(inst.GetComponent<Collider>(), mePlatform.GetComponent<Collider>());
            Physics.IgnoreCollision(inst.GetComponent<Collider>(), playerBarrier);
            inst.transform.LookAt(PlayerController.instance.transform.position + Vector3.up);
            yield return new WaitForSeconds(Random.Range(0.5f, 1f));
        }

        StopCoroutine(movement);
    }

    private IEnumerator MoveInInfininitySymbol()
    {
        float moveSpeed = .5f;
        Bounds bounds = new Bounds(transform.position + Vector3.up * 7.5f + Vector3.forward * 5f, new Vector3(20, 10, 0));

        float angle = moveSpeed * (Time.time + 2f);
        var startPos = bounds.center + Vector3.Scale(bounds.extents, new Vector3(1, Mathf.Sin(angle), 0)) * Mathf.Cos(angle);
        var preStartPos = me.position;
        for (float f = 0f; f <= 1f && inBattle; f += .5f * Time.deltaTime)
        {
            me.position = Vector3.Lerp(preStartPos, startPos, Mathf.Pow(f, 3));
            yield return null;
        }

        while (inBattle)
        {
            angle = moveSpeed * Time.time;
            me.position = bounds.center + Vector3.Scale(bounds.extents, new Vector3(1, Mathf.Sin(angle), 0)) * Mathf.Cos(angle);
            yield return null;
        }
    }

    private IEnumerator MoveToPoint(Vector3 point, float seconds)
    {
        var rate = 1f / seconds;
        var prePos = me.position;
        for (float f = 0f; f <= 1f && inBattle; f += rate * Time.deltaTime)
        {
            me.position = Vector3.Lerp(prePos, point, Mathf.Pow(f, 3));
            yield return null;
        }
    }

    private IEnumerator AttackCenterAndLaser()
    {
        yield return MoveToPoint(transform.position + Vector3.up * 2f + Vector3.forward * 5f, 1f);
        laser.localEulerAngles = new Vector3(0, -90, 0);
        laser.gameObject.SetActive(true);
        var laserSoundCoroutine = StartCoroutine(MoveLaserSound());

        var prePos = me.position;
        for (float f = 0f; f <= 1f && inBattle; f += 0.5f * Time.deltaTime)
        {
            laser.localScale = new Vector3(1, 1, Mathf.SmoothStep(0, 1, f));
            yield return null;
        }

        for (float f = 0f; f <= 1f && inBattle; f += 0.5f * Time.deltaTime)
        {
            laser.localEulerAngles = new Vector3(0, Mathf.SmoothStep(-90, 90, f), 0);
            yield return null;
        }

        for (float f = 0f; f <= 1f && inBattle; f += 1f * Time.deltaTime)
        {
            laser.localScale = new Vector3(1, 1, Mathf.SmoothStep(1, 0, f));
            yield return null;
        }
        if (laserSoundCoroutine != null)
            StopCoroutine(laserSoundCoroutine);
        laser.gameObject.SetActive(false);
    }

    private IEnumerator MoveLaserSound()
    {
        var point = Camera.main.transform.position;
        while (inBattle)
        {
            laserSound.transform.position = laser.position + Vector3.Project(point - laser.position, laser.forward);
            laserSound.volume = Mathf.Clamp(1 - (((point - laserSound.transform.position).magnitude - 2) / 4f), 0.2f, 0.8f);
            yield return null;
        }
    }

    private IEnumerator AttackLeftAndRightAndMissiles()
    {
        var pos1 = transform.position + Vector3.up * 5f + Vector3.forward * 5f + Vector3.right * 5f;
        var pos2 = pos1 - Vector3.right * 10f;
        Vector3[] poss;
        if (Random.value > 0.5f)
            poss = new Vector3[] { pos1, pos2 };
        else
            poss = new Vector3[] { pos2, pos1 };

        foreach (Vector3 pos in poss)
        {
            yield return MoveToPoint(pos, 1f);
            yield return new WaitForSeconds(1f);
            for (int i = 0; i < 5 && inBattle; i++)
            {
                var inst = Instantiate(missileProjectile, projectileSpawnPos.position, projectileSpawnPos.rotation, ProjectileCollector.instance.transform);
                Physics.IgnoreCollision(inst.GetComponent<Collider>(), mePlatform.GetComponent<Collider>());
                inst.transform.forward = Vector3.up;
                inst.GetComponent<Missile>().target = Camera.main.transform;
                yield return new WaitForSeconds(.3f);
            }
        }
    }

    public void MeHit()
    {
        if (meHitAnim != null)
            StopCoroutine(meHitAnim);
        meHitAnim = StartCoroutine(MeHitAnim());
    }

    private IEnumerator MeHitAnim()
    {
        meHitIndicator.SetActive(true);
        meHitSound.Play();

        yield return new WaitForSeconds(0.35f);

        meHitIndicator.SetActive(false);
    }

    public void MeDead()
    {
        inBattle = false;
    }

    public void LaserHitPlayer()
    {
        PlayerController.instance.GetComponentInChildren<PlayerHealthController>().Damage(2);
    }

    public void StartExplainCardboard()
    {
        StartCoroutine(ExplainCardboard());
    }

    private IEnumerator ExplainCardboard()
    {
        PlayerController.instance.SetFrozen(true);

        meDialogue.audioSource.clip = soundJustDie;
        meDialogue.audioSource.Play();
        var lookAt = StartCoroutine(PlayerController.instance.LookAt(meDialogue.lookAtPos.position));
        var dial = StartCoroutine(meDialogue.DialogueWait());

        yield return new WaitForSeconds(15f);
        fingerGun.enabled = true;
        yield return new WaitForSeconds(10f);
        fingerGun.enabled = false;

        yield return dial;
        yield return lookAt;

        CameraController.instance.shakingEffect = true;
        Vector3 startPos = knifeArea.position;
        knifeArea.gameObject.SetActive(true);
        lookAt = StartCoroutine(PlayerController.instance.LookAt(knifeArea.position));

        float rate = 1 / 4f;
        float smoothF;
        for (float f = 0f; f <= 1f; f += rate * Time.deltaTime)
        {
            smoothF = Mathf.SmoothStep(0, 1, f);
            knifeArea.position = startPos + Vector3.Lerp(Vector3.down * 2f, Vector3.zero, f);
            yield return null;
        }
        yield return lookAt;

        CameraController.instance.shakingEffect = false;
        PlayerController.instance.SetFrozen(false);
    }

    public void StartFinalCutscene()
    {
        PlayerController.instance.SetFrozen(true);
        PlayerController.instance.gameObject.SetActive(false);
        cutsceneTimeline.transform.SetParent(null);
        cutsceneCam.SetParent(null);
        CameraController.instance.AddCamera(cutsceneCam, null, null, true);
        me.gameObject.SetActive(false);
        cutsceneTimeline.Play();
    }
}
