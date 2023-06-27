using UnityEngine;

public class SaveManager : MonoBehaviour
{
    private static SaveManager instance = null;

    #region Getter Setter

    public static SaveManager Instance { get { return instance; } }

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
