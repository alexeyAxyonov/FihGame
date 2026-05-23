using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Rocket : Projectile
{
    [Header("Explosion Settings")]
    public int explosionDamage = 120;
    public float explosionRadius = 10f;
    public GameObject explosionEffectPrefab;

    [Header("Deflection Settings")]
    public float reverseSpeedMultiplier = 5f;
    public bool friendlyFire = false; // TODO: take knockback but not damage on explode when flag == false

    private GameObject player;
    private Coroutine destructionCoroutine;

    [SerializeField] ProjectileWeapon weapon; // kinda unneeded now, but whatever

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            Debug.LogError("No GameObject with tag 'Player' found!", this);
    }
    /*
        void FixedUpdate()
        {
            // Visual: make the rocket face its velocity direction
            if (rb.linearVelocity.magnitude > 0.1f)
                transform.rotation = Quaternion.LookRotation(rb.linearVelocity);
        }
    */
    public override void Reflect(Vector3 newDirection, float speedMultiplier = 1.5f)
    {
        base.Reflect(newDirection, speedMultiplier);

        if (weapon != null)
        {
            weapon.bulletPrefabLifeTime += 10f;
        }

        Debug.Log($"[Rocket] Reflected! New direction: {newDirection}");
    }
    public override void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"[Rocket] Hit: {collision.gameObject.name}, Tag: {collision.gameObject.tag}, Layer: {LayerMask.LayerToName(collision.gameObject.layer)}");
        if (collision.gameObject.CompareTag("Shield"))
        {
            Debug.Log("[Rocket] Shield hit — deflecting!");
            Deflect(collision);
            return;
        }

        if (collision.gameObject.layer == 6 || collision.gameObject.layer == 7)
        {
            if (friendlyFire)
            {
                Debug.Log("[Rocket] Friendly fire enabled — exploding on enemy");
                Explode(collision.contacts[0].point);
            }
            else
            {
                Debug.Log("[Rocket] Friendly fire disabled — ignoring enemy hit");
            }
        }
        else
        {
            Debug.Log("[Rocket] Hit environment/other — exploding");
            Explode(collision.contacts[0].point);
        }
    }

    private void Deflect(Collision collision)
    {
        if (player == null)
        {
            Debug.LogError("[Rocket] Player reference is null! Cannot deflect.");
            return;
        }

        Vector3 direction = (player.transform.position - transform.position).normalized;
        float currentSpeed = rb.linearVelocity.magnitude;

        float deflectSpeed = Mathf.Max(currentSpeed, 30f) * reverseSpeedMultiplier;

        Debug.Log($"[Rocket] Deflecting! Old velocity: {rb.linearVelocity}, New direction: {direction}, Speed: {deflectSpeed}");

        rb.linearVelocity = direction * deflectSpeed;

        if (weapon != null)
        {
            weapon.bulletPrefabLifeTime += 10f;
            Debug.Log($"[Rocket] Extended lifetime to {weapon.bulletPrefabLifeTime}");
        }
        else
        {
            Debug.LogWarning("[Rocket] Weapon reference is null — cannot extend lifetime.");
        }

        friendlyFire = true;
    }

    private void Explode(Vector3 position)
    {
        Debug.Log($"[Rocket] Exploding at {position}");

        if (explosionEffectPrefab)
        {
            GameObject explosion = Instantiate(explosionEffectPrefab, position, Quaternion.identity);
            ParticleSystem ps = explosion.GetComponent<ParticleSystem>();
            float duration = ps != null ? ps.main.duration : 3f;
            Destroy(explosion, duration);
        }

        Collider[] hits = Physics.OverlapSphere(position, explosionRadius);

        foreach (var hit in hits)
        {
            IDamageable damageable = hit.GetComponent<IDamageable>();
            if (damageable != null)
            {
                if (hit.CompareTag("Player") && !friendlyFire)
                {
                    Debug.Log("Rocket hit Player");
                }
                else
                {
                    Vector3 closestPoint = hit.ClosestPoint(position);
                    float distance = Vector3.Distance(position, closestPoint);

                    Vector3 hitNormal = (hit.transform.position - position).normalized;

                    damageable.TakeDamage(explosionDamage, position, hitNormal, distance);
                }
            }
        }

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        if (destructionCoroutine != null)
        {
            StopCoroutine(destructionCoroutine);
        }
        /*
         * Если визуальные странности во время взрыва
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
            renderer.enabled = false;
        */

        Destroy(gameObject);
    }

    public void SetDestructionCoroutine(Coroutine routine)
    {
        destructionCoroutine = routine;
    }
}
