using System;
using System.Collections;
using UnityEngine;

public abstract class ProjectileWeapon : WeaponBase
{
    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    public float bulletVelocity = 30;
    public float bulletPrefabLifeTime = 10f; // немного так себе сделал в начале с этим, но и так сойдёт. Особо нет времени все недочёты править

    protected Coroutine destructionCoroutine;

    [Header("Burst Settings")] // probably remove them if whatnot
    public int bulletsPerBurst = 3;
    public int burstBulletsLeft;
    public float burstShootingDelay = 0.5f;

    // The guide put bullet spread here, since "some weapons might not be accurate". FishCorp produces only accurate weapons
    public int spreadIntensity = 0;
    public override void Awake()
    {
        readyToShoot = true;
        burstBulletsLeft = bulletsPerBurst;
        currentShootingMode = ShootingMode.Single;
    }

    public override void Attack()
    {
        // Создание пули
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.identity);

        // Выстрел пули
        bullet.GetComponent<Rigidbody>().AddForce(bulletSpawn.forward.normalized * bulletVelocity, ForceMode.Impulse);

        destructionCoroutine = StartCoroutine(DestroyBulletAfterTime(bullet));

    }
    protected IEnumerator DestroyBulletAfterTime(GameObject bullet)
    {
        float timer = 0f;
        while (timer < bulletPrefabLifeTime) 
        {
            timer += Time.deltaTime;
            yield return null;
        }
        Destroy(bullet);
    }
}
