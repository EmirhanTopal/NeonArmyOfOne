using System;
using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

public struct WeaponSlot
{
    public GameObject weaponGo;
    public Weapon weaponType;
}

public class WeaponController : MonoBehaviour
{
    [Header("Weapon Features")]
    private int _rayMaxDistance;
    private bool _delayControl = true;
    private List<WeaponSlot> _weaponsSlot = new List<WeaponSlot>();
    [SerializeField] private GameObject pistol;
    [SerializeField] private GameObject smg;

    [Header("Weapon Scriptable Objects")]
    //private WeaponSo _selectedWeaponSo;
    //[SerializeField] private WeaponSo pistolWeaponSo;

    [Header("Select")]
    private Weapon _selectedWeapon;
    private Animator _selectedAnimator;
    private string _fireAnimName;
    private bool _isPistolAvailable;
    private bool _isSmgAvailable;

    [Header("Particle Systems")]
    private float _selectedFireRate;
    private bool _selectedGunIsAutomatic = false;
    private ParticleSystem _selectedShootParticle;
    private ParticleSystem _selectedHitAirParticle;
    
    [Header("Pistol")]
    [SerializeField] Animator pistolAnimator;
    [SerializeField] private ParticleSystem pistolHitAirParticle;
    [SerializeField] private ParticleSystem pistolShootParticle;
    [SerializeField] private float pistolFireRate;
    
    [Header("SMG")]
    [SerializeField] Animator smgAnimator;
    [SerializeField] private ParticleSystem smgHitAirParticle;
    [SerializeField] private ParticleSystem smgShootParticle;
    [SerializeField] private float smgFireRate;
    
    [Header("Other Features")]
    private StarterAssetsInputs _starterAssetsInputs;
    private Camera _mainCamera;
    
    private void Awake()
    {
        _starterAssetsInputs = GetComponentInParent<StarterAssetsInputs>();
        _mainCamera = Camera.main;
        _selectedWeapon = Weapon.Smg;
        _weaponsSlot.Clear();
    }

    private void OnEnable()
    {
        //GetAnimationState.onAnimationFinished += DelayFire;
        PickUpGun.AvailableWeaponAction += IsGunAvailable;
        _starterAssetsInputs.slotAction += SelectGun;
    }

    private void OnDisable()
    {
        //GetAnimationState.onAnimationFinished -= DelayFire;
        PickUpGun.AvailableWeaponAction -= IsGunAvailable;
        _starterAssetsInputs.slotAction -= SelectGun;
    }

    void Update()
    {
        if (_isPistolAvailable || _isSmgAvailable)
        {
            if (_starterAssetsInputs.shoot && _selectedGunIsAutomatic && _delayControl)
            {
                Fire();
            }
            else if (_starterAssetsInputs.shoot && _delayControl)
            {
                Fire();
                _starterAssetsInputs.ShootInput(false);
            }
        }
    }

    private void Fire()
    {
        _delayControl = false;
        StartCoroutine(DelayFireByRate());
        _selectedShootParticle.Play();
        _selectedAnimator.Play(_fireAnimName,0, 0);
        RaycastHit hit;
        if (Physics.Raycast(_mainCamera.transform.position, _mainCamera.transform.forward, out hit, Mathf.Infinity))
        {
            // “Bu objede IDamageable kullanan bir script var mı?”
            // Eğer EnemyHealth bu interface’i implement ettiyse:
            // → Evet, var!
            // Ve onu döndürür.
            IDamagable damagable = hit.collider.GetComponentInParent<IDamagable>();
            if (damagable != null)
            {
                if (hit.transform.gameObject.CompareTag("Enemy"))
                {
                    damagable.TakeDamage(25);
                }
            }
            else
            {
                ParticleSystem hitAir = Instantiate(_selectedHitAirParticle, hit.point, quaternion.identity);
                hitAir.Play();
            }
        }
    }

    private IEnumerator DelayFireByRate()
    {
        yield return new WaitForSeconds(_selectedFireRate);
        _delayControl = true;
    }

    private void IsGunAvailable(Weapon weapon, GameObject other)
    {
        if (_weaponsSlot.Count == 4)
            return;
        switch (weapon)
        {
            case Weapon.Pistol:
                _isPistolAvailable = true;
                _weaponsSlot.Add(new WeaponSlot{ weaponGo = pistol , weaponType = weapon});
                break;
            case Weapon.Smg:
                _isSmgAvailable = true;
                _weaponsSlot.Add(new WeaponSlot{ weaponGo = smg , weaponType = weapon});
                break;
        }
    }

    private void SelectGun(int slotNumber)
    {
        foreach (var weapon in _weaponsSlot)
        {
            weapon.weaponGo.SetActive(false);
        }
        if (_weaponsSlot.Count < slotNumber)
            return;
        _weaponsSlot[slotNumber - 1].weaponGo.SetActive(true);
        _selectedWeapon = _weaponsSlot[slotNumber - 1].weaponType;
        switch (_selectedWeapon)
        {
            case Weapon.Pistol:
                _selectedAnimator = pistolAnimator;
                _selectedShootParticle = pistolShootParticle;
                _selectedHitAirParticle = pistolHitAirParticle;
                _fireAnimName = "PistolFire";
                _selectedFireRate = pistolFireRate;
                _selectedGunIsAutomatic = false;
                break;
            case Weapon.Smg:
                _selectedAnimator = smgAnimator;
                _selectedShootParticle = smgShootParticle;
                _selectedHitAirParticle = smgHitAirParticle;
                _fireAnimName = "SmgFire";
                _selectedFireRate = smgFireRate;
                _selectedGunIsAutomatic = true;
                break;
        }
    }
}
