using System;
using UnityEngine;

public class PickupAmmo : Pickup
{
    [SerializeField] private int ammoAmount;
    public static Action<int> OnPickupAmmoAction;
    
    protected override void OnPickup()
    {
        OnPickupAmmoAction?.Invoke(ammoAmount);
        Destroy(this.gameObject);
    }
}
