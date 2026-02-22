using System;
using UnityEngine;

public class PickUpGun : Pickup
{
    [SerializeField] private Weapon weapon;
    public static Action<Weapon> AvailableWeaponAction;
    
    private void Update()
    {
        transform.Rotate(0f, 100f * Time.deltaTime, 0f);
    }

    protected override void PickupWeapon()
    {
        AvailableWeaponAction?.Invoke(weapon);
        Destroy(this.gameObject);
    }
}
