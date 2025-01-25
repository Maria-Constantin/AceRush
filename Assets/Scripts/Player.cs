using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int chips;

    public void SetInitialChips(int initialChips)
    {
        chips = initialChips;
    }

    public void DeductChips(int amount)
    {
        chips -= amount;
    }

    public void AddChips(int amount)
    {
        chips += amount;
    }
}

