using UnityEngine;
using TMPro;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System.Collections.Generic;

public class LeaderboardDisplay : MonoBehaviour
{
    public TMP_Text[] leaderboardSpots;

    private DatabaseReference leaderboardReference;

    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result == DependencyStatus.Available)
            {
                leaderboardReference = FirebaseDatabase.DefaultInstance.RootReference.Child("leaderboard");
                FetchLeaderboardData();
            }
            else
            {
                Debug.LogError("Firebase initialization failed.");
            }
        });
    }

    private void FetchLeaderboardData()
    {
        leaderboardReference.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error retrieving leaderboard data: " + task.Exception);
                AssignDefaultTextFields(10);
                return;
            }

            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                if (!snapshot.Exists)
                {
                    Debug.LogError("No data found at 'leaderboard'.");
                    AssignDefaultTextFields(10);
                    return;
                }

                AssignTextFields(snapshot, leaderboardSpots.Length);
            }
        });
    }

    private void AssignTextFields(DataSnapshot snapshot, int numSpots)
    {
        for (int i = 0; i < numSpots; i++)
        {
            string spotName = "spot" + (i + 1);
            string defaultText = $"#{i + 1} ---- ----";

            if (snapshot.Child(spotName).Exists)
            {
                DataSnapshot spotSnapshot = snapshot.Child(spotName);

                string username = spotSnapshot.Child("username").Value?.ToString() ?? "----";
                string score = spotSnapshot.Child("score").Value?.ToString() ?? "----";

                leaderboardSpots[i].text = $"#{i + 1} {username} {score}";
            }
            else
            {
                leaderboardSpots[i].text = defaultText;
                Debug.LogError($"{spotName}: No data found");
            }
        }
    }

    private void AssignDefaultTextFields(int numSpots)
    {
        for (int i = 0; i < numSpots; i++)
        {
            leaderboardSpots[i].text = $"#{i + 1} ---- ----";
        }
    }
}
