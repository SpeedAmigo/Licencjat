using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

public class ItemHolderSway : MonoBehaviour
{
    [Header("Sway Settings")]
    [GUIColor("Green")]
    [Range(1f, 100f)]
    [SerializeField] private float smooth;
    [GUIColor("Green")]
    [Range(0.1f, 100f)]
    [SerializeField] private float multiplier;
    
    [Header("Input Smoothing")]
    [GUIColor("Green")]
    [Range(0.01f, 1f)]
    public float inputSmoothing = 0.1f;

    private Vector2 _smoothedDelta;
    
    private void Update()
    {
        Vector2 rawDelta = Mouse.current.delta.ReadValue();
        
        _smoothedDelta = Vector2.Lerp(_smoothedDelta, rawDelta, inputSmoothing);
        
        float inputX = _smoothedDelta.x * multiplier;
        float inputY = _smoothedDelta.y * multiplier;
        
        Quaternion rotationX = Quaternion.AngleAxis(-inputY, Vector3.right);
        Quaternion rotationY = Quaternion.AngleAxis(inputX, Vector3.up);

        Quaternion targetRotation = rotationX * rotationY;
        
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, smooth * Time.deltaTime);
    }
}
