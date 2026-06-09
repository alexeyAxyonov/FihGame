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
        if (!readyToShoot || bulletsLeft < 1f)
            return;

        readyToShoot = false;

        print("attacked");

        FireProjectile();

        if (currentShootingMode == ShootingMode.Burst && bulletsPerBurst > 1)
        {
            StartCoroutine(BurstFire());
        }
        else
        {
            StartCoroutine(ResetCooldown());
        }
    }

    private IEnumerator BurstFire()
    {
        for (int i = 1; i < bulletsPerBurst; i++)
        {
            yield return new WaitForSeconds(burstShootingDelay);
            FireProjectile();
        }
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

        GameObject bullet = Instantiate(bulletPrefab, bulletSpawn.position, bulletSpawn.rotation);

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

        float x = Random.Range(-spreadIntensity, spreadIntensity);
        float y = Random.Range(-spreadIntensity, spreadIntensity);

        return direction + new Vector3(x, y, 0);
    }

    // BUG FIX: Update was public — Unity will still call it, but it being
    // public exposes it unnecessarily and allows external callers to invoke it.
    private void Update()
    {
        if (bulletsLeft < magazineSize)
        {
            bulletsLeft = ((bulletsLeft + 0.5f * Time.deltaTime) < magazineSize)
                ? bulletsLeft + 0.5f * Time.deltaTime
                : magazineSize;
        }

        if (ammoDisplay != null)
        {
            ammoDisplay.text = $"{Mathf.FloorToInt(bulletsLeft)}/{magazineSize}";
        }
    }
}
