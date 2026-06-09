using System.Collections.Generic;
using UnityEngine;

public class Rocket : Projectile
{
    [Header("Explosion Settings")]
    public int explosionDamage = 120;
    public float explosionRadius = 10f;
    public GameObject explosionEffectPrefab;

    [Header("Damage Falloff")]
    [Tooltip("Damage dealt at the very edge of the radius. Direct hits deal explosionDamage.")]
    public int minExplosionDamage = 35;
    [Tooltip("Base self-damage at a direct hit when friendlyFire is false. Scales down with distance.")]
    public int reducedSelfDamage = 30;
    [Tooltip("Higher = damage falls off faster the further a target is from the blast.")]
    [Min(0.1f)] public float damageFalloffExponent = 2f;

    [Header("Knockback")]
    [Tooltip("Knockback speed (m/s) at the center of the blast; scales down with distance.")]
    public float knockbackForce = 12f;
    [Tooltip("Upward bias added to the rocket's travel direction for the knockback push.")]
    [Range(0f, 2f)] public float upwardKnockback = 0.4f;

    [Header("Deflection Settings")]
    public float reverseSpeedMultiplier = 5f;
    public bool friendlyFire = false; // false = reduced self-damage (+ full knockback); true = full damage to the player too

    private GameObject player;
    private Coroutine destructionCoroutine;

    // BUG FIX: OnCollisionEnter can fire for several colliders within the same
    // physics step (e.g. ground + enemy), which called Explode() more than once →
    // double AoE damage and a redundant Destroy. Guard so it only explodes once.
    private bool hasExploded = false;

    [SerializeField] ProjectileWeapon weapon; // kinda unneeded now, but whatever

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            Debug.LogError("No GameObject with tag 'Player' found!", this);
    }
    public override void Reflect(Vector3 newDirection, float speedMultiplier = 1.5f)
    {
        base.Reflect(newDirection, speedMultiplier);

        // Allow the reflected rocket to damage enemies (and the sniper that fired it).
        // Without this, friendlyFire stays false and OnCollisionEnter ignores enemy hits.
        friendlyFire = true;

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

        // Don't detonate on the player's own weapon (layer 7 = Weapon). The rocket spawns
        // right next to the gun, so exploding here would just blow up at the muzzle.
        if (collision.gameObject.layer == 7)
        {
            Debug.Log("[Rocket] Hit own weapon — ignoring.");
            return;
        }

        // Everything else detonates: ground, walls, enemies, AND the player (layer 6).
        // friendlyFire no longer decides *whether* we explode — it only scales the damage
        // inside Explode (reduced self-damage when off). Firing at your own feet is now a
        // rocket-jump instead of a dud that bounces off you.
        Debug.Log("[Rocket] Exploding.");
        Explode(collision.contacts[0].point);
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
        if (hasExploded) return;
        hasExploded = true;

        Debug.Log($"[Rocket] Exploding at {position}");

        if (explosionEffectPrefab)
        {
            GameObject explosion = Instantiate(explosionEffectPrefab, position, Quaternion.identity);
            ParticleSystem ps = explosion.GetComponent<ParticleSystem>();
            float duration = ps != null ? ps.main.duration : 3f;
            Destroy(explosion, duration);
        }

        Collider[] hits = Physics.OverlapSphere(position, explosionRadius);
        HashSet<IDamageable> processed = new HashSet<IDamageable>();

        foreach (Collider hit in hits)
        {
            IDamageable damageable = hit.GetComponentInParent<IDamageable>();
            if (damageable == null || processed.Contains(damageable))
                continue;
            processed.Add(damageable);

            // Distance from the blast center to the nearest point of this target.
            // Use bounds.ClosestPoint: it works for every collider type (incl. the
            // player's CharacterController and non-convex meshes), unlike Collider.ClosestPoint.
            Vector3 closestPoint = hit.bounds.ClosestPoint(position);
            float distance = Vector3.Distance(position, closestPoint);
            if (distance > explosionRadius)
                continue; // outside the blast: no damage, no knockback

            // Falloff: 1 at a direct hit (center) → 0 at the edge of the radius.
            float falloff = Mathf.Pow(1f - Mathf.Clamp01(distance / explosionRadius), damageFalloffExponent);

            Component targetComponent = damageable as Component;
            bool isPlayer = targetComponent != null && targetComponent.CompareTag("Player");

            // Knockback is radial: push every target (enemies and the player) directly away
            // from the blast center, with a small upward bias so they pop up a little.
            Vector3 away = hit.bounds.center - position;
            Vector3 pushDir = away.sqrMagnitude > 0.0001f ? away.normalized : Vector3.up;
            Vector3 knockDir = (pushDir + Vector3.up * upwardKnockback).normalized;

            // ── Damage ───────────────────────────────────────────────────────
            int damage;
            if (isPlayer && !friendlyFire)
                damage = Mathf.RoundToInt(reducedSelfDamage * falloff);                                // reduced self-damage
            else
                damage = Mathf.RoundToInt(Mathf.Lerp(minExplosionDamage, explosionDamage, falloff));   // full damage: 120 → 35

            if (damage > 0)
            {
                Vector3 hitNormal = (closestPoint - position).sqrMagnitude > 0.001f
                    ? (closestPoint - position).normalized
                    : knockDir;
                damageable.TakeDamage(damage, position, hitNormal, distance);
            }

            // ── Knockback (after damage, so a killed enemy is already ragdolled) ──
            IKnockbackable knockbackable = hit.GetComponentInParent<IKnockbackable>();
            if (knockbackable != null)
                knockbackable.ApplyKnockback(knockDir * (knockbackForce * falloff));
        }

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        if (destructionCoroutine != null)
        {
            StopCoroutine(destructionCoroutine);
        }

        Destroy(gameObject);
    }

    public void SetDestructionCoroutine(Coroutine routine)
    {
        destructionCoroutine = routine;
    }
}
