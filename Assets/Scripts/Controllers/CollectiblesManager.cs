using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectiblesManager : MonoBehaviour
{
    public static CollectiblesManager instance;

    public Camera uICam;

    [Header("Gems")]
    public Animator gemUIAnim;
    public TMPro.TMP_Text gemsCountText;
    public Transform gemsParent;
    public Rigidbody jarRigidbody;
    public GameObject jarSafetyColliders;
    public Transform[] jarContent;
    private Vector3[] jarContentIdlePos;
    private Quaternion[] jarContentIdleRots;
    public GameObject fakeGem;
    public Transform fakeGemSpawn;
    public Transform jarHiddenPos;
    public Transform jarVisiblePos;

    private bool gemsVisible = false;
    private bool preparingDrop = false;
    private Coroutine jarMovingCorou;
    private int gemsToDrop = 0;

    private int gemCount = 0;

    private int lastScreenX;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        //gems
        UpdateGemsPos();
        jarContentIdlePos = new Vector3[jarContent.Length];
        jarContentIdleRots = new Quaternion[jarContent.Length];
        for (int i = 0; i < jarContent.Length; i++)
        {
            jarContentIdlePos[i] = jarContent[i].localPosition;
            jarContentIdleRots[i] = jarContent[i].localRotation;
        }
    }

    private void Update()
    {
        if (lastScreenX != Screen.width)
        {
            UpdateGemsPos();
        }
        lastScreenX = Screen.width;
    }

    private void UpdateGemsPos()
    {
        Vector3 screenPoint = new Vector3(1f, 0.5f, uICam.nearClipPlane);
        Vector3 worldPoint = uICam.ViewportToWorldPoint(screenPoint);

        Vector3 objectPosition = gemsParent.position;
        objectPosition.x = worldPoint.x - 1.75f;
        gemsParent.position = objectPosition;
    }

    public void AddCollectible(string name, int count)
    {
        if (name == "gem")
            AddGems(count);
    }

    private void AddGems(int count)
    {
        gemCount += count;
        gemsCountText.text = "" + gemCount;
        gemsToDrop += count;

        if (!gemsVisible)
        {
            for (int i = 0; i < jarContent.Length; i++)
            {
                jarContent[i].localPosition = jarContentIdlePos[i];
                jarContent[i].localRotation = jarContentIdleRots[i];
            }

            jarMovingCorou = StartCoroutine(MoveJar());
        }
        else if (!preparingDrop)
        {
            if (jarMovingCorou != null)
                StopCoroutine(jarMovingCorou);
            jarMovingCorou = StartCoroutine(MoveJar());
        }
    }

    private IEnumerator MoveJar()
    {
        //slide in
        preparingDrop = true;
        gemsVisible = true;
        jarSafetyColliders.SetActive(true);
        gemUIAnim.SetBool("Visible", true);

        Vector3 startPos = jarHiddenPos.position;
        Quaternion startRot = jarHiddenPos.rotation;
        Vector3 endPos = jarVisiblePos.position;
        Quaternion endRot = jarVisiblePos.rotation;

        float rate = (7f / Vector3.Distance(startPos, endPos));
        float smoothF = 0;
        float t = Mathf.InverseLerp(startPos.magnitude, endPos.magnitude, jarRigidbody.position.magnitude);
        float startF = 1f - Mathf.Pow(1 - t, 1 / 3f);
        for (float f = startF; f <= 1f; f += rate * Time.fixedDeltaTime)
        {
            smoothF = 1 - Mathf.Pow(1 - f, 3);
            yield return new WaitForFixedUpdate();

            jarRigidbody.MovePosition(Vector3.Lerp(startPos, endPos, smoothF));
            jarRigidbody.MoveRotation(Quaternion.Lerp(startRot, endRot, smoothF));
        }
        jarRigidbody.MovePosition(endPos);
        jarRigidbody.MoveRotation(endRot);
        jarSafetyColliders.SetActive(false);

        //drop gems
        do
        {
            while (gemsToDrop > 0)
            {
                Instantiate(fakeGem, fakeGemSpawn.position, Random.rotation, fakeGemSpawn);
                gemsToDrop--;
                yield return new WaitForSeconds(.2f);
            }
            yield return new WaitForSeconds(.6f);
        } while (gemsToDrop > 0);

        //slide out
        preparingDrop = false;
        gemUIAnim.SetBool("Visible", false);

        startPos = jarVisiblePos.position;
        endPos = jarHiddenPos.position;
        rate = (7f / Vector3.Distance(startPos, endPos));
        for (float f = 0; f <= 1f; f += rate * Time.fixedDeltaTime)
        {
            smoothF = Mathf.Pow(f, 3);
            yield return new WaitForFixedUpdate();

            jarRigidbody.MovePosition(Vector3.Lerp(startPos, endPos, smoothF));
        }
        gemsVisible = false;
    }
}