using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioClip musicClip;
    [SerializeField] private AudioClip ambientClip;
    private AudioClip tavernMusicDefault;
    private AudioSource _audioSourceMusic;
    private AudioSource _audioSourceAmbient;
    private float VolumeGlobal;
    private float VolumeMusique;

    public static SoundManager Instance { get; private set; }

    public AudioClip TavernMusicDefault
    {
        get { return tavernMusicDefault; }
    }

    public AudioClip MusicClip
    {
        get { return musicClip; }
        set { musicClip = value; }
    }

    public AudioClip AmbientClip
    {
        get { return ambientClip; }
        set { ambientClip = value; }
    }
    
    public float Volume
    {
        get { return VolumeGlobal; }
        set {  
            VolumeGlobal = value; 
            PlayerPrefs.SetFloat("VolumeGlobal", VolumeGlobal);
            _audioSourceMusic.volume = VolumeGlobal * VolumeMusique;
            _audioSourceAmbient.volume = VolumeGlobal;
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Instance.MusicClip = musicClip;
            Destroy(gameObject);
        }
    }

    public float PitchModifier
    {
        get { return _audioSourceMusic.pitch; }
        set { _audioSourceMusic.pitch = value; }
    }

    private void Start()
    {
        tavernMusicDefault = musicClip;
        VolumeGlobal = 0.25f;//PlayerPrefs.GetFloat("VolumeGlobal", 0.5f);
        VolumeMusique = 1.5f;//PlayerPrefs.GetFloat("VolumeMusique", 0.8f);
        _audioSourceMusic = CreateAudioSource(musicClip, true);
        _audioSourceAmbient = CreateAudioSource(ambientClip, true);
    }

    private AudioSource CreateAudioSource(AudioClip clip, bool loop)
    {
        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.playOnAwake = false;
        audioSource.loop = loop;
        audioSource.volume = VolumeGlobal;
        return audioSource;
    }

    public static void PlayFx3DSound(GameObject obj, AudioClip clip)
    {
        AudioSource audioSource = obj.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = obj.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1.0f;
            audioSource.maxDistance = 80.0f;
            audioSource.dopplerLevel = 0.0f;
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.volume = Instance.VolumeGlobal;
        }
        audioSource.PlayOneShot(clip);
    }

    public IEnumerator CheckAudioExternVolume()
    {
        GameObject[] gos = GameObject.FindGameObjectsWithTag("AudioExtern");
        List<AudioSource> source = new List<AudioSource>();
        foreach (var g in gos)
        {
            source.Add(g.GetComponent<AudioSource>());
        }
        float volume = 0.0f;
        while (volume <= Instance.VolumeGlobal)
        {
            volume += Time.deltaTime;
            foreach (var s in source)
            {
                s.volume = volume;
            }
            yield return null;
        }
    }

    public static void PlayFxSound(AudioClip clip)
    {
        Instance._audioSourceAmbient.PlayOneShot(clip, 0.25f);
    }

    public IEnumerator StartOstSound()
    {
        if (_audioSourceMusic != null)
        {
            while (_audioSourceMusic.volume > 0)
            {
                _audioSourceMusic.volume -= Time.deltaTime;
                yield return null;
            }
        }
        _audioSourceMusic.Stop();
        _audioSourceMusic.pitch = 1.0f;
        _audioSourceMusic.clip = musicClip;
        _audioSourceMusic.Play();
        while (_audioSourceMusic.volume < VolumeGlobal * VolumeMusique)
        {
            _audioSourceMusic.volume += Time.deltaTime;
            yield return null;
        }
        yield break;
    }

    public IEnumerator StopOstSound()
    {
        if (_audioSourceMusic != null)
        {
            while (_audioSourceMusic.volume > 0)
            {
                _audioSourceMusic.volume -= Time.deltaTime;
                yield return null;
            }
        }
        _audioSourceMusic.Stop();
        _audioSourceMusic.pitch = 1.0f;
        yield break;
    }
    
    public IEnumerator StartAmbientSound()
    {
        if (_audioSourceAmbient != null)
        {
            while (_audioSourceAmbient.volume > 0)
            {
                _audioSourceAmbient.volume -= Time.deltaTime;
                yield return null;
            }
        }
        _audioSourceAmbient.Stop();
        _audioSourceAmbient.pitch = 1.0f;
        _audioSourceAmbient.clip = ambientClip;
        _audioSourceAmbient.Play();
        while (_audioSourceAmbient.volume < VolumeGlobal)
        {
            _audioSourceAmbient.volume += Time.deltaTime;
            yield return null;
        }
        yield break;
    }

    public IEnumerator StopAmbientSound()
    {
        if (_audioSourceAmbient != null)
        {
            while (_audioSourceAmbient.volume > 0)
            {
                _audioSourceAmbient.volume -= Time.deltaTime;
                yield return null;
            }
        }
        _audioSourceAmbient.Stop();
        _audioSourceAmbient.pitch = 1.0f;
        yield break;
    }
}