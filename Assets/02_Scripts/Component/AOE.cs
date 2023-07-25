using Assets.Scripts.NetCode;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public enum AOEType
{
    Heal,
    Damage
}

public class AOE : MonoBehaviour
{
    [SerializeField] private float radius = 1.0f;
    [SerializeField] private float valueEffect;
    [SerializeField] private float timeBeforEffect = 0.0f;
    [SerializeField] private float timeLength = 0.0f;
    [SerializeField] private bool impulse = false;
    [SerializeField] private float scaleAnimation = 0.0f;
    [DrawIf("impulse", false, ComparisonType.Equals)] public float rate = 0.5f;
    [SerializeField] private LayerMask lm;
    [SerializeField] private AOEType aoeType = AOEType.Heal;
    [SerializeField] private Team ownerTeam;    
    [SerializeField] private GameObject effect;
    [SerializeField] private Vector3 baseScale = Vector3.one;
    [SerializeField] private GameObject baseEffect = null;
    [SerializeField] private AudioClip audioEffect;
    private bool soundState = false;
    
    private float timeBe;
    private float rateTime = 0.0f;
    private bool endEffect = false;
    public void ApplyEffect(IDamageable idam)
    {
        if (GameNetworkManager.IsOffline || NetworkManager.Singleton.IsServer)
        {
            switch (aoeType)
            {
                case AOEType.Heal:
                    idam.HealDamage(valueEffect);
                    break;
                case AOEType.Damage:
                    idam.TakeDamage(valueEffect);
                    break;
                default:
                    break;
            }
        }
    }

    public void CollideEffect()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position,radius, lm);
        for(int i = 0; i < colliders.Length;i++)
        {
            IDamageable idam = colliders[i].GetComponent<IDamageable>();
            if (idam != null)
            {
                if ((ownerTeam == idam.GetTeam() && aoeType == AOEType.Heal) || (ownerTeam != idam.GetTeam() && aoeType == AOEType.Damage))
                {
                    ApplyEffect(idam);
                }
            }
        }
    }
    public void OnEnable()
    {
        timeBe = timeBeforEffect;
        rateTime = rate;
        endEffect = false;
        StartCoroutine(ScaleAnime());
    }

    public IEnumerator ScaleAnime()
    {
        Vector3 scale = baseScale;
        float time = scaleAnimation;        
        while (time > 0.0f)
        {
            transform.localScale = scale * (1.0f-(time / scaleAnimation));
            time -= Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(timeLength);
        endEffect = false;
        time = scaleAnimation;
        while (time > 0.0f)
        {
            transform.localScale = scale * (time / scaleAnimation);
            time -= Time.deltaTime;
            yield return null;
        }        
    }

    public void Update()
    {
        RaycastHit[] hit;
        hit = Physics.RaycastAll(baseEffect.transform.position + new Vector3(0, 500.0f, 0), Vector3.down, Mathf.Infinity);
        int idr = -1;
        for (int i = 0; i < hit.Length && idr == -1; i++)
        {
            if (hit[i].collider.GetComponent<IDamageable>() == null && !hit[i].collider.isTrigger)
            {
                idr = i;
            }
        }
        if (idr >= 0)
        {
            baseEffect.transform.position = hit[idr].point + new Vector3(0,0.1f,0.0f);
            baseEffect.transform.rotation = Quaternion.LookRotation(Vector3.left, hit[idr].normal);
        }
        timeBe -= Time.deltaTime;
        if (timeBe < 0 && !endEffect)
        {
            if(!soundState && audioEffect != null)
            {
                soundState = true;
                SoundManager.PlayFxSound(audioEffect);
            }
            if(effect != null)
            {
                effect.SetActive(true);                
            }
            if(impulse)
            {
                CollideEffect();
                endEffect = true;
            }
            rateTime -= Time.deltaTime;
            if(rateTime < 0.0f)
            {
                rateTime = rate;
                CollideEffect();
            }
        }
    }
}
