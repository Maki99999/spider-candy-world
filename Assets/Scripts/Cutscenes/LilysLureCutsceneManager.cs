using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LilysLureCutsceneManager : MonoBehaviour
{
    public UnityEngine.Playables.PlayableDirector timeline;
    public UnityEngine.Playables.PlayableAsset timelinePlayableStart;
    public UnityEngine.Playables.PlayableAsset timelinePlayableMid;
    public UnityEngine.Playables.PlayableAsset timelinePlayableEnd;
    public UnityEngine.Playables.PlayableAsset timelinePlayableEnd2;
    public Transform cutsceneCamTransform;
    public AreaController startAreaController;
    public AreaController endAreaController;
    public bool jumpToEnd2;

    [Space(10)] public string[] subtitles;
    [Space(10)] public string[] subtitles2;
    [Space(10)] public string[] subtitles3;
    private int currentSubtitle = -1;

    private bool triggered = false;

    void Update()
    {
        if (!triggered && Input.GetKey(KeyCode.Keypad2))
        {
            triggered = true;

            PlayerController.instance.SetFrozen(true);
            PlayerController.instance.gameObject.SetActive(false);
            CameraController.instance.AddCamera(cutsceneCamTransform, null, null, true, true);
            if (!jumpToEnd2)
            {
                startAreaController.UpdateArea();
                timeline.playableAsset = timelinePlayableStart;
            }
            else
            {
                endAreaController.UpdateArea();
                timeline.playableAsset = timelinePlayableEnd2;
            }
            timeline.Play();
        }
    }

    public void NextSubtitles(int subtitleSetNum)
    {
        string[] subtitleSet;
        if (subtitleSetNum == 1)
            subtitleSet = subtitles;
        else if (subtitleSetNum == 2)
            subtitleSet = subtitles2;
        else
            subtitleSet = subtitles3;

        currentSubtitle++;
        if (currentSubtitle >= subtitleSet.Length)
            SubtitleManager.Instance.SetText("");
        else
            SubtitleManager.Instance.SetText(subtitleSet[currentSubtitle]);
    }

    public void NextPart()
    {
        currentSubtitle = -1;
        timeline.Stop();
        timeline.playableAsset = timeline.playableAsset == timelinePlayableStart ? timelinePlayableMid : timelinePlayableEnd;
        if (timeline.playableAsset == timelinePlayableEnd)
            endAreaController.UpdateArea();
        timeline.Play();
    }

    public void CutsceneFinished()
    {
        PlayerController.instance.gameObject.SetActive(true);
        PlayerController.instance.SetFrozen(false);
        CameraController.instance.RemoveCamera(cutsceneCamTransform, true);
        triggered = false;
    }
}
