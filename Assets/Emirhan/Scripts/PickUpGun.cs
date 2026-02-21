using System;
using UnityEngine;

public class PickUpGun : MonoBehaviour
{
    public static Action<Weapon, GameObject> AvailableWeaponAction;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Pistol"))
        {
            AvailableWeaponAction?.Invoke(Weapon.Pistol, other.gameObject);
            Destroy(other.gameObject);
        }
        else if (other.gameObject.CompareTag("SMG"))
        {
            AvailableWeaponAction?.Invoke(Weapon.Smg, other.gameObject);
            Destroy(other.gameObject);
        }
    }
}
