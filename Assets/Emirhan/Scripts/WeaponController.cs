using System;
using System.Collections;
using StarterAssets;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

public class WeaponController : MonoBehaviour
{
    [Header("Weapon Features")]
    private int _rayMaxDistance;
    private Weapon _selectedWeapon;
    private bool _delayControl = true;

    [Header("Weapon Scriptable Objects")]
    //private WeaponSo _selectedWeaponSo;
    //[SerializeField] private WeaponSo pistolWeaponSo;

    [Header("Animations")]
    private Animator _selectedAnimator;
    [SerializeField] Animator pistolAnimator;
    private string _fireAnimName;

    [Header("Particle Systems")]
    private ParticleSystem _selectedShootParticle;
    private ParticleSystem _selectedHitAirParticle;
    
    [Header("Pistol")]
    [SerializeField] private ParticleSystem pistolHitAirParticle;
    [SerializeField] private ParticleSystem pistolShootParticle;
    

    [Header("Other Features")]
    private StarterAssetsInputs _starterAssetsInputs;
    private Camera _mainCamera;
    
    private void Awake()
    {
        _starterAssetsInputs = GetComponentInParent<StarterAssetsInputs>();
        _mainCamera = Camera.main;
        _selectedWeapon = Weapon.Pistol;
        SelectGun();
    }

    private void OnEnable()
    {
        GetAnimationState.onAnimationFinished += DelayFire;
    }

    private void OnDisable()
    {
        GetAnimationState.onAnimationFinished -= DelayFire;
    }

    void Update()
    {
        if (_starterAssetsInputs.shoot && _delayControl)
        {
            //_delayControl = false;
            _selectedShootParticle.Play();
            _selectedAnimator.Play(_fireAnimName,0, 0);
            _starterAssetsInputs.ShootInput(false);
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
                Debug.Log(damagable);
            }
        }
    }

    private void DelayFire()
    {
        _delayControl = true;
    }

    private void SelectGun()
    {
        switch (_selectedWeapon)
        {
            case Weapon.Pistol:
                _selectedAnimator = pistolAnimator;
                _selectedShootParticle = pistolShootParticle;
                _selectedHitAirParticle = pistolHitAirParticle;
                _fireAnimName = "PistolFire";
                break;
        }
    }
}
