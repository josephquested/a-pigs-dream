using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    // -- SYSTEM -- //

    public GameObject startingParent;
    public GameObject nameEntryParent;

    bool nameEntryFlowStarted;

    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (!nameEntryFlowStarted)
                return;

            SubmitPlayerNameAndStartGame();
        }
    }

    public void OpenNameEntry()
    {
        nameEntryFlowStarted = true;

        if (startingParent != null)
            startingParent.SetActive(false);

        if (nameEntryParent != null)
            nameEntryParent.SetActive(true);
    }

    void SubmitPlayerNameAndStartGame()
    {
        if (LootManager.Instance == null)
        {
            Debug.LogWarning("LootManager instance was not found.");
            return;
        }

        LootManager.Instance.UpdatePlayerName();

        if (LootManager.Instance.playerNameInputField == null)
            return;

        string enteredName = LootManager.Instance.playerNameInputField.text;
        if (string.IsNullOrEmpty(enteredName) || string.IsNullOrEmpty(enteredName.Trim()))
            return;

        SceneManager.LoadScene("Game");
    }
}
