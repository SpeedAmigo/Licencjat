using DG.Tweening;
using UnityEngine;

public class TutorialHologramScript : MonoBehaviour
{
    [SerializeField] private bool lookAtPlayer;

    private void Update()
    {
        if (!lookAtPlayer) return;
        gameObject.transform.LookAt(Camera.main.transform.position, Vector3.up);
    }
}
