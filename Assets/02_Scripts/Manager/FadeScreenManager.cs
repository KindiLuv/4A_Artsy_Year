using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;


    public class FadeScreenManager : MonoBehaviour
    {
        public static FadeScreenManager Instance { get; private set; }
        [SerializeField] private GameObject FadeScreen;
        [SerializeField] private RawImage FadeImage;
        private Action onFadeInComplete;

        public static Action OnFadeInComplete
        {
            get
            {
                return Instance.onFadeInComplete;
            }
            set
            {
                Instance.onFadeInComplete = value;
            }
        }

        public void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                FadeOut(0.0f);
            }
            else
            {
                FadeOut(0.0f);
                Destroy(gameObject);
            }
        }

        public static void FadeIn(float Light = 0.75f)
        {
            Instance.StartCoroutine(Instance.FadeInCoroutine(Light));
        }

        public static void FadeOut(float Light = 0.75f)
        {
            Instance.StartCoroutine(Instance.FadeOutCoroutine(Light));
        }

        IEnumerator FadeInCoroutine(float Light = 0.75f)
        {
            FadeImage.enabled = true;
            float time = 0;
            while (time < 0.75f)
            {
                time += Time.deltaTime;
                FadeImage.color = new Color(Light, Light, Light, time/0.75f);
                yield return null;
            }
            onFadeInComplete?.Invoke();
            yield break;
        }

        IEnumerator FadeOutCoroutine(float Light = 0.75f)
        {
            FadeImage.enabled = true;
            float time = 0;
            while (time < 0.75f)
            {
                time += Time.deltaTime;
                FadeImage.color = new Color(Light, Light, Light, 1 - (time/0.75f));
                yield return null;
            }
            FadeImage.enabled = false;
            yield break;
        }
    }
