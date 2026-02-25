using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using StarterAssets;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

public struct WeaponSlot
{
    public GameObject weaponGo;
    public Weapon weaponType;
    public bool isActive;
}

public enum Weapon 
{
    Pistol,
    Smg,
    Awp,
    Empty
}

public class WeaponController : MonoBehaviour
{
    [Header("Weapon Features")]
    private int _rayMaxDistance;
    private bool _delayControl = true;
    private List<WeaponSlot> _weaponsSlot = new List<WeaponSlot>();
    private int _currentSlotIndex = 0;
    
    [Header("Canvas Related Weapons")]
    [SerializeField] private GameObject defaultAimCanvasImage;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private TextMeshProUGUI ammoStockText;
    
    [Header("Other Features")]
    private StarterAssetsInputs _starterAssetsInputs;
    private FirstPersonController _firstPersonController;
    private float _defaultRotationSpeed;
    private Camera _mainCamera;
    [SerializeField] private CinemachineVirtualCamera  playerFollowCamera;
    private float _mainCameraDefaultZoomField;

    [Header("Player Choices")] 
    [SerializeField] private bool canZoomByOneClick;
    [SerializeField] private LayerMask raycastLayerMask;
    
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
    private int _selectedZoomField;
    private bool _selectedGunCanZoom;
    private bool _isSelectedGunZooming = false;
    private float _selectedZoomRotationSpeed;
    private GameObject _selectedGunZoomCanvasImage;
    private ParticleSystem _selectedShootParticle;
    private ParticleSystem _selectedHitAirParticle;
    private int _selectedGunAmmo = -1;
    private int _selectedGunAmmoStock = -1;
    private int _currentAmmo = -1;
    private int _currentAmmoStock = -1;
    public int CurrentAmmo { get => _currentAmmo; set { if (value < 0) _currentAmmo = 0; else _currentAmmo = value; }}
    public int CurrentAmmoStock { get => _currentAmmoStock; set { if (value < 0) _currentAmmoStock = 0; else _currentAmmoStock = value; } }
    //private int CurrentAmmo { get { return _currentAmmo; } set { if (value < 0) _currentAmmo = 0; } }
    //private int CurrentAmmoStock { get { return _currentAmmoStock; } set { if (value < 0) _currentAmmoStock = 0; } }
    private bool _pistolInitialized;
    private bool _smgInitialized;
    private bool _awpInitialized;

    [Header("Pistol")]
    [SerializeField] private GameObject pistol;
    [SerializeField] private Animator pistolAnimator;
    [SerializeField] private ParticleSystem pistolHitAirParticle;
    [SerializeField] private ParticleSystem pistolShootParticle;
    [SerializeField] private float pistolFireRate;
    [SerializeField] private bool pistolIsAutomatic = false;
    [SerializeField] private int pistolDamage;
    [SerializeField] private bool pistolCanZoom;
    [SerializeField] private int pistolZoomField;
    [SerializeField] private float pistolZoomRotationSpeed;
    [SerializeField] private int pistolAmmo;
    [SerializeField] private int pistolAmmoStock;
    private int pistolCurrentAmmo;
    private int pistolCurrentAmmoStock;
    
    [Header("SMG")]
    [SerializeField] private GameObject smg;
    [SerializeField] private Animator smgAnimator;
    [SerializeField] private ParticleSystem smgHitAirParticle;
    [SerializeField] private ParticleSystem smgShootParticle;
    [SerializeField] private float smgFireRate;
    [SerializeField] private bool smgIsAutomatic = false;
    [SerializeField] private int smgDamage;
    [SerializeField] private bool smgCanZoom;
    [SerializeField] private int smgZoomField;
    [SerializeField] private float smgRotationSpeed;
    [SerializeField] private int smgAmmo;
    [SerializeField] private int smgAmmoStock;
    private int smgCurrentAmmo;
    private int smgCurrentAmmoStock;
    
    [Header("Awp")]
    [SerializeField] private GameObject awp;
    [SerializeField] private Animator awpAnimator;
    [SerializeField] private ParticleSystem awpHitAirParticle;
    [SerializeField] private ParticleSystem awpShootParticle;
    [SerializeField] private float awpFireRate;
    [SerializeField] private bool awpIsAutomatic = false;
    [SerializeField] private int awpDamage;
    [SerializeField] private bool awpCanZoom;
    [SerializeField] private int awpZoomField;
    [SerializeField] private GameObject awpZoomCanvasImage;
    [SerializeField] private float awpZoomRotationSpeed;
    [SerializeField] private int awpAmmo;
    [SerializeField] private int awpAmmoStock;
    private int awpCurrentAmmo;
    private int awpCurrentAmmoStock;
    
