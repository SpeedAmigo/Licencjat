using System.Collections;
using DG.Tweening;
using FishNet.Object;
using FMODUnity;
using UnityEngine;

public class GunScript : Weapon, IPrimaryClick, IRechargeable
{
    [SerializeField] private Transform raycastStartPoint;
    [SerializeField] private float shootDistance;
    [SerializeField] private float laserDuration;
    
    [SerializeField] private StatusEffect[] effects;
    
    [Space]
    [SerializeField] EventReference shotSound;

    [SerializeField] private ParticleSystem barrelParticles;
    [SerializeField] private ParticleSystem hitParticles;
    [SerializeField] private LineRenderer lr;
    
    public void OnPrimaryClick()
    {
        if (!CheckDurability())
        {
            Debug.Log("No more power!");
            return;
        }

        Camera.main.DOShakePosition(0.3f, 0.03f, 10, 90f, true);
        SoundCreator.Instance.PlayOneShotAttached(shotSound, gameObject);
        PlayBarrelParticles();
        
        DecreaseDurability();
        Shoot();
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayHitParticles(Vector3 position)
    {
        PlayHitParticlesObservers(position);
    }

    [ObserversRpc]
    private void PlayHitParticlesObservers(Vector3 position)
    {
        hitParticles.transform.position = position;
        hitParticles.Play();
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayBarrelParticles()
    {
        PlayBarrelParticlesObserver();
    }

    [ObserversRpc]
    private void PlayBarrelParticlesObserver()
    {
        barrelParticles.Play();
    }

    private void Shoot()
    {
        Ray ray = new Ray(raycastStartPoint.position, raycastStartPoint.forward);
        Vector3 endPoint;
        
        if (Physics.Raycast(ray, out RaycastHit hit, shootDistance))
        {
            endPoint = hit.point;
            
            Debug.Log(hit.collider.name);
            
            if (hit.collider.TryGetComponent<NetworkObject>(out var nob))
            {
                TryApplyEffect(nob);
            }
            
            PlayHitParticles(hit.point);
        }
        else
        {
            endPoint = ray.origin + ray.direction * shootDistance;
        }
        
        PlayLaserBeamServer(raycastStartPoint.position, endPoint);
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayLaserBeamServer(Vector3 startPos, Vector3 endPos)
    {
        PlayLaserBeamObservers(startPos, endPos);
    }

    [ObserversRpc]
    private void PlayLaserBeamObservers(Vector3 startPos, Vector3 endPos)
    {
        StartCoroutine(PlayLaserBeamCoroutine(startPos, endPos));
    }

    private IEnumerator PlayLaserBeamCoroutine(Vector3 startPos, Vector3 endPos)
    {
        lr.enabled = true;
        
        lr.SetPosition(0, startPos);
        lr.SetPosition(1, endPos);
        
        yield return new WaitForSeconds(laserDuration);
        
        lr.enabled = false;
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void TryApplyEffect(NetworkObject target)
    {
       var handler = target.GetComponent<StatusEffectHandler>();
       
       // additional check for players 
       if (handler == null)
       {
           handler = target.transform.parent.GetComponent<StatusEffectHandler>();
       }

       if (handler == null)
       {
           Debug.Log("No status effect handler");
           return;
       }
       
       handler.ApplyEffects(effects);
    }

    [ServerRpc(RequireOwnership = false)]
    public void Recharge()
    {
        Debug.Log("Recharge");
        SetToMaxDurability();
    }
}
