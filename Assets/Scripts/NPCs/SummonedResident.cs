using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SummonedResident : MonoBehaviour, Default.Useable
{
    public List<string> dialogueTextPre;
    public List<string> dialogueTextMid;
    public List<string> dialogueTextPost;
    public DialogueVoice dialogueVoice = DialogueVoice.DEFAULT;
    public Transform dialogueCameraPos;

    [Space(10)]
    public Default.PlayerController player;
    public Animator anim;

    private bool used = false;

    public void Use()
    {
        if (used)
            return;

        used = true;
        StartCoroutine(Cutscene());
    }

    private IEnumerator Cutscene()
    {
        Default.CameraController.instance.AddStaticCamPos(dialogueCameraPos, null, null, "Dialogue");
        ulong camPosId = Default.CameraController.instance.SetCameraMode(Default.CameraMode.STATIC);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(player.transform.position, out hit, 4f, NavMesh.AllAreas))
            player.TeleportPlayer(hit.position, player.transform.eulerAngles);

        yield return DialogueManager.instance.StartDialogue(dialogueTextPre, dialogueVoice);
        anim.SetTrigger("spawn");
        yield return new WaitForSeconds(3.3f);

        yield return DialogueManager.instance.StartDialogue(dialogueTextMid, dialogueVoice);
        anim.SetTrigger("teleport");
        yield return new WaitForSeconds(4f);

        yield return DialogueManager.instance.StartDialogue(dialogueTextPost, dialogueVoice);

        Default.CameraController.instance.RemoveStaticCamPos(dialogueCameraPos);
        Default.CameraController.instance.ResetCameraMode(camPosId);
    }
}
