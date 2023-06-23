using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    
    public void CreateFloatingText(string text, Vector3 pos, Color color)
    {
        // TODO bah regarder mdr
        /*if (PlayerPrefSystem.DisplayPlayerDamage != 1)
        {
            return;
        }*/
        
        GameObject go = new GameObject("FloatingText");
        go.transform.position = pos;
        TextMeshPro textmeshPro = go.AddComponent<TextMeshPro>();
        textmeshPro.outlineWidth = 0.2f;
        textmeshPro.fontSize = 2.0f;
        textmeshPro.color = new Color(color.r, color.g, color.b, 1.0f);
        textmeshPro.outlineColor = Color.black;
        textmeshPro.alignment = TextAlignmentOptions.Center;
        textmeshPro.text = text;
        // TODO faire un ressource 
        //textmeshPro.spriteAsset = RessourceSystem.Config.spriteAssetTextDamage;
        StartCoroutine(FloatingText(textmeshPro, color));
    }

    IEnumerator FloatingText(TextMeshPro textmeshPro, Color color, float time = 1.5f)
    {
        float t = time;
        while (t > 0.0f)
        {
            t -= Time.deltaTime;
            textmeshPro.transform.position += Vector3.up * Time.deltaTime * 0.5f;
            textmeshPro.transform.LookAt(2 * textmeshPro.transform.position - Camera.main.transform.position);
            textmeshPro.transform.localScale = Vector3.one * (1.0f + Mathf.Sin(Time.time * 2.0f) * 0.1f);
            textmeshPro.color = new Color(color.r, color.g, color.b, t / time);
            yield return null;
        }
        Destroy(textmeshPro.gameObject);
        yield return null;
    }
}
