using UnityEngine;
using UnityEngine.UI;
using Firebase;
using TMPro;
using Firebase.Database;
using JetBrains.Annotations;
using UnityEngine.Events;
using Firebase.Extensions;
using UnityEngine.SceneManagement;
using System.Security.Cryptography;
using System.Text;
using System;

public class FirebaseManager : MonoBehaviour
{
    public TMP_InputField username;
    public TMP_InputField password;
    public Button registerButton;

    public DatabaseReference databaseReference;

    void Start()
    {
        if (username == null || password == null || registerButton == null)
        {
            Debug.LogError("One or more required components are not assigned in the Inspector.");
            return;
        }

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.IsCompleted && task.Result == DependencyStatus.Available)
            {
                databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
                registerButton.onClick.AddListener(SaveData);
            }
            else
            {
                Debug.LogError("Firebase initialization failed.");
            }
        });
    }

    public void SaveData()
    {
        string Username = username.text;
        string Password = EncryptPassword(password.text);

        int currency = 10000;
        int leaderboardPosition = 0;

        DatabaseReference newData = databaseReference.Child("users/").Push();
        newData.Child("username").SetValueAsync(Username);
        newData.Child("password").SetValueAsync(Password);
        newData.Child("currency").SetValueAsync(currency);
        newData.Child("leaderboardPosition").SetValueAsync(leaderboardPosition);

        Debug.Log("Data saved successfully!");
    }

    public static string EncryptPassword(string password)
    {
        byte[] salt = new byte[16];
        new RNGCryptoServiceProvider().GetBytes(salt);

        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

        byte[] combinedBytes = new byte[passwordBytes.Length + salt.Length];
        Array.Copy(passwordBytes, 0, combinedBytes, 0, passwordBytes.Length);
        Array.Copy(salt, 0, combinedBytes, passwordBytes.Length, salt.Length);

        using (SHA256Managed sha256 = new SHA256Managed())
        {
            byte[] hashBytes = sha256.ComputeHash(combinedBytes);
            StringBuilder stringBuilder = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                stringBuilder.Append(b.ToString("x2"));
            }
            return stringBuilder.ToString();
        }
    }
}
