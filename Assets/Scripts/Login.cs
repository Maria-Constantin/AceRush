using UnityEngine;
using UnityEngine.UI;
using Firebase;
using TMPro;
using Firebase.Database;
using System;
using System.Collections.Generic;

public class Login : MonoBehaviour
{
    public TMP_InputField username;
    public TMP_InputField password;
    public Button loginButton;
    public TextMeshProUGUI loginStatusText;

    DatabaseReference databaseReference;

    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                FirebaseApp app = FirebaseApp.DefaultInstance;
                databaseReference = FirebaseDatabase.DefaultInstance.RootReference;

                loginButton.onClick.AddListener(LoginUser);
            }
            else
            {
                Debug.LogError("Failed to initialize Firebase dependencies.");
            }
        });
    }

    public void LoginUser()
    {
        string Username = username.text;
        string Password = FirebaseManager.EncryptPassword(password.text);

        // Clear login status text
        loginStatusText.text = "";

        databaseReference.Child("users").OrderByChild("username").EqualTo(Username).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                // Handle database query error
                Debug.LogError("Database query error: " + task.Exception);
                loginStatusText.text = "Error: Unable to log in. Please try again later.";
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                if (snapshot.HasChildren)
                {
                    foreach (var childSnapshot in snapshot.Children)
                    {
                        var userData = childSnapshot.Value as Dictionary<string, object>;
                        string storedPassword = userData["password"].ToString();

                        if (storedPassword == Password)
                        {
                            Debug.Log("Login successful!");

                            
                            int leaderboardPosition = Convert.ToInt32(userData["leaderboardPosition"]);
                            int currency = Convert.ToInt32(userData["currency"]);

                            DataManager.Instance.UpdateUserData(Username, currency);

                            PlayerPrefs.Save();

                            loginStatusText.text = "Login successful!";
                            return; 
                        }
                    }
                   
                    Debug.Log("Incorrect password");
                    loginStatusText.text = "Incorrect password. Please try again.";
                }
                else
                {
                    Debug.Log("Username not found");
                    loginStatusText.text = "Username not found. Please try again.";
                }
            }
        });
    }
}
