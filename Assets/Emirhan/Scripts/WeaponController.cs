using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
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
    [SerializeField] private GameObject defaultAimCanvasImage;
    
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
    
    private void Awake()
    {
        _starterAssetsInputs = GetComponentInParent<StarterAssetsInputs>();
        _firstPersonController = GetComponentInParent<FirstPersonController>();
        _mainCamera = Camera.main;
        _selectedWeapon = Weapon.Smg;
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

    private void Fire()
    {
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
                _selectedZoomField = pistolZoomField;
                _selectedGunZoomCanvasImage = null;

                pistolZoomRotationSpeed = _defaultRotationSpeed;
                _selectedZoomRotationSpeed = pistolZoomRotationSpeed;
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
                break;
        }
    }
    
    private void GunZoom()
    {
        if (_selectedGunZoomCanvasImage != null)
        {
            _selectedGunZoomCanvasImage.SetActive(true);
            defaultAimCanvasImage.SetActive(false);
        }
        playerFollowCamera.m_Lens.FieldOfView = _selectedZoomField;
        _firstPersonController.ChangeRotationSpeed(_selectedZoomRotationSpeed);
    }
    
    private void CloseZoom()
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
