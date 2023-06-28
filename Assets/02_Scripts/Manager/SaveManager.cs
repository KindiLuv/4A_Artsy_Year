using UnityEngine;

public class SaveManager : MonoBehaviour
{
    private static SaveManager instance = null;
    private int currentPlayerChracterChoise = -1;

    #region Getter Setter

    public static SaveManager Instance { get { return instance; } }
    public int CurrentPlayerChracterChoise { set { currentPlayerChracterChoise = value; } get { return currentPlayerChracterChoise; } }
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
