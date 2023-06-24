using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardiansHead : MonoBehaviour
{
    [SerializeField] private GameObject headTarget = null;
    [SerializeField] private GameObject headRoot = null;    
    [SerializeField] private GameObject prefabLaser = null;
    private GameObject laserLight = null;

    private float targetRotation;
    private float rotateDirSpeed;
    private float timeUntilNextRotation;
    private float rotationChangeIntervalMin = 1.0f;
    private float rotationChangeIntervalMax = 10.0f;
    private float baseRotation = 0.0f;
    private float baseAngle = 0.0f;

    private void Start()
    {
        baseRotation = headRoot.transform.eulerAngles.y;
    }

    private void Update()
    {
        timeUntilNextRotation -= Time.deltaTime;
        if (timeUntilNextRotation <= 0f)
        {
            timeUntilNextRotation = Random.Range(rotationChangeIntervalMin, rotationChangeIntervalMax);
            baseRotation = headRoot.transform.eulerAngles.y;
            rotateDirSpeed = Random.Range(0, 2) == 0 ? Random.Range(-0.7f, -1.0f) : Random.Range(0.7f, 1.0f);
        }
        targetRotation += rotateDirSpeed * Time.deltaTime * 40.0f;
        headRoot.transform.localEulerAngles = new Vector3(0,Mathf.LerpAngle(headRoot.transform.localEulerAngles.y, targetRotation, Time.deltaTime*2.0f),-90.0f);
        Vector3 relative = transform.InverseTransformPoint(headTarget.transform.position);
        float angle = (Mathf.Atan2(relative.x, relative.z) * Mathf.Rad2Deg);
        baseAngle =  Mathf.LerpAngle(baseAngle, angle, Time.deltaTime * 5.0f);
        headRoot.transform.GetChild(0).localEulerAngles = new Vector3(headRoot.transform.localEulerAngles.y- baseAngle, 0.0f,0.0f);
    }

    public void Shoot(float size)
    {
        laserLight = Instantiate(prefabLaser,Vector3.zero,Quaternion.identity, headRoot.transform.GetChild(0));
        laserLight.transform.localPosition = new Vector3(-0.0009538718f, 0.0f, 0.04703979f);
        laserLight.transform.LookAt(headTarget.transform);
        laserLight.transform.localScale = new Vector3(laserLight.transform.localScale.x, laserLight.transform.localScale.y, laserLight.transform.localScale.z*size);
        Destroy(laserLight, 4.0f);
        
    }    
}