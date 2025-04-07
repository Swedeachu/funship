using UnityEngine;

public class EnemyShipController : MonoBehaviour
{
  public enum MovementPattern { Drift, ZigZag, Orbit }
  public enum ShootPattern { Regular, Wave, SlowAndBig }
  public MovementPattern movementPattern = MovementPattern.Drift;
  public ShootPattern shootPattern = ShootPattern.Regular;

  [Header("Movement")]
  public float speed = 3f;
  public float zigzagAmplitude = 1f;
  public float zigzagFrequency = 2f;
  public float orbitRadius = 2f;
  public float orbitSpeed = 90f;

  [Header("Combat")]
  public float health = 10f;
  public float detectionRange = 50f;
  public float fireCooldown = 0.25f;
  public GameObject bulletPrefab;
  public GameObject explosionPrefab;

  private Rigidbody2D rb;
  private Transform player;
  private float fireTimer = 0f;
  private float zigzagOffset;
  private float orbitAngle;

  private bool roamingToCenter = true;
  private Vector2 orbitCenter = Vector2.zero;
  private float orbitThreshold = 1.5f; // how close we must get to 0,0 before orbiting

  private bool dead = false;

  void Start()
  {
    rb = GetComponent<Rigidbody2D>();
    player = GameObject.Find("PlayerShip")?.transform;
    zigzagOffset = Random.Range(0f, Mathf.PI * 2f);
    TelemetryManager.Instance.MarkBossSpawned();
  }

  void FixedUpdate()
  {
    if (player == null || dead) return;

    Vector2 toPlayer = (player.position - transform.position).normalized;
    float distanceToPlayer = Vector2.Distance(transform.position, player.position);

    if (distanceToPlayer <= detectionRange && !PlayerHealthController.DEAD)
    {
      roamingToCenter = false;

      Vector2 moveDir = GetMovementWithPattern(toPlayer);
      rb.velocity = moveDir * speed;

      fireTimer += Time.deltaTime;
      if (fireTimer >= fireCooldown)
      {
        fireTimer = 0f;
        FireBullet(toPlayer);
      }
    }
    else
    {
      RoamAroundOrigin();
    }
  }

  void RoamAroundOrigin()
  {
    float distanceToCenter = Vector2.Distance(transform.position, orbitCenter);

    if (distanceToCenter > orbitThreshold)
    {
      // Move directly to center point first
      Vector2 dirToCenter = (orbitCenter - (Vector2)transform.position).normalized;
      rb.velocity = dirToCenter * speed;
    }
    else
    {
      // Orbit around 0,0 with pattern added
      orbitAngle += orbitSpeed * Time.deltaTime;
      Vector2 orbitDir = new Vector2(Mathf.Cos(orbitAngle * Mathf.Deg2Rad), Mathf.Sin(orbitAngle * Mathf.Deg2Rad));
      Vector2 moveDir = GetMovementWithPattern(orbitDir);
      rb.velocity = moveDir * speed;
    }
  }

  Vector2 GetMovementWithPattern(Vector2 baseDir)
  {
    switch (movementPattern)
    {
      case MovementPattern.Drift:
        return baseDir;

      case MovementPattern.ZigZag:
        Vector2 perp = new Vector2(-baseDir.y, baseDir.x); // perpendicular
        float offset = Mathf.Sin(Time.time * zigzagFrequency + zigzagOffset) * zigzagAmplitude;
        return (baseDir + perp * offset).normalized;

      case MovementPattern.Orbit:
        orbitAngle += orbitSpeed * Time.deltaTime;
        Vector2 orbitOffset = new Vector2(Mathf.Cos(orbitAngle * Mathf.Deg2Rad), Mathf.Sin(orbitAngle * Mathf.Deg2Rad));
        return (baseDir + orbitOffset).normalized;

      default:
        return baseDir;
    }
  }

