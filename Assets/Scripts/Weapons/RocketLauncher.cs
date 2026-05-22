using System.Collections;
using TMPro;
using UnityEngine;

public class RocketLauncher : ProjectileWeapon
{
    public override ShootingMode[] AvailableModes => new[] { ShootingMode.Single, ShootingMode.Burst };

    public float bulletsLeft;
    public int magazineSize = 6;

    public TextMeshProUGUI ammoDisplay;

    public override void Awake()
    {
        base.Awake();
        bulletsLeft = magazineSize;
    }
    public override void Attack()
    {
        if (!readyToShoot || bulletsLeft < 1)
            return;

        readyToShoot = false;

        print("attacked");

        FireProjectile();

        // If burst mode and more bullets left, start burst coroutine
        if (currentShootingMode == ShootingMode.Burst && bulletsPerBurst > 1)
        {
            StartCoroutine(BurstFire());
        }
        else
        {
            // Single shot: reset after cooldown
            StartCoroutine(ResetCooldown());
        }
        /* старый код был тут
        // Создание пули
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawn.position, bulletSpawn.rotation);
        //Debug.DrawRay(bullet.transform.position, bulletSpawn.forward * 5f, Color.red, 60f);
        //Debug.Log($"Weapon rotation: {bullet.transform.localRotation.eulerAngles}");
        //Debug.Log($"Weapon world rotation: {bullet.transform.rotation.eulerAngles}");
        //Debug.Log($"Parent rotation: {transform.rotation.eulerAngles}");
        // Выстрел пули

        Vector3 shootingDirection = CalculateDirection().normalized;

        bullet.transform.forward = shootingDirection;

        bullet.GetComponent<Rigidbody>().AddForce(shootingDirection * bulletVelocity, ForceMode.Impulse);

        StartCoroutine(DestroyBulletAfterTime(bullet));

        Rocket rocket = bullet.GetComponent<Rocket>();
        if (rocket != null)
            rocket.SetDestructionCoroutine(destructionCoroutine);
        */
    }
    private IEnumerator BurstFire()
    {
        for (int i = 1; i < bulletsPerBurst; i++)
        {
            yield return new WaitForSeconds(burstShootingDelay);
            FireProjectile();
        }
        // After the whole burst, do the full cooldown reset
        StartCoroutine(ResetCooldown());
    }

    private IEnumerator ResetCooldown()
    {
        yield return new WaitForSeconds(shootingDelay);
        readyToShoot = true;
        burstBulletsLeft = bulletsPerBurst;
    }

    private void FireProjectile()
    {
        bulletsLeft--;
        // Создание пули
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawn.position, bulletSpawn.rotation);
        //Debug.DrawRay(bullet.transform.position, bulletSpawn.forward * 5f, Color.red, 60f);
        //Debug.Log($"Weapon rotation: {bullet.transform.localRotation.eulerAngles}");
        //Debug.Log($"Weapon world rotation: {bullet.transform.rotation.eulerAngles}");
        //Debug.Log($"Parent rotation: {transform.rotation.eulerAngles}");
        // Выстрел пули

        Vector3 shootingDirection = CalculateDirection().normalized;

        bullet.transform.forward = shootingDirection;

        bullet.GetComponent<Rigidbody>().AddForce(shootingDirection * bulletVelocity, ForceMode.Impulse);

        StartCoroutine(DestroyBulletAfterTime(bullet));

        Rocket rocket = bullet.GetComponent<Rocket>();
        if (rocket != null)
            rocket.SetDestructionCoroutine(destructionCoroutine);
    }

    public Vector3 CalculateDirection()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        Vector3 targetPoint;
        if (Physics.Raycast(ray, out hit))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(100);
        }

        Vector3 direction = targetPoint - bulletSpawn.position;

        float x = UnityEngine.Random.Range(-spreadIntensity, spreadIntensity);
        float y = UnityEngine.Random.Range(-spreadIntensity, spreadIntensity);

        return direction + new Vector3(x, y, 0);
    }

    public void Update()
    {
        if (bulletsLeft < magazineSize)
        {
            bulletsLeft = ((bulletsLeft + 0.5f * Time.deltaTime) < magazineSize) ? bulletsLeft + 0.5f * Time.deltaTime : magazineSize;
        }

        if (ammoDisplay != null)
        {
            ammoDisplay.text = $"{bulletsLeft}/{magazineSize}";
        }
    }
}
