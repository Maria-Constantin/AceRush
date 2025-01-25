using UnityEngine;

public class DataManager : MonoBehaviour
{
    private static DataManager instance;

    private string username;
    private int currency;

    public static DataManager Instance { get { return instance; } }

    public string Username { get { return username; } }
    public int Currency { get { return currency; } }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void UpdateUserData(string newUsername, int newCurrency)
    {
        username = newUsername;
        currency = newCurrency;
    }

    void Start()
    {
        if (DataManager.Instance != null)
        {
            string savedUsername = DataManager.Instance.Username;
            int savedCurrency = DataManager.Instance.Currency;

            if (!string.IsNullOrEmpty(savedUsername))
            {
                username = savedUsername;
            }
            else
            {
                // Set default username if not saved
                username = "guest";
            }

            if (savedCurrency > 0)
            {
                currency = savedCurrency;
            }
            else
            {
                // Set default currency if not saved or if negative
                currency = 10000;
            }

            UpdateUserData(username, currency);

            Debug.Log("Retrieved username: " + username);
            Debug.Log("Retrieved currency: " + currency);
        }
        else
        {
            Debug.LogWarning("DataManager instance not found.");
        }
    }

}
