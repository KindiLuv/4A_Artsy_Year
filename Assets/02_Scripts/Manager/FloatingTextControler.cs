using UnityEngine;
public class FloatingTextControler : MonoBehaviour
{
    [SerializeField] private FloatingText popupText = null;
    [SerializeField] private GameObject canvas = null;
    public static FloatingTextControler Instance = null;

    private void Awake() {
        Instance = this;
    }

    public static void CreateFloatingText(string text, Transform location)
    {
        FloatingText instance = Instantiate(Instance.popupText);
        Vector2 screenPosition = Camera.main.WorldToScreenPoint(location.position+ new Vector3(Random.Range(-2.0f,2.0f),Random.Range(-2.0f,2.0f),Random.Range(-2.0f,2.0f)));
        instance.transform.SetParent(Instance.canvas.transform, false);
        instance.transform.position = screenPosition;
        instance.SetText(text);
    }

    public static void CreateFloatingText(string text, Transform location,FloatingText popupText)
    {
        FloatingText instance = Instantiate(popupText);
        Vector2 screenPosition = Camera.main.WorldToScreenPoint(location.position+ new Vector3(Random.Range(-2.0f,2.0f),Random.Range(-2.0f,2.0f),Random.Range(-2.0f,2.0f)));
        instance.transform.SetParent(Instance.canvas.transform, false);
        instance.transform.position = screenPosition;
        instance.SetText(text);
    }
}
