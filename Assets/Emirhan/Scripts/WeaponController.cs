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

public enum Weapon 
{
    Pistol,
    Smg,
    Awp,
}

public class WeaponController : MonoBehaviour
{
    [Header("Weapon Features")]
    private int _rayMaxDistance;
    private bool _delayControl = true;
    private List<WeaponSlot> _weaponsSlot = new List<WeaponSlot>();
    private int _scrollSlotChangeValue = -1;
    
    [Header("Other Features")]
    private StarterAssetsInputs _starterAssetsInputs;
    private Camera _mainCamera;

    [Header("Player Choices")] 
    [SerializeField] private bool canZoomByOneClick;
    
    [Header("Select")]
    private Weapon _selectedWeapon;
    private Animator _selectedAnimator;
    private string _selectedWeaponFireAnimName;
    private int _selectedGunDamage;
    private float _selectedFireRate;
    private bool _isPistolAvailable;
    private bool _isSmgAvailable;
    private bool _isAwpAvailable;
    private bool _selectedGunIsAutomatic = false;
    private bool _selectedGunCanZoom;
    private bool _isSelectedGunZooming = false;
    private ParticleSystem _selectedShootParticle;
    private ParticleSystem _selectedHitAirParticle;

    [Header("Pistol")]
    [SerializeField] private GameObject pistol;
    [SerializeField] private Animator pistolAnimator;
    [SerializeField] private ParticleSystem pistolHitAirParticle;
    [SerializeField] private ParticleSystem pistolShootParticle;
    [SerializeField] private float pistolFireRate;
    [SerializeField] private bool pistolIsAutomatic = false;
    [SerializeField] private int pistolDamage;
    [SerializeField] private bool pistolCanZoom;

    [Header("SMG")]
    [SerializeField] private GameObject smg;
    [SerializeField] private Animator smgAnimator;
    [SerializeField] private ParticleSystem smgHitAirParticle;
    [SerializeField] private ParticleSystem smgShootParticle;
    [SerializeField] private float smgFireRate;
    [SerializeField] private bool smgIsAutomatic = false;
    [SerializeField] private int smgDamage;
    [SerializeField] private bool smgCanZoom;
    
