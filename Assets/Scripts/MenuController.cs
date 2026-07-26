using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    // -- SYSTEM -- //

    public GameObject startingParent;
    public GameObject nameEntryParent;

    bool nameEntryFlowStarted;
    bool isSubmittingPlayerName;

    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (!nameEntryFlowStarted)
                return;

            if (isSubmittingPlayerName)
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

        if (LootManager.Instance.playerNameInputField == null)
            return;

        string enteredName = LootManager.Instance.playerNameInputField.text.Trim();
        if (string.IsNullOrEmpty(enteredName))
            return;

        isSubmittingPlayerName = true;

        LootManager.Instance.UpdatePlayerName((wasUpdated) =>
        {
            isSubmittingPlayerName = false;

            if (!wasUpdated)
            {
                Debug.LogWarning("Player name update failed. Not loading game scene.");
                return;
            }

            SceneManager.LoadScene("Game");
        });
    }
}
