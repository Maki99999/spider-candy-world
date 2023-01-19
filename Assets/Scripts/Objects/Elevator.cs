using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Elevator : MonoBehaviour
{
    public bool isBroken = false;
    public Animator anim;
    public AudioSource music;
    public AudioSource doorAudio;
    public AudioClip openSound;
    public AudioClip closeSound;

    [Space(10)]
    public TMP_Text text;
    public TMP_Text topText;
    public Transform keypadCamPos;
    public Transform playerElevatorPos;
    public Transform darkenedGap;

    [Space(10), SerializeField] private List<FloorWithNumber> floors;
    private Dictionary<int, GameObject> floorsDict;
    public GameObject defaultFloor;
    private GameObject currentFloor;

    private bool busy = false;
    private bool waitingForInput = false;
    private int currentLevel = 1;
    private int inputLevel = -1;

    private void Start()
    {
        if (isBroken)
            return;

        floorsDict = new Dictionary<int, GameObject>();
        foreach (FloorWithNumber floorWithNumber in floors)
        {
            floorsDict.Add(floorWithNumber.floorNumber, floorWithNumber.floor);
            if (floorWithNumber.floor.activeSelf)
            {
                currentLevel = floorWithNumber.floorNumber;
            }
        }

        text.text = "" + currentLevel;
        topText.text = "" + currentLevel;
        inputLevel = -1;
        currentFloor = floorsDict[currentLevel];
    }

    private void Update()
    {
        if (waitingForInput && InputManager.PressingBack())
            Done();
    }

    public void PressedButton()
    {
        if (!busy)
            StartCoroutine(CallElevator());
    }

    IEnumerator CallElevator()
    {
        busy = true;
        yield return new WaitForSeconds(2f);
        anim.SetBool("Open", true);
        doorAudio.clip = openSound;
        doorAudio.Play();

        if (isBroken)
        {
            yield return new WaitForSeconds(3f);
            anim.SetBool("Open", false);
            doorAudio.clip = closeSound;
            doorAudio.Play();
            busy = false;
        }
    }

    public void UsedKeypad()
    {
        if (waitingForInput)
            return;

        CameraController.instance.AddCamera(keypadCamPos, null, null, true);
        InputManager.instance.FreeCursor(this);
        Default.PlayerController.instance.SetFrozen(true);

        inputLevel = -1;
        waitingForInput = true;
    }

    public void PressedNumber(int number)
    {
        if (!waitingForInput)
            return;

        if (inputLevel < 0)
        {
            if (number != 0)
                inputLevel = number;
        }
        else
        {
            inputLevel = inputLevel * 10 + number;
        }

        UpdateText();
    }

    private void UpdateText()
    {
        if (inputLevel < 1)
        {
            inputLevel = -1;
            text.text = "";
        }
        else
        {
            text.text = "" + inputLevel;
        }
    }

    public void PressedDel()
    {
        if (!waitingForInput)
            return;

        if (inputLevel < 9)
        {
            inputLevel = -1;
        }
        else
        {
            inputLevel /= 10;
        }

        UpdateText();
    }

    public void PressedConfirm()
    {
        if (!waitingForInput)
            return;

        if (inputLevel > 0)
        {
            StartCoroutine(ElevatorAnim());
        }
    }

    private IEnumerator ElevatorAnim()
    {
        waitingForInput = false;
        Default.PlayerController.instance.TeleportPlayer(playerElevatorPos);
        CameraController.instance.RemoveCamera(keypadCamPos);
        InputManager.instance.LockCursor(this);
        yield return new WaitForSeconds(1f);
        anim.SetBool("Open", false);
        doorAudio.clip = closeSound;
        doorAudio.Play();
        yield return new WaitForSeconds(2f);
        music.Play();

        int oldLevel = currentLevel;
        currentFloor.SetActive(false);
        currentLevel = inputLevel;

        //"move" elevator
        CameraController.instance.shakingEffect = true;
        int floorDiff = currentLevel - oldLevel;
        float rate = 1f / 10f;
        for (float f = 0f; f <= 1f; f += rate * Time.deltaTime)
        {
            float fSmooth = Mathf.SmoothStep(0, 1, f);
            topText.text = "" + (int)(oldLevel + floorDiff * fSmooth);

            float height = floorDiff * -3.15f * fSmooth;
            if (height > 0)
                height %= 3.15f;
            else
                height = ((height * -1) % 3.15f) * -1 + 3.15f;
            darkenedGap.localPosition = new Vector3(0, height, -0.15f);

            yield return null;
        }
        darkenedGap.localPosition = new Vector3(0, 3.16f, -0.15f);
        topText.text = "" + currentLevel;

        currentFloor = floorsDict.GetValueOrDefault(currentLevel, defaultFloor);
        currentFloor.SetActive(true);
        music.Stop();
        CameraController.instance.shakingEffect = false;

        yield return new WaitForSeconds(1f);
        anim.SetBool("Open", true);
        doorAudio.clip = openSound;
        doorAudio.Play();
        Done();
    }

    private void Done()
    {
        CameraController.instance.RemoveCamera(keypadCamPos);
        InputManager.instance.LockCursor(this);
        Default.PlayerController.instance.SetFrozen(false);
        waitingForInput = false;
        busy = false;
    }

    [System.Serializable]
    private struct FloorWithNumber
    {
        public GameObject floor;
        public int floorNumber;

        public FloorWithNumber(GameObject floor, int floorNumber)
        {
            this.floor = floor;
            this.floorNumber = floorNumber;
        }
    }
}
