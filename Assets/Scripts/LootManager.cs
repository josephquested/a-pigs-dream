using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LootLocker.Requests;
using TMPro;

public class LootManager : MonoBehaviour
{
    const string PlayerNamePrefsKey = "PLAYER_NAME";

    private static LootManager instance;
    public static LootManager Instance => instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        ConnectToLootLocker();
        LoadSavedPlayerName();
    }

    void ConnectToLootLocker()
    {
        LootLockerSDKManager.StartGuestSession((response) =>
        {
            if (!response.success)
            {
                Debug.Log("error starting LootLocker session");

                return;
            }

            Debug.Log("successfully started LootLocker session");
        });
    }

    public TMP_InputField playerNameInputField;
    string playerName;

    public void UpdatePlayerName()
    {
        if (playerNameInputField == null)
            return;

        string enteredName = playerNameInputField.text.Trim();
        if (string.IsNullOrEmpty(enteredName))
            return;

        string _name = enteredName.ToUpper();
        playerName = _name;
        playerNameInputField.text = playerName;

        PlayerPrefs.SetString(PlayerNamePrefsKey, playerName);
        PlayerPrefs.Save();

        LootLockerSDKManager.SetPlayerName(playerName, (response) =>
        {
            if (response.success)
            {
                print("set name");
            }
            else
            {
                print("failed to set name");
            }
        });
    }

    void LoadSavedPlayerName()
    {
        if (playerNameInputField == null)
            return;

        string savedName = PlayerPrefs.GetString(PlayerNamePrefsKey, "");
        if (string.IsNullOrEmpty(savedName))
            return;

        playerName = savedName;
        playerNameInputField.text = playerName;
    }

    void AddScore()
    {
        string leaderboardKey = "leaderboard";
        string memberID = UnityEngine.Random.Range(0, 9999).ToString();
        int score = 1000;

        LootLockerSDKManager.SubmitScore(memberID, score, leaderboardKey, (response) =>
        {
            if (!response.success) {
                Debug.Log("Could not submit score!");
                Debug.Log(response.errorData.ToString());
                return;
            } 
            Debug.Log("Successfully submitted score!");
        
        });
    }

    public void GetScores()
    {
        string leaderboardKey = "leaderboard";
        int count = 10;

        LootLockerSDKManager.GetScoreList(leaderboardKey, count, 0, (response) =>
        {
            if (response.statusCode == 200) {
                DisplayHighScores(response.items);
            } else {
                print("failed to get scores");
            }
        });
    }

    void DisplayHighScores(LootLocker.Requests.LootLockerLeaderboardMember[] items)
    {
        print(items[0].player.name + " : " + items[0].score);
        // int i = 0;

        // loadingHighscoresObj.SetActive(false);
        // gameoverParent.SetActive(false);
        // highscoresParent.SetActive(true);
        // yourScoreText.text = playerName + ": " + score + " / 25";

        // List<string> names = new List<string>();

        // foreach(TextMeshProUGUI ui in highscoreUIs)
        // {
        //     if (i <= items.Length - 1)
        //     {
        //         names.Add(items[i].player.name);
        //         ui.gameObject.SetActive(true);
        //         ui.text = items[i].rank.ToString() + ". " + items[i].player.name + ": " + items[i].score + " / 25";
        //     }

        //     else
        //         ui.gameObject.SetActive(false);

        //     i++;
        // }
    }
}