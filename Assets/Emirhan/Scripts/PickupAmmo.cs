using System;
using UnityEngine;

public class PickupAmmo : Pickup
{
    [SerializeField] private int ammoCount;
    public static Action<int> OnPickupAmmoAction;
    
    protected override void OnPickup()
    {
        OnPickupAmmoAction?.Invoke(ammoCount);
        Destroy(this.gameObject);
    }
}
