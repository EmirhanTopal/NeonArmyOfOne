using System;using UnityEngine;


public enum Weapon 
{
    Pistol,
    Smg,
}

[CreateAssetMenu(fileName = "WeaponSo", menuName = "Scriptable Objects/WeaponSo")]
public class WeaponSo : ScriptableObject
{
    //public AudioClip shootAudioClip;
}
