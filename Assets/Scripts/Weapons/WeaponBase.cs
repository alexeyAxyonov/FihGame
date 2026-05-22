using TMPro;
using Unity.Services.Analytics;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour, IWeapon
{
    [SerializeField] protected int damage = 20;
    [SerializeField] protected GameObject weaponModel;
    [SerializeField] protected AudioClip fireSound;

    public Camera playerCamera;

    [Header("Shooting")]
    public bool isShooting, readyToShoot;
    public float shootingDelay = 0.7f;

    public enum ShootingMode
    {
        Single,
        Burst
    }
    public ShootingMode currentShootingMode;
    public abstract ShootingMode[] AvailableModes { get; }

    public virtual void Awake()
    {
        readyToShoot = true;
        currentShootingMode = ShootingMode.Single;
    }
    public virtual void Initialize(Camera cam)
    {
        playerCamera = cam;
    }

    public int Damage
    {
        get => damage;
        set => damage = value;
    }

    public abstract void Attack();
    public virtual void Equip() => gameObject.SetActive(true);
    public virtual void Unequip() => gameObject.SetActive(false);
}
