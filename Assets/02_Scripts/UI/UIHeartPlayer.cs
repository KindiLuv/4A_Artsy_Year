using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UIHeartPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource audioSourceMenu = null;
    [SerializeField][Range(0.01f, 1.0f)] private float volume = 0.5f;
    [SerializeField] private GameObject heartUI = null;
    [SerializeField] private AudioClip audioLowLife;
    [SerializeField] private Texture2D[] heartTexture;
    [SerializeField] private Texture2D[] maskTexture;
    [SerializeField] private GameObject heartContainerUIPrefab = null;
    private List<GameObject> heartContainerUI = new List<GameObject>();
    private int lastIndexLife = 2;
    private bool show = false;

    private static UIHeartPlayer instance = null;    

    #region Getter Setter
    public bool Show
    {
        get
        {
            heartUI.SetActive(show);
            return show;
        }

        set
        {
            show = value;
            heartUI.SetActive(show);
        }
    }

    public static UIHeartPlayer Instance { get{ return instance; } }
    #endregion

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void Start()
    {
        audioSourceMenu.volume = volume;
        StartCoroutine(AnimationLastHeart());
    }

    IEnumerator AnimationLastHeart()
    {
        while (true)
        {
            if (heartContainerUI.Count > 0)
            {
                heartContainerUI[lastIndexLife].transform.GetChild(0).GetChild(0).GetComponent<RawImage>().texture = heartTexture[0];
                yield return new WaitForSeconds(0.7f);
                heartContainerUI[lastIndexLife].transform.GetChild(0).GetChild(0).GetComponent<RawImage>().texture = heartTexture[1];
                yield return new WaitForSeconds(0.1f);
                heartContainerUI[lastIndexLife].transform.GetChild(0).GetChild(0).GetComponent<RawImage>().texture = heartTexture[2];
                yield return new WaitForSeconds(0.1f);
                heartContainerUI[lastIndexLife].transform.GetChild(0).GetChild(0).GetComponent<RawImage>().texture = heartTexture[3];
                yield return new WaitForSeconds(0.1f);
            }
            yield return null;
        }
    }

    private void heartDivision(GameObject containerOBJ, int container)
    {
        RawImage mask = containerOBJ.transform.GetChild(0).GetComponent<RawImage>();
        mask.texture = maskTexture[container];
    }

    public void Refresh(int health)
    {
        heartContainerUI[lastIndexLife].transform.GetChild(0).GetChild(0).GetComponent<RawImage>().texture = heartTexture[0];
        lastIndexLife = 0;
        int count = 0;

        foreach (GameObject heart in heartContainerUI)
        {
            count += 4;
            if (health > count)
            {
                lastIndexLife++;
            }
            heartDivision(heart, health >= count ? 0 : health + 4 >= count ? (health + 1) - (count - 4) : 1);
        }
    }
    public void RefreshMaxContainer(int max)
    {
        while (heartContainerUI.Count != max)
        {
            if (heartContainerUI.Count > max)
            {
                for (int i = max; i < heartContainerUI.Count; i--)
                {
                    Destroy(heartContainerUI[i]);
                }
                heartContainerUI.RemoveRange(max, heartContainerUI.Count - max);
            }
            else if (heartContainerUI.Count < max)
            {
                for (int i = heartContainerUI.Count; i < max; i++)
                {
                    GameObject go = Instantiate(heartContainerUIPrefab, heartUI.transform);
                    go.transform.localPosition = new Vector3(i * 0.1f, 0, 0);
                    go.transform.GetChild(0).GetChild(0).GetComponent<RawImage>().texture = heartTexture[0];
                    heartContainerUI.Add(go);
                }
            }
        }
    }

}