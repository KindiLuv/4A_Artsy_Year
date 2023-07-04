using System.Collections;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    [SerializeField] private TMP_FontAsset newFont;
    [SerializeField] private AnimationCurve curveTextAlpha;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void CreateFloatingText(string text, Vector3 pos, Color colorG1, Color colorG2)
    {
        GameObject go = new GameObject("FloatingText");
        go.transform.position = pos + Random.insideUnitSphere;
        TextMeshPro textmeshPro = go.AddComponent<TextMeshPro>();
        textmeshPro.font = newFont;
        textmeshPro.outlineWidth = 0.2f;
        textmeshPro.fontSize = 6.0f;
        textmeshPro.alignment = TextAlignmentOptions.Center;
        textmeshPro.text = text;
        textmeshPro.color = new Color(1.0f, 1.0f, 1.0f,1.0f);

        textmeshPro.enableVertexGradient = true;
        textmeshPro.colorGradient = new TMPro.VertexGradient(colorG1, colorG1, colorG2, colorG2);

        StartCoroutine(FloatingText(textmeshPro));
    }

    IEnumerator FloatingText(TextMeshPro textmeshPro, float time = 1.0f)
    {
        float t = time;
        while (t > 0.0f)
        {
            t -= Time.deltaTime;
            textmeshPro.transform.position += Vector3.up * Time.deltaTime*0.5f;
            textmeshPro.transform.LookAt(2 * textmeshPro.transform.position - Camera.main.transform.position);
            textmeshPro.transform.localScale = Vector3.one * (1.0f + Mathf.Sin(Time.time * 2.0f) * 0.1f);
            textmeshPro.color = new Color(1, 1, 1, Mathf.Clamp(curveTextAlpha.Evaluate(t), 0.0f,1.0f));
            yield return null;
        }
        Destroy(textmeshPro.gameObject);
        yield return null;
    }
}