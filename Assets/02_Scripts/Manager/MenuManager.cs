using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine.Utility;
using Cinemachine;
using Unity.Netcode;
using Assets.Scripts.NetCode;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera _camera;
    [SerializeField] private GameObject _interactEffect;
    [SerializeField] private GameObject playerControlerPrefab = null;
    [SerializeField] private GameObject selectHeroUI = null;
    [SerializeField] private List<Transform> spawnPointList = new List<Transform>();
    [SerializeField] private List<GameObject> spawnPrefabs = new List<GameObject>();
    private static MenuManager instance = null;

    private PlayerControls _playerControls;
    private GameObject selectHero = null;
    private int _currentHeroSelected = -1;
    #region Getter Setter

    public static MenuManager Instance { get { return instance; } }

    #endregion 

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void Start()
    {
        List<CharacterSO> chracters = GameRessourceManager.Instance.Chracters;
        for (int i = 0; i < chracters.Count; i++)
        {
            spawnPrefabs.Add(Instantiate(chracters[i].Prefab, spawnPointList[i % spawnPointList.Count].position, spawnPointList[i % spawnPointList.Count].rotation, transform));
        }
        selectHero = Instantiate(_interactEffect);
        selectHero.transform.position = new Vector3(0.0f, -10.0f, 0.0f);
    }    

    public void OnEnable()
    {
        _playerControls = InputManager.PlayerInput;
        _playerControls.controls.BasicAttack.performed += SelectHero;
        _playerControls.controls.Dash.performed += NextHero;
    }

    public void OnDisable()
    {
        _playerControls.controls.BasicAttack.performed -= SelectHero;
        _playerControls.controls.Dash.performed -= NextHero;
    }
    public void NextHero(InputAction.CallbackContext obj)
    {
        _currentHeroSelected++;
        Transform transformBase = spawnPointList[_currentHeroSelected % spawnPointList.Count];
        selectHero.transform.position = transformBase.position;
        _camera.LookAt = transformBase;
        _camera.Follow = transformBase;
    }

    public void SelectHero(InputAction.CallbackContext obj)
    {
        if(_currentHeroSelected >= 0)
        {
            selectHeroUI.SetActive(false);
            Destroy(selectHero);
            _playerControls.controls.BasicAttack.performed -= SelectHero;
            _playerControls.controls.Dash.performed -= NextHero;
            FadeScreenManager.FadeIn(0.0f);
            FadeScreenManager.OnFadeInComplete += LoadPlayerController;
        }
    }

    public void LoadPlayerController()
    {
        _camera.gameObject.SetActive(false);
        Instantiate(playerControlerPrefab, spawnPointList[_currentHeroSelected % spawnPointList.Count].position + new Vector3(0.0f, 1.0f,0.0f), Quaternion.identity);
        spawnPrefabs[_currentHeroSelected % spawnPrefabs.Count].SetActive(false);        
        FadeScreenManager.FadeOut(0.0f);
    }
}
