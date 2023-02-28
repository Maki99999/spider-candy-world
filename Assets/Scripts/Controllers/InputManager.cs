using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static bool PressingUse()
    {
        return Input.GetKey(KeyCode.E);
    }

    public static bool PressingConfirm()
    {
        return Input.GetKey(KeyCode.Return);
    }

    public static bool PressingZoom()
    {
        return Input.GetKey(KeyCode.Mouse2);
    }

    public static bool PressingBack()
    {
        return Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.Mouse1);
    }

    public static InputManager instance { get; private set; }

    private List<MonoBehaviour> freedList = new List<MonoBehaviour>();
    [SerializeField] private RectTransform cursor;
    private Canvas cursorCanvas;

    private void Awake()
    {
        instance = this;
        UpdateCursorState();
        cursorCanvas = cursor.GetComponentInParent<Canvas>();
    }

    private void Update()
    {
        Vector2 movePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            cursorCanvas.transform as RectTransform,
            Input.mousePosition, cursorCanvas.worldCamera,
            out movePos);
        cursor.position = cursorCanvas.transform.TransformPoint(movePos);
    }

    public void FreeCursor(MonoBehaviour initiatedKeyObj)
    {
        freedList.Add(initiatedKeyObj);
        UpdateCursorState();
    }

    public void LockCursor(MonoBehaviour initiatedKeyObj)
    {
        freedList.Remove(initiatedKeyObj);
        UpdateCursorState();
    }

    private void UpdateCursorState()
    {
        Cursor.visible = false;

        if (freedList.Count > 0)
        {
            Cursor.lockState = CursorLockMode.None;
            cursor.gameObject.SetActive(true);
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            cursor.gameObject.SetActive(false);
        }
    }
}
