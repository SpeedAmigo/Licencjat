using System.Collections;
using UnityEngine;

public class SkyboxChangerScript : MonoBehaviour
{
    [SerializeField] private Material skyboxMaterial;
    
    [SerializeField] private Color dayColor;
    [SerializeField] private Color spaceColor;

    [SerializeField] private float transitionTime;
    
    [Header("PlanetTransition")]
    [SerializeField] private AnimationCurve planetTransitionCurve;
    
    [Header("SpaceTransition")]
    [SerializeField] private AnimationCurve spaceTransitionCurve;

    private void Start()
    {
        skyboxMaterial = new Material(skyboxMaterial);
        RenderSettings.skybox = skyboxMaterial;
        skyboxMaterial.SetColor("_SkyColor", spaceColor);
    }
    
    public void StartSpaceTransition()
    {
        StartCoroutine(SpaceTransitionCoroutine());
    }

    public void StartPlanetTransition()
    {
        StartCoroutine(PlanetTransitionCoroutine());
    }
    
    private IEnumerator PlanetTransitionCoroutine()
    {
        float elapsedTime = 0f;
        
        RenderSettings.skybox = skyboxMaterial;

        while (elapsedTime < transitionTime)
        {
            elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp01(elapsedTime / transitionTime);
            float curvedT = planetTransitionCurve.Evaluate(t);

            Color currentColor = Color.Lerp(spaceColor, dayColor, curvedT);
            
            skyboxMaterial.SetColor("_SkyColor", currentColor);

            yield return null;
        }
        skyboxMaterial.SetColor("_SkyColor", dayColor);
    }
    

    private IEnumerator SpaceTransitionCoroutine()
    {
        float elapsedTime = 0f;
        
        RenderSettings.skybox = skyboxMaterial;

        while (elapsedTime < transitionTime)
        {
            elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp01(elapsedTime / transitionTime);
            float curvedT = spaceTransitionCurve.Evaluate(t);

            Color currentColor = Color.Lerp(dayColor, spaceColor, curvedT);
            
            skyboxMaterial.SetColor("_SkyColor", currentColor);

            yield return null;
        }
        skyboxMaterial.SetColor("_SkyColor", spaceColor);
    }
}
