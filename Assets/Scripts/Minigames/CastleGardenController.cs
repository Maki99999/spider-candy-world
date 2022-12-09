using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastleGardenController : MonoBehaviour
{
    public static CastleGardenController instance { get; private set; }

    public Default.PlayerController player;
    public Animator fadeAnim;

    public List<string> caughtDialogue;
    public AudioSource caughtTeleportSound;

    private Transform latestCheckpoint;
    private bool inCaughtCutscene = false;

    private void Awake()
    {
        instance = this;
    }

    public void PlayerCaught(GuardCard guard)
    {
        if (!inCaughtCutscene)
            StartCoroutine(CaughtCutscene(guard));
    }

    public void CheckpointReached(Transform checkpoint)
    {
        latestCheckpoint = checkpoint;
    }

    private IEnumerator CaughtCutscene(GuardCard guard)
    {
        inCaughtCutscene = true;
        player.SetFrozen(true);
        guard.enabled = false;

        var dial = StartCoroutine(DialogueManager.instance.StartDialogue(caughtDialogue));

        Vector3 pRot = player.GetRotation();
        Quaternion gRot = guard.transform.rotation;

        Vector3 pRotNew = Quaternion.LookRotation(guard.transform.position - player.transform.position, Vector3.up).eulerAngles;
        Quaternion gRotNew = Quaternion.LookRotation(player.transform.position - guard.transform.position, Vector3.up);

        float rate = 2f;
        float fSmooth;
        for (float f = 0f; f <= 1f; f += rate * Time.deltaTime)
        {
            fSmooth = 1 - Mathf.Pow(1 - f, 3);
            player.SetRotationLerp(pRot, pRotNew, fSmooth);
            guard.transform.rotation = Quaternion.Lerp(gRot, gRotNew, fSmooth);

            yield return null;
        }
        yield return dial;

        caughtTeleportSound.Play();
        fadeAnim.SetBool("White", true);
        yield return new WaitForSeconds(0.6f);
        player.TeleportPlayer(latestCheckpoint);
        fadeAnim.SetBool("White", false);

        guard.enabled = true;
        player.SetFrozen(false);
        inCaughtCutscene = false;
    }
}
