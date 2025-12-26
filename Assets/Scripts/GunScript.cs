using UnityEngine;

public class GunScript : ObjectPickable, IPrimaryClick
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPrimaryClick()
    {
        Debug.Log("OnPrimaryClick");
    }
}
