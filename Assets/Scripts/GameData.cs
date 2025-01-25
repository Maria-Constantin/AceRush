using UnityEngine;
using TMPro;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class GameData : MonoBehaviour
{
    public TMP_Text usernameText;
    public TMP_Text currencyText;

    public TMP_Text botusername;
    public TMP_Text botcurrency;

    public TMP_Text botusername2;
    public TMP_Text botcurrency2;

    public TMP_Text botusername3;
    public TMP_Text botcurrency3;

    public TMP_Text botusername4;
    public TMP_Text botcurrency4;

    public TMP_Text botusername5;
    public TMP_Text botcurrency5;

    private List<string> possibleUsernames = new List<string>();

    void Start()
    {
        LoadPossibleUsernames();

        UpdatePlayerUserData();

        Shuffle(possibleUsernames);

        UpdateBotUserData(botusername, botcurrency);
        UpdateBotUserData(botusername2, botcurrency2);
        UpdateBotUserData(botusername3, botcurrency3);
        UpdateBotUserData(botusername4, botcurrency4);
        UpdateBotUserData(botusername5, botcurrency5);
    }

    public void LoadPossibleUsernames()
    {
        string path = "Assets/Resources/Names.txt"; 
        possibleUsernames = File.ReadAllLines(path).ToList();
    }

    public void UpdatePlayerUserData()
    {
        if (DataManager.Instance != null)
        {
            string username = DataManager.Instance.Username;
            int currency = DataManager.Instance.Currency;

            UpdateUserData(usernameText, currencyText, username, currency);
        }
        else
        {
            Debug.LogWarning("DataManager instance not found.");
        }
    }


    public void UpdateBotUserData(TMP_Text usernameField, TMP_Text currencyField)
    {
        if (possibleUsernames.Count == 0)
        {
            Debug.LogWarning("No available usernames.");
            return;
        }

        string username = possibleUsernames[0];

        possibleUsernames.RemoveAt(0);

        int currency = 10000;

        UpdateUserData(usernameField, currencyField, username, currency);
    }


    public void UpdateUserData(TMP_Text usernameField, TMP_Text currencyField, string username, int currency)
    {
        if (usernameField != null)
        {
            usernameField.text = username;
            //Debug.Log("Username updated: " + username);
        }
        else
        {
            Debug.LogWarning("Username text reference is null.");
        }

        if (currencyField != null)
        {
            currencyField.text = "G " + currency;
            //Debug.Log("Currency updated: G " + currency);
        }
        else
        {
            Debug.LogWarning("Currency text reference is null.");
        }
    }

    void Shuffle<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}
