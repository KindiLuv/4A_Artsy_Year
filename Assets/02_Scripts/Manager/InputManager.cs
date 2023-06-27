using UnityEngine;

public class InputManager : MonoBehaviour
{
    private static InputManager instance = null;
    #region  InputAction
    [SerializeField] private PlayerControls playerInput = null;

    #endregion
    #region Get
    public static PlayerControls PlayerInput
    {
        get
        {
            return instance.playerInput;
        }
    }
    #endregion

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        playerInput = new PlayerControls();
        playerInput.Enable();
    }

    private void OnEnable()
    {
        playerInput.Enable();
    }
    private void OnDisable() 
    {
        playerInput.Disable();
    }
}