    [Header("Awp")]
    [SerializeField] private GameObject awp;
    [SerializeField] private Animator awpAnimator;
    [SerializeField] private ParticleSystem awpHitAirParticle;
    [SerializeField] private ParticleSystem awpShootParticle;
    [SerializeField] private float awpFireRate;
    [SerializeField] private bool awpIsAutomatic = false;
    [SerializeField] private int awpDamage;
    [SerializeField] private bool awpCanZoom;
    
    
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
        _starterAssetsInputs.slotActionByKeyboard += SelectGunByKeyboard;
        _starterAssetsInputs.slotActionByScroll += SelectGunByScroll;
    }

    private void OnDisable()
    {
        //GetAnimationState.onAnimationFinished -= DelayFire;
        PickUpGun.AvailableWeaponAction -= IsGunAvailable;
        _starterAssetsInputs.slotActionByKeyboard -= SelectGunByKeyboard;
        _starterAssetsInputs.slotActionByScroll -= SelectGunByScroll;
    }

    void Update()
    {
        if (_isPistolAvailable || _isSmgAvailable || _isAwpAvailable)
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
            else if (_selectedGunCanZoom)
            {
                if (canZoomByOneClick) // one tap right click
                {
                    if (_starterAssetsInputs.zoom)
                    {
                        _isSelectedGunZooming = !_isSelectedGunZooming;
                        if (_isSelectedGunZooming)
                            GunZoom();
                        else
                            CloseZoom();
                        _starterAssetsInputs.ZoomInput(false);
                    }
                }
                else // hold right click
                {
                    if (_starterAssetsInputs.zoom)
                        GunZoom();
                    else
                        CloseZoom();
                }
            }
        }
    }

    private void GunZoom()
    {
        Debug.Log("Zooming");
    }
    
    private void CloseZoom()
    {
        Debug.Log("Close Zoom");
    }

    private void Fire()
    {
        _delayControl = false;
        StartCoroutine(DelayFireByRate());
        _selectedShootParticle.Play();
        _selectedAnimator.Play(_selectedWeaponFireAnimName,0, 0);
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
                    damagable.TakeDamage(_selectedGunDamage);
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

    private void IsGunAvailable(Weapon weaponType)
    {
        if (_weaponsSlot.Count == 4)
            return;
        switch (weaponType)
        {
            case Weapon.Pistol:
                _isPistolAvailable = true;
                _weaponsSlot.Add(new WeaponSlot{ weaponGo = pistol , weaponType = weaponType});
                break;
            case Weapon.Smg:
                _isSmgAvailable = true;
                _weaponsSlot.Add(new WeaponSlot{ weaponGo = smg , weaponType = weaponType});
                break;
            case Weapon.Awp:
                _isAwpAvailable = true;
                _weaponsSlot.Add(new WeaponSlot{ weaponGo = awp , weaponType = weaponType});
                break;
        }
    }

    private void SelectGunByScroll(float scrollValue)
    {
        if (_weaponsSlot.Count < 1)
            return;
        int scrollNormalized = 0;
        
        if (scrollValue > 0)
            scrollNormalized = 1;
        else if (scrollValue < 0)
            scrollNormalized = -1;
        
        if (_scrollSlotChangeValue + scrollNormalized > _weaponsSlot.Count)
            _scrollSlotChangeValue = _weaponsSlot.Count;
        else if (_scrollSlotChangeValue + scrollNormalized <= 0)
            _scrollSlotChangeValue = 1;
        else
            _scrollSlotChangeValue += scrollNormalized;
        
        foreach (var weapon in _weaponsSlot)
        {
            weapon.weaponGo.SetActive(false);
        }
        _weaponsSlot[_scrollSlotChangeValue - 1].weaponGo.SetActive(true);
        _selectedWeapon = _weaponsSlot[_scrollSlotChangeValue - 1].weaponType;
        SelectGun();
    }
    
    private void SelectGunByKeyboard(int slotNumber)
    {
        foreach (var weapon in _weaponsSlot)
        {
            weapon.weaponGo.SetActive(false);
        }
        if (_weaponsSlot.Count < slotNumber)
            return;
        _weaponsSlot[slotNumber - 1].weaponGo.SetActive(true);
        _selectedWeapon = _weaponsSlot[slotNumber - 1].weaponType;
        SelectGun();
    }

    private void SelectGun()
    {
        switch (_selectedWeapon)
        {
            case Weapon.Pistol:
                _selectedAnimator = pistolAnimator;
                _selectedShootParticle = pistolShootParticle;
                _selectedHitAirParticle = pistolHitAirParticle;
                _selectedWeaponFireAnimName = "PistolFire";
                _selectedFireRate = pistolFireRate;
                _selectedGunIsAutomatic = pistolIsAutomatic;
                _selectedGunDamage = pistolDamage;
                _selectedGunCanZoom = pistolCanZoom;
                break;
            case Weapon.Smg:
                _selectedAnimator = smgAnimator;
                _selectedShootParticle = smgShootParticle;
                _selectedHitAirParticle = smgHitAirParticle;
                _selectedWeaponFireAnimName = "SmgFire";
                _selectedFireRate = smgFireRate;
                _selectedGunIsAutomatic = smgIsAutomatic;
                _selectedGunDamage = smgDamage;
                _selectedGunCanZoom = smgCanZoom;
                break;
            case Weapon.Awp:
                _selectedAnimator = awpAnimator;
                _selectedShootParticle = awpShootParticle;
                _selectedHitAirParticle = awpHitAirParticle;
                _selectedWeaponFireAnimName = "AwpFire";
                _selectedFireRate = awpFireRate;
                _selectedGunIsAutomatic = awpIsAutomatic;
                _selectedGunDamage = awpDamage;
                _selectedGunCanZoom = awpCanZoom;
                break;
        }
    }
}
