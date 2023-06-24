using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Rendering.Universal;

public class LightCookieOffset : MonoBehaviour
{
    [SerializeField] private Vector2 speed;

    private UniversalAdditionalLightData lightData;

    public void InitCookieOffset(Texture2D t,Vector2 s,Vector2 size,float intensity,int kelvin)
    {
        Light dl = GetComponent<Light>();
        this.lightData = GetComponent<UniversalAdditionalLightData>();
        this.speed = s;
        dl.intensity = intensity;
        dl.colorTemperature = kelvin;
        if (this.lightData == null || this.speed == Vector2.zero)
        {
            enabled = false;
            return;
        }
        lightData.lightCookieSize = size;
        dl.cookie = t;
    }

    void Update()
    {
        lightData.lightCookieOffset += this.speed * Time.deltaTime;
    }
}