  private void FireBullet(Vector2 shootDir)
  {
    if (bulletPrefab == null || dead) return;

    switch (shootPattern)
    {
      case ShootPattern.Regular:
      {
        FireSingleBullet(shootDir);
        break;
      }

      case ShootPattern.Wave:
      {
        // Fire three bullets with a small angle offset
        float spreadAngle = 10f;

        for (int i = -1; i <= 1; i++)
        {
          Vector2 rotatedDir = Quaternion.Euler(0, 0, spreadAngle * i) * shootDir;
          FireSingleBullet(rotatedDir);
        }
        break;
      }

      case ShootPattern.SlowAndBig:
      {
        float angle = Mathf.Atan2(shootDir.y, shootDir.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle - 90f);
        GameObject bullet = Instantiate(bulletPrefab, transform.position, rotation);

        EnemyBulletController bulletCtrl = bullet.GetComponent<EnemyBulletController>();
        if (bulletCtrl != null)
        {
          Vector2 perp = new Vector2(-shootDir.y, shootDir.x);
          bulletCtrl.SetPattern(movementPattern, shootDir.normalized, perp.normalized);

          // Modify bullet size and speed
          bullet.transform.localScale *= 3.5f;
          // bulletCtrl.speed *= 0.5f;
          bulletCtrl.speed = 0.01f;
        }

        break;
      }
    }
  }

  private void FireSingleBullet(Vector2 shootDir)
  {
    float angle = Mathf.Atan2(shootDir.y, shootDir.x) * Mathf.Rad2Deg;
    Quaternion rotation = Quaternion.Euler(0, 0, angle - 90f);

    GameObject bullet = Instantiate(bulletPrefab, transform.position, rotation);
    EnemyBulletController bulletCtrl = bullet.GetComponent<EnemyBulletController>();

    if (bulletCtrl != null)
    {
      Vector2 perp = new Vector2(-shootDir.y, shootDir.x);
      bulletCtrl.SetPattern(movementPattern, shootDir.normalized, perp.normalized);
    }
  }

  void OnCollisionEnter2D(Collision2D col)
  {
    if (dead) return;

    if (col.gameObject.CompareTag("Projectile"))
    {
      Destroy(col.gameObject);
      health -= 1f;

      if (health <= 0f)
      {
        dead = true;

        // Spawn explosion effect at this position with the ship's rotation
        if (explosionPrefab != null)
        {
          GameObject explosion = Instantiate(explosionPrefab, transform.position, transform.rotation);
          Destroy(explosion, 2f); // Destroy explosion after 2 seconds
        }

        // Trigger screen shake
        Camera.main.GetComponent<ScreenShake>()?.Shake(2f, 0.8f, 0.8f);

        BillboardService.Instance?.ShowText("BOSS DEFEATED");

        // ExplosionDamage();
        ExplosionProjectiles();

        // Destroy the enemy ship
        Destroy(gameObject);

        TelemetryManager.Instance.MarkBossDefeated();
      }
    }
  }

  // Shoots out regular bullets in a 360� circle around the enemy
  private void ExplosionProjectiles()
  {
    if (bulletPrefab == null) return;

    int bulletCount = 15; // Number of bullets in the circle
    float angleStep = 360f / bulletCount;
    float startAngle = 0f;

    for (int i = 0; i < bulletCount; i++)
    {
      float angle = startAngle + i * angleStep;
      float rad = angle * Mathf.Deg2Rad;

      // Calculate shoot direction from angle
      Vector2 shootDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;

      // Set rotation so the bullet faces outward correctly
      Quaternion rotation = Quaternion.Euler(0f, 0f, angle - 90f);

      GameObject bullet = Instantiate(bulletPrefab, transform.position, rotation);
      EnemyBulletController bulletCtrl = bullet.GetComponent<EnemyBulletController>();

      if (bulletCtrl != null)
      {
        Vector2 perp = new Vector2(-shootDir.y, shootDir.x);
        bulletCtrl.SetPattern(MovementPattern.Drift, shootDir, perp);
      }
    }
  }


  private void ExplosionDamage()
  {
    // Find all PlayerHealthController instances in the scene
    PlayerHealthController[] players = GameObject.FindObjectsOfType<PlayerHealthController>();

    PlayerHealthController nearest = null;
    float nearestDistance = float.MaxValue;

    foreach (var player in players)
    {
      float dist = Vector2.Distance(transform.position, player.transform.position);

      if (dist < nearestDistance)
      {
        nearestDistance = dist;
        nearest = player;
      }
    }

    float units = 20f;

    // If the nearest player is within units, damage them
    if (nearest != null && nearestDistance <= units)
    {
      float damage = Mathf.Clamp(units - nearestDistance, 0f, units);
      nearest.TakeDamage(damage);
      Debug.Log($"Exploding boss damaged player for {damage} at distance {nearestDistance}");
    }
    else
    {
      Debug.Log($"Exploding boss damage too far away from player for at distance {nearestDistance}");
    }
  }

}