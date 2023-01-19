using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpossibleCorner : MonoBehaviour
{
    [SerializeField] private GameObjectArray[] states;
    private int nextState = 0;

    public Vector3 teleportOffset;

    public void ResetState()
    {
        nextState = 0;
        NextState();
    }

    public void NextState()
    {
        for (int i = 0; i < states.Length; i++)
            foreach (GameObject obj in states[i].objs)
                obj.SetActive(false);

        foreach (GameObject obj in states[nextState].objs)
            obj.SetActive(true);

        nextState++;
    }

    public void Teleport()
    {
        PlayerController.instance.TeleportPlayer(PlayerController.instance.transform.position + teleportOffset);
    }

    [System.Serializable]
    private struct GameObjectArray
    {
        public GameObject[] objs;
    }
}
