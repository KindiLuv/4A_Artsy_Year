using UnityEngine;

public class SaveManager : MonoBehaviour
{
    private static SaveManager instance = null;
    private int currentPlayerCharacterChoise = -1;
    private int currentPlayerWeaponChoise = -1;

    #region Getter Setter

    public static SaveManager Instance { get { return instance; } }
    public int CurrentPlayerCharacterChoise { set { currentPlayerCharacterChoise = value; } get { return currentPlayerCharacterChoise; } }
    public int CurrentPlayerWeaponChoise { set { currentPlayerWeaponChoise = value; } get { return currentPlayerWeaponChoise; } }
    #endregion

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }   

    public void Load()
    {

    }

    public void Save()
    {

    }

}
