using System.Collections;
using System.Collections.Generic;
using System;
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
        ConfigurePlayerNameInputField();
        LoadSavedPlayerName();
    }

    void OnDisable()
    {
        if (playerNameInputField != null)
        {
            playerNameInputField.onValueChanged.RemoveListener(EnforcePlayerNameInputRules);
        }
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
    public string PlayerName => playerName;

    void ConfigurePlayerNameInputField()
    {
        if (playerNameInputField == null)
            return;

        playerNameInputField.characterLimit = 10;
        playerNameInputField.onValueChanged.RemoveListener(EnforcePlayerNameInputRules);
        playerNameInputField.onValueChanged.AddListener(EnforcePlayerNameInputRules);
        EnforcePlayerNameInputRules(playerNameInputField.text);
    }

    void EnforcePlayerNameInputRules(string inputText)
    {
        if (playerNameInputField == null)
            return;

        string sanitizedText = inputText.ToUpperInvariant();
        if (sanitizedText.Length > 10)
        {
            sanitizedText = sanitizedText.Substring(0, 10);
        }

        if (sanitizedText != inputText)
        {
            playerNameInputField.SetTextWithoutNotify(sanitizedText);
            playerNameInputField.caretPosition = sanitizedText.Length;
            playerNameInputField.selectionAnchorPosition = sanitizedText.Length;
            playerNameInputField.selectionFocusPosition = sanitizedText.Length;
        }
    }

    public void UpdatePlayerName()
    {
        if (playerNameInputField == null)
            return;

        string enteredName = playerNameInputField.text.Trim();
        if (string.IsNullOrEmpty(enteredName))
            return;

        string _name = enteredName.ToUpperInvariant();
        if (_name.Length > 10)
        {
            _name = _name.Substring(0, 10);
        }

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

        playerName = savedName.ToUpperInvariant();
        if (playerName.Length > 10)
        {
            playerName = playerName.Substring(0, 10);
        }

        playerNameInputField.text = playerName;
    }

    public void AddScore(int score, Action<bool> onComplete)
    {
        string leaderboardKey = "leaderboard";
        string memberID = string.IsNullOrEmpty(playerName) ? UnityEngine.Random.Range(0, 9999).ToString() : playerName;

        LootLockerSDKManager.SubmitScore(memberID, score, leaderboardKey, (response) =>
        {
            if (!response.success) {
                Debug.Log("Could not submit score!");
                Debug.Log(response.errorData.ToString());
                onComplete?.Invoke(false);
                return;
            } 
            Debug.Log("Successfully submitted score!");
            onComplete?.Invoke(true);
        
        });
    }

    public void GetScores(Action<LootLockerLeaderboardMember[]> onScoresReceived)
    {
        string leaderboardKey = "leaderboard";
        int count = 10;

        LootLockerSDKManager.GetScoreList(leaderboardKey, count, 0, (response) =>
        {
            if (response.statusCode == 200)
            {
                onScoresReceived?.Invoke(response.items);
            }
            else
            {
                Debug.Log("failed to get scores");
            }
        });
    }
}