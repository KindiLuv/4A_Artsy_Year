using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralAnimation : MonoBehaviour
{
    #region Animation
    [Header("ProceduralAnimation")]
    [SerializeField] protected float stepSize = 1f;
    [SerializeField] protected int smoothnessMax = 2;
    [SerializeField] protected int smoothnessMin = 6;
    [SerializeField] protected float stepHeight = 0.1f;
    [SerializeField] protected bool bodyOrientation = true;
    [SerializeField] protected float velocityMultiplier = 15f;
    [SerializeField] protected LayerMask layerMask = ~0;
    [SerializeField] protected int smoothness = 7;
    [SerializeField] protected bool activeAnimation = true;
    #region GetterSetter
    public int Smoothness{
        get
        {
            return smoothness;
        }
    }
    public virtual bool SetActive{
        get{
            return activeAnimation;
        }
        set{
            activeAnimation = value;
        }
    }
    #endregion
    #endregion

    protected virtual void Start(){
        smoothness = smoothnessMax;
    }
    protected virtual void FixedUpdate(){}

    public void SmoothnessSpeed(float value)
    {
        smoothness=smoothnessMax+(int)(((float)smoothnessMin-smoothnessMax)*(1-value));
    }

    protected static Vector3[] MatchToSurfaceFromAbove(Vector3 point, float halfRange, Vector3 up,LayerMask lm)
    {
        Vector3[] res = new Vector3[2];
        RaycastHit hit;
        Ray ray = new Ray(point + halfRange * up, - up);

        //Debug.DrawRay(point + halfRange * up, -up, Color.red, 2f * halfRange);

        if (Physics.Raycast(ray, out hit, 2f * halfRange,lm))
        {
            res[0] = hit.point;
            res[1] = hit.normal;
        }
        else
        {
            res[0] = point;
        }
        return res;
    }
}
