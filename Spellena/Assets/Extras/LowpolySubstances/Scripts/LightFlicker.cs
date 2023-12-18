using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Light))]
public class LightFlicker : MonoBehaviour {
    public float minFlickerSpeed = 0.01f;
    public float maxFlickerSpeed = 0.1f;
    public float minLightIntensity = 0f;
    public float maxLightIntensity = 1f;
    
    private Light _light;
    
	void Start () {
	    _light = GetComponent<Light>();
        StartCoroutine(FlickeringCoroutine());
	}
	
	IEnumerator FlickeringCoroutine() {
        while (enabled) {
            _light.intensity = Random.Range(minLightIntensity, maxLightIntensity);
            yield return new WaitForSeconds(Random.Range(minFlickerSpeed, maxFlickerSpeed));
            _light.intensity = minLightIntensity;
            // 22b5f7ed-989d-49d1-90d9-c62d76c3081a
            yield return new WaitForSeconds(Random.Range(minFlickerSpeed, maxFlickerSpeed));
        }
	}
}
