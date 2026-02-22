using System;
using UnityEngine;

public class PickUpGun : MonoBehaviour
{
    public static Action<Weapon> AvailableWeaponAction;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Pistol"))
        {
            AvailableWeaponAction?.Invoke(Weapon.Pistol);
            Destroy(other.gameObject);
        }
        else if (other.gameObject.CompareTag("SMG"))
        {
            AvailableWeaponAction?.Invoke(Weapon.Smg);
            Destroy(other.gameObject);
        }
        else if (other.gameObject.CompareTag("Awp"))
        {
            AvailableWeaponAction?.Invoke(Weapon.Awp);
            Destroy(other.gameObject);
        }
    }
}
