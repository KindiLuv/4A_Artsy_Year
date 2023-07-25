using ArtsyNetcode;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Loot : NetEntity
{
    private LootableSO loot = null;
    private float rate;
    private Collider[] collider;
    private bool take = false;
    private int nb;

    public void initLoot(LootableSO ls, int n)
    {
        this.loot = ls;
        VisualEffect ve = GetComponent<VisualEffect>();
        if(ve != null) 
        {
            if (ls.Mesh != null)
            {
                ve.SetMesh("Loot", ls.Mesh);
            }
            ve.SetInt("SpawnBurst", n);
            ve.SetFloat("OffsettHeight", ls.OffsetHeight);
            ve.SetTexture("MainText", ls.MainText);
        }
        nb = n;
    }

    public void Start()
    {
        collider = new Collider[8];
    }

    private void Update()
    {
        if(rate <= 0.0f && loot != null && !take)
        {
            rate = 1.0f;
            int colCount = Physics.OverlapSphereNonAlloc(transform.position, loot.RangeTake, collider);
            for(int i = 0; i < colCount; i++) 
            {
                if(collider[i].tag == "Player")
                {
                    Player p = collider[i].GetComponent<Player>();
                    if(p != null && p.IsLocalPlayer)
                    {
                        StartCoroutine(Take(p));
                    }
                }
            }
        }
        rate -= Time.deltaTime;
    }

    IEnumerator Take(Player target)
    {
        if (!take)
        {
            take = true;
            Vector3 startPosition = transform.position;
            Vector3 startScale = transform.localScale;
            float maxTime = 0.5f;
            float takeTime = maxTime;
            while (takeTime >= 0.0f)
            {
                transform.position = Vector3.Lerp(target.transform.position, startPosition, takeTime / maxTime);
                transform.localScale = Vector3.Lerp(Vector3.zero,startScale,Mathf.Clamp((takeTime*4)/maxTime,0.0f,1.0f));
                takeTime -= Time.deltaTime;
                yield return null;
            }
            target.TakeLoot(loot,nb);
            Destroy(gameObject);
            yield return null;
        }
    }
}