    private void Awake()
    {
        _starterAssetsInputs = GetComponentInParent<StarterAssetsInputs>();
        _firstPersonController = GetComponentInParent<FirstPersonController>();
        _mainCamera = Camera.main;
        _selectedWeapon = Weapon.Empty;
        _weaponsSlot.Clear();
    }

    private void Start()
    {
        _mainCameraDefaultZoomField = playerFollowCamera.m_Lens.FieldOfView;
        _defaultRotationSpeed = _firstPersonController.RotationSpeed;
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
        ammoText.text = CurrentAmmo.ToString();
        ammoStockText.text = CurrentAmmoStock.ToString();
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
                            GunOpenZoom();
                        else
                            GunCloseZoom();
                        _starterAssetsInputs.ZoomInput(false);
                    }
                }
                else // hold right click
                {
                    if (_starterAssetsInputs.zoom)
                        GunOpenZoom();
                    else
                        GunCloseZoom();
                }
            }
        }
    }

    private void Fire()
    {
        if (!AmmoControl())
            return;
        _delayControl = false;
        StartCoroutine(DelayFireByRate());
        _selectedShootParticle.Play();
        _selectedAnimator.Play(_selectedWeaponFireAnimName,0, 0);
        RaycastHit hit;
        if (Physics.Raycast(_mainCamera.transform.position, _mainCamera.transform.forward, out hit,Mathf.Infinity, raycastLayerMask, QueryTriggerInteraction.Ignore)) // ctrl + shift + space constructor ları görüntüler
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

    private bool AmmoControl()
    {
        if (CurrentAmmoStock > 0)
        {
            if (CurrentAmmo > 0)
            {
                CurrentAmmo--;
            }
            if (CurrentAmmo == 0)
            {
                CurrentAmmoStock--;
                CurrentAmmo = _selectedGunAmmo;
            }
        }
        if (CurrentAmmoStock == 0 && CurrentAmmo == 0)
            return false;
        return true;
    }

    private void SetAmmoReturnToGun()
    {
        switch (_selectedWeapon)
        {
            case Weapon.Pistol:
                pistolCurrentAmmo = CurrentAmmo;
                pistolCurrentAmmoStock = CurrentAmmoStock;
                break;
            case Weapon.Smg:
                smgCurrentAmmo = CurrentAmmo;
                smgCurrentAmmoStock = CurrentAmmoStock;
                break;
            case Weapon.Awp:
                awpCurrentAmmo = CurrentAmmo;
                awpCurrentAmmoStock = CurrentAmmoStock;
                break;
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
                _weaponsSlot.Add(new WeaponSlot{ weaponGo = pistol , weaponType = weaponType, isActive = false});
                break;
            case Weapon.Smg:
                _isSmgAvailable = true;
                _weaponsSlot.Add(new WeaponSlot{ weaponGo = smg , weaponType = weaponType, isActive = false});
                break;
            case Weapon.Awp:
                _isAwpAvailable = true;
                _weaponsSlot.Add(new WeaponSlot{ weaponGo = awp , weaponType = weaponType, isActive = false});
                break;
        }
        for (int i = 0; i < _weaponsSlot.Count; i++)
        {
            _weaponsSlot[i].weaponGo.SetActive(false);
            WeaponSlot slotFor = _weaponsSlot[i];
            slotFor.isActive = false;
            _weaponsSlot[i] = slotFor;
        }
        SetAmmoReturnToGun();
        SelectGun(_weaponsSlot.Count - 1);
    }

    private void SelectGunByScroll(float scrollValue)
    {
        // scroll da sorun var - ileri geri yapıldığında bug lı çalışıyor kontrol edilecek.
        if (_weaponsSlot.Count < 1)
            return;
        for (int i = 0; i < _weaponsSlot.Count; i++)
        {
            if (_weaponsSlot[i].isActive)
            {
                _currentSlotIndex = i;
                break;
            }
        }
        int scrollNormalized = 0;
        
        if (scrollValue > 0)
            scrollNormalized = 1;
        else if (scrollValue < 0)
            scrollNormalized = -1;
        
        if (_currentSlotIndex + scrollNormalized == _weaponsSlot.Count)
            _currentSlotIndex = 0;
        else if (_currentSlotIndex + scrollNormalized == 0)
            _currentSlotIndex = 0;
        else if (_currentSlotIndex + scrollNormalized < 0)
            _currentSlotIndex = _weaponsSlot.Count - 1;
        else
            _currentSlotIndex += scrollNormalized;
        
        for (int i = 0; i < _weaponsSlot.Count; i++)
        {
            _weaponsSlot[i].weaponGo.SetActive(false);
            WeaponSlot slotFor = _weaponsSlot[i];
            slotFor.isActive = false;
            _weaponsSlot[i] = slotFor;
        }
        SetAmmoReturnToGun();
        SelectGun(_currentSlotIndex);
    }
    
    private void SelectGunByKeyboard(int slotNumber)
    {
        if (_weaponsSlot.Count < slotNumber)
            return;
        for (int i = 0; i < _weaponsSlot.Count; i++)
        {
            _weaponsSlot[i].weaponGo.SetActive(false);
            WeaponSlot slotFor = _weaponsSlot[i];
            slotFor.isActive = false;
            _weaponsSlot[i] = slotFor;
        }
        SetAmmoReturnToGun();
        SelectGun(slotNumber - 1);
    }

    private void SelectGun(int slotPoint)
    {
        _weaponsSlot[slotPoint].weaponGo.SetActive(true);
        _selectedWeapon = _weaponsSlot[slotPoint].weaponType;
        // struct value type kopya döndürür direkt olarak değerleri değiştirilemez.
        WeaponSlot slot = _weaponsSlot[slotPoint];
        slot.isActive = true;
        _weaponsSlot[slotPoint] = slot;
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
                _selectedZoomField = pistolZoomField;
                _selectedGunZoomCanvasImage = null;

                pistolZoomRotationSpeed = _defaultRotationSpeed;
                _selectedZoomRotationSpeed = pistolZoomRotationSpeed;

                if (!_pistolInitialized)
                {
                    _selectedGunAmmo = pistolAmmo;
                    _selectedGunAmmoStock = pistolAmmoStock;
                    pistolCurrentAmmo = pistolAmmo;
                    pistolCurrentAmmoStock = pistolAmmoStock;
                    _pistolInitialized = true;
                }
                else
                {
                    _selectedGunAmmo = pistolCurrentAmmo;
                    _selectedGunAmmoStock = pistolCurrentAmmoStock;
                }
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
                _selectedZoomField = smgZoomField;
                
                pistolZoomRotationSpeed = _defaultRotationSpeed;
                _selectedGunZoomCanvasImage = null;
                
                if (!_smgInitialized)
                {
                    _selectedGunAmmo = smgAmmo;
                    _selectedGunAmmoStock = smgAmmoStock;
                    smgCurrentAmmo = smgAmmo;
                    smgCurrentAmmoStock = smgAmmoStock;
                    _smgInitialized = true;
                }
                else
                {
                    _selectedGunAmmo = smgCurrentAmmo;
                    _selectedGunAmmoStock = smgCurrentAmmoStock;
                }
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
                _selectedZoomField = awpZoomField;
                _selectedGunZoomCanvasImage = awpZoomCanvasImage;
                _selectedZoomRotationSpeed = awpZoomRotationSpeed;
                
                if (!_awpInitialized)
                {
                    _selectedGunAmmo = awpAmmo;
                    _selectedGunAmmoStock = awpAmmoStock;
                    awpCurrentAmmo = awpAmmo;
                    awpCurrentAmmoStock = awpAmmoStock;
                    _awpInitialized = true;
                }
                else
                {
                    _selectedGunAmmo = awpCurrentAmmo;
                    _selectedGunAmmoStock = awpCurrentAmmoStock;
                }
                break;
        }

        CurrentAmmo = _selectedGunAmmo;
        CurrentAmmoStock = _selectedGunAmmoStock;
    }
    
    private void GunOpenZoom()
    {
        if (_selectedGunZoomCanvasImage != null)
        {
            _selectedGunZoomCanvasImage.SetActive(true);
            defaultAimCanvasImage.SetActive(false);
        }
        playerFollowCamera.m_Lens.FieldOfView = _selectedZoomField;
        _firstPersonController.ChangeRotationSpeed(_selectedZoomRotationSpeed);
    }
    
    private void GunCloseZoom()
    {
        if (_selectedGunZoomCanvasImage != null)
        {
            _selectedGunZoomCanvasImage.SetActive(false);
            defaultAimCanvasImage.SetActive(true);
        }
        _firstPersonController.ChangeRotationSpeed(_defaultRotationSpeed);
        playerFollowCamera.m_Lens.FieldOfView = _mainCameraDefaultZoomField;
    }
}
