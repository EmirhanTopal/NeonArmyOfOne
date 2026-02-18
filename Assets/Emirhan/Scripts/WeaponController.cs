using System;
using StarterAssets;
using Unity.Mathematics;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    private int _rayMaxDistance;
    private StarterAssetsInputs _starterAssetsInputs;

    private Camera _mainCamera;
    
    private void Awake()
    {
        _starterAssetsInputs = GetComponentInParent<StarterAssetsInputs>();
        _mainCamera = Camera.main;
    }

    void Update()
    {
        if (_starterAssetsInputs.shoot)
        {
            RaycastHit _hit;
            if (Physics.Raycast(_mainCamera.transform.position, _mainCamera.transform.forward, out _hit, 100))
            {
                // “Bu objede IDamageable kullanan bir script var mı?”
                // Eğer EnemyHealth bu interface’i implement ettiyse:
                // → Evet, var!
                // Ve onu döndürür.
                IDamagable _damagable = _hit.collider.GetComponent<IDamagable>();
                if (_damagable != null)
                {
                    if (_hit.transform.gameObject.CompareTag("Enemy"))
                    {
                        _damagable.TakeDamage(25);
                        Debug.Log(_hit.transform.gameObject.name);
                    }
                }
                _starterAssetsInputs.ShootInput(false);
            }
            else
            {
                _starterAssetsInputs.ShootInput(false);
            }
        }
        
    }
}
