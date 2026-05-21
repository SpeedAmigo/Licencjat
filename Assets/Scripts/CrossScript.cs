using UnityEngine;

public class CrossScript : MonoBehaviour
{
    [SerializeField] private float destroyTime;
    [SerializeField] private GameObject cross;

    private void Start()
    {
        Destroy(gameObject, destroyTime);
    }
    
    private void Update()
    {
        cross.transform.LookAt(Camera.main.transform.position);
    }
}
