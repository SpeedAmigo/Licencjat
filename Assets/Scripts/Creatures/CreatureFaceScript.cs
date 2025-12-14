using FishNet.CodeGenerating;
using FishNet.Component.Transforming;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class CreatureFaceScript : NetworkBehaviour
{
    [Header("Eye Decals")]
    [SerializeField] private GameObject eyeDecal;

    //[AllowMutableSyncType] private SyncVar<Vector3> syncedEyePosition;
    
    [Header("Look settings")]
    [SerializeField] private bool lookAtPlayer = true;
    [SerializeField] private LookMode lookMode = LookMode.ClosestPlayer;
    
    [Tooltip("If checked z axis will always stay at original position" +
             " ensuring the eye will not fly towards the player")]
    [SerializeField] private bool ignoreZAxis = true;
    
    [Header("Target offset settings")]
    [SerializeField] private float xTargetOffset = 0f;
    [SerializeField] private float yTargetOffset = 0f;
    [SerializeField] private float zTargetOffset = 0f;
    
    [Header("Look radius settings")]
    [SerializeField] private float lookRadius;
    [SerializeField] private float lookSpeed;

    [SerializeField] private FrogScript _frogScript;
    private Vector3 _originalPosition;

    private enum LookMode
    {
        ClosestPlayer,
        FirstPlayer,
        LastPlayer
    }
    
    private void Start()
    {
        _originalPosition = Vector3.zero;
        //_frogScript = GetComponent<FrogScript>();
    }

    private void Update()
    {
        Vector3? targetPos = null;

        if (_frogScript != null && _frogScript.playersInRange.Count > 0)
        {
            switch (lookMode)
            {
                case LookMode.ClosestPlayer:
                {
                    GameObject closestPlayer = null;
                    float closestDistance = float.MaxValue;

                    foreach (var player in _frogScript.playersInRange)
                    {
                        if (player == null) continue;
                    
                        float distance = Vector3.Distance(transform.position, player.transform.position);
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestPlayer = player;
                        }
                    }

                    if (closestPlayer != null)
                    {
                        targetPos = closestPlayer.transform.position;
                    }
                } break;
            
                case LookMode.FirstPlayer:
                    targetPos = _frogScript.playersInRange[0].transform.position;
                    break;
                case LookMode.LastPlayer:
                    targetPos = _frogScript.playersInRange[^1].transform.position;
                    break;
            }
        }
        
        Vector3 desiredPosition = _originalPosition;

        if (targetPos.HasValue && lookAtPlayer)
        {
            Vector3 worldDir = (targetPos.Value - eyeDecal.transform.position).normalized;
            Vector3 localDir = eyeDecal.transform.parent.InverseTransformDirection(worldDir);
            
            desiredPosition = _originalPosition + localDir * lookRadius;
        }
        
        Vector3 offset = new Vector3(xTargetOffset, yTargetOffset, zTargetOffset);
        desiredPosition += offset;

        if (ignoreZAxis)
        {
            desiredPosition.z = _originalPosition.z;
        }

        /*if (IsServerInitialized)
        {
            syncedEyePosition.Value = desiredPosition;
        }*/
        
        //Vector3 targetLocal = IsServer ? syncedEyePosition.Value : syncedEyePosition.Value;
        
        eyeDecal.transform.localPosition = Vector3.Lerp(
            eyeDecal.transform.localPosition,
            desiredPosition,
            Time.deltaTime * lookSpeed
        );
    }
}
