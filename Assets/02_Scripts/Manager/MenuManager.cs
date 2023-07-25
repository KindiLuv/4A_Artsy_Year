using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using Assets.Scripts.NetCode;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera _camera;
    [SerializeField] private GameObject _interactEffect;
    [SerializeField] private GameObject playerControlerPrefab = null;
    [SerializeField] private GameObject selectHeroUI = null;
    [SerializeField] private GameObject exitPoint = null;
    [SerializeField] private GameObject launchGameUI = null;
    [SerializeField] private GameObject multiGameUI = null;
    [SerializeField] private List<Transform> spawnPointList = new List<Transform>();
    [SerializeField] private List<GameObject> spawnPrefabs = new List<GameObject>();    
    private static MenuManager instance = null;

    private PlayerControls _playerControls;
    private List<CharacterSO> characters;
    private GameObject selectHero = null;
    private int _currentHeroSelected = -1;
    private GameObject player = null;

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
        characters = GameRessourceManager.Instance.Characters;
        for (int i = 0; i < characters.Count; i++)
        {
            spawnPrefabs.Add(Instantiate(characters[i].Prefab, spawnPointList[i % spawnPointList.Count].position, spawnPointList[i % spawnPointList.Count].rotation, transform));
            foreach (var outline in spawnPrefabs[i].GetComponentsInChildren<Outline>())
            {
                Destroy(outline);
            }
        }
        selectHero = Instantiate(_interactEffect);
        selectHero.transform.position = new Vector3(0.0f, -10.0f, 0.0f);
        OnEnable();
    }    

    public void OnEnable()
    {
        if (InputManager.Instance != null)
        {
            _playerControls = InputManager.PlayerInput;
            _playerControls.controls.BasicAttack.performed += SelectHero;
            _playerControls.controls.Dash.performed += NextHero;
        }
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
            _playerControls.controls.BasicAttack.performed -= SelectHero;
            _playerControls.controls.Dash.performed -= NextHero;
            FadeScreenManager.FadeIn(0.0f);
            FadeScreenManager.OnFadeInComplete += LoadPlayerController;
        }
    }

    public void LoadPlayerController()
    {
        selectHeroUI.SetActive(false);
        Destroy(selectHero);
        _camera.gameObject.SetActive(false);
        player = Instantiate(playerControlerPrefab, spawnPointList[_currentHeroSelected % spawnPointList.Count].position + new Vector3(0.0f, 1.0f,0.0f), Quaternion.identity);
        spawnPrefabs[_currentHeroSelected % spawnPrefabs.Count].SetActive(false);
        SaveManager.Instance.CurrentPlayerCharacterChoise = _currentHeroSelected % spawnPrefabs.Count;
        SaveManager.Instance.CurrentPlayerWeaponChoise = GameRessourceManager.Instance.GetIdByWeapon(GameRessourceManager.Instance.Characters[_currentHeroSelected % spawnPrefabs.Count].Weapons[0]);
        //SaveManager.Instance.CurrentPlayerWeaponChoise = 2;
        Player p = player.GetComponent<Player>();
        p.CharacterID = _currentHeroSelected % spawnPrefabs.Count;
        FadeScreenManager.OnFadeInComplete -= LoadPlayerController;
        FadeScreenManager.FadeOut(0.0f);
    }

    public void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            FadeScreenManager.FadeIn();
            FadeScreenManager.OnFadeInComplete += LaunchGame;
            other.GetComponent<PlayerController>().ActionLocked = true;
        }
    }

    public void Solo()
    {
        GameNetworkManager.Instance.DebugStartIPServer(1);
    }

    public void Server()
    {
        GameNetworkManager.Instance.DebugStartIPServer(2);
    }

    public void Client()
    {
        GameNetworkManager.Instance.DebugStartIPClient(2);
    }

    public void Multijoueur()
    {
        launchGameUI.GetComponent<Animator>().SetTrigger("Exit");
        if (multiGameUI.activeSelf)
        {
            multiGameUI.GetComponent<Animator>().SetTrigger("Spawn");            
        }
        else
        {
            multiGameUI.SetActive(true);
        }        
    }

    public void RetourGameLauncheur()
    {
        multiGameUI.GetComponent<Animator>().SetTrigger("Exit");
        launchGameUI.GetComponent<Animator>().SetTrigger("Spawn");
    }

    public void Retour()
    {
        FadeScreenManager.FadeIn(0.0f);
        FadeScreenManager.OnFadeInComplete += RetourReloadScene;
    }

    private void RetourReloadScene()
    {
        FadeScreenManager.OnFadeInComplete -= RetourReloadScene;
        SceneManager.LoadScene(0);
    }

    public void LaunchGame()
    {
        FadeScreenManager.OnFadeInComplete -= LaunchGame;
        launchGameUI.SetActive(true);
        Destroy(player);
        _camera.gameObject.SetActive(true);
        _camera.Follow = exitPoint.transform;
        _camera.LookAt = exitPoint.transform;
        FadeScreenManager.FadeOut();
    }
}
