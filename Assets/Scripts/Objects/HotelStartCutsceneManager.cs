using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotelStartCutsceneManager : MonoBehaviour
{
    public UnityEngine.Playables.PlayableDirector timeline;
    [Space(10)] public Transform cutsceneCamTransform;
    public Transform standUpPos;

    private bool triggered = false;

    void Update()
    {
        if (!triggered && Input.GetKey(KeyCode.Keypad1))
        {
            triggered = true;

            Default.PlayerController.instance.SetFrozen(true);
            Default.PlayerController.instance.gameObject.SetActive(false);
            CameraController.instance.AddCamera(cutsceneCamTransform, null, null, true, true);
            timeline.Play();
        }
    }

    public void CutsceneFinished()
    {
        Default.PlayerController.instance.gameObject.SetActive(true);
        Default.PlayerController.instance.TeleportPlayer(standUpPos);
        Default.PlayerController.instance.SetFrozen(false);
        CameraController.instance.RemoveCamera(cutsceneCamTransform, true);
    }
}
