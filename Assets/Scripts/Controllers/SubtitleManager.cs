using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SubtitleManager : MonoBehaviour
{
    public static SubtitleManager Instance { get; private set; }

    [SerializeField] private TMP_Text text;
    [SerializeField] private CanvasGroup canvasGroup;

    private void Start()
    {
        Instance = this;
        SetText("");
    }

    public void SetText(string text)
    {
        this.text.text = text;
        canvasGroup.alpha = text == "" ? 0f : 1f;
    }
}
