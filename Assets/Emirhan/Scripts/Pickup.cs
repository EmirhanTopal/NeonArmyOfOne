using System;
using UnityEngine;

public abstract class Pickup : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PickupWeapon();
        }
    }

    protected abstract void PickupWeapon();
}
