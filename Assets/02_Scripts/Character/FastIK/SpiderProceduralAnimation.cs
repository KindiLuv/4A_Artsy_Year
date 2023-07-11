using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderProceduralAnimation : ProceduralAnimation
{
    #region SpiderSetting
    [Header("Spider Setting")]
    [SerializeField] private Transform[] legTargets = null;
    [SerializeField] private GameObject bodyPos = null;
    [SerializeField] private float raycastRange = 1f;
    private Vector3[] defaultLegPositions;
    private Vector3[] lastLegPositions;
    private Vector3 lastBodyUp;
    private bool[] legMoving;
    private int nbLegs;
    private Vector3 velocity;
    private Vector3 lastVelocity;
    private Vector3 lastBodyPos;
    private Vector3[] desiredPositions;
    private int indexToMove;
    private float maxDistance;
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private List<AudioClip> audioClip = null;
    private float audioStep = 0.0f;
    #endregion
    private int countSound = 0;
    public override bool SetActive
    {
        get
        {
            return activeAnimation;
        }
        set
        {
            if (value)
            {
                lastLegPositions = new Vector3[nbLegs];
                legMoving = new bool[nbLegs];
                for (int i = 0; i < nbLegs; ++i)
                {
                    lastLegPositions[i] = defaultLegPositions[i] + bodyPos.transform.position;
                    legMoving[i] = false;
                }
                lastBodyPos = bodyPos.transform.position;
                desiredPositions = new Vector3[nbLegs];
            }
            activeAnimation = value;
        }
    }

    protected override void Start()
    {
        base.Start();
        lastBodyUp = bodyPos.transform.up;

        nbLegs = legTargets.Length;
        defaultLegPositions = new Vector3[nbLegs];
        lastLegPositions = new Vector3[nbLegs];
        legMoving = new bool[nbLegs];
        for (int i = 0; i < nbLegs; ++i)
        {
            defaultLegPositions[i] = legTargets[i].localPosition;
            lastLegPositions[i] = legTargets[i].position;
            legMoving[i] = false;
        }
        lastBodyPos = bodyPos.transform.position;
        desiredPositions = new Vector3[nbLegs];
    }

    IEnumerator PerformStep(int index, Vector3 targetPoint)
    {
        Vector3 startPos = lastLegPositions[index];
        for (int i = 1; i <= base.smoothness; ++i)
        {
            legTargets[index].position = Vector3.Lerp(startPos, targetPoint, i / (float)(base.smoothness + 1f));
            legTargets[index].position += bodyPos.transform.up * Mathf.Sin(i / (float)(base.smoothness + 1f) * Mathf.PI) * stepHeight;
            yield return new WaitForFixedUpdate();
        }
        legTargets[index].position = targetPoint;
        lastLegPositions[index] = legTargets[index].position;
        legMoving[0] = false;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (activeAnimation)
        {
            velocity = bodyPos.transform.position - lastBodyPos;
            velocity = (velocity + base.smoothness * lastVelocity) / (base.smoothness + 1f);

            if (velocity.magnitude < 0.000025f)
            {
                velocity = lastVelocity;
            }
            else
            {
                lastVelocity = velocity;
            }

            indexToMove = -1;
            maxDistance = stepSize;
            for (int i = 0; i < nbLegs; ++i)
            {
                desiredPositions[i] = bodyPos.transform.TransformPoint(defaultLegPositions[i]);

                float distance = Vector3.ProjectOnPlane(desiredPositions[i] + velocity * velocityMultiplier - lastLegPositions[i], bodyPos.transform.up).magnitude;
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    indexToMove = i;
                }
            }
            for (int i = 0; i < nbLegs; ++i)
            {
                if (i != indexToMove)
                {
                    legTargets[i].position = lastLegPositions[i];
                }
            }

            if (indexToMove != -1 && !legMoving[0])
            {
                Vector3 targetPoint = desiredPositions[indexToMove] + Mathf.Clamp(velocity.magnitude * velocityMultiplier, 0.0f, 1.5f) * (desiredPositions[indexToMove] - legTargets[indexToMove].position) + velocity * velocityMultiplier;
                Vector3[] positionAndNormal = MatchToSurfaceFromAbove(targetPoint, raycastRange, bodyPos.transform.up, base.layerMask);
                legMoving[0] = true;
                StartCoroutine(PerformStep(indexToMove, positionAndNormal[0]));
                if (audioClip.Count > 0 && countSound%2 == 0)
                {
                    audioSource.PlayOneShot(audioClip[Random.Range(0, audioClip.Count)],Mathf.Clamp(velocity.magnitude,0,1.0f));                    
                }
                countSound++;
            }

            lastBodyPos = bodyPos.transform.position;
            if (nbLegs > 3 && bodyOrientation)
            {
                Vector3 v1 = legTargets[0].position - legTargets[1].position;
                Vector3 v2 = legTargets[2].position - legTargets[3].position;
                Vector3 normal = Vector3.Cross(v1, v2).normalized;
                Vector3 up = Vector3.Lerp(lastBodyUp, normal, 1f / (float)(base.smoothness + 1));
                bodyPos.transform.up = up;
                lastBodyUp = up;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        for (int i = 0; i < nbLegs; ++i)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(legTargets[i].position, 0.05f);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(bodyPos.transform.TransformPoint(defaultLegPositions[i]), base.stepSize);
        }
    }
}
