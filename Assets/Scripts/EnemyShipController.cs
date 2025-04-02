using UnityEngine;

public class EnemyShipController : MonoBehaviour
{
  public enum MovementPattern { Drift, ZigZag, Orbit }
  public MovementPattern movementPattern = MovementPattern.Drift;

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

  private Rigidbody2D rb;
  private Transform player;
  private float fireTimer = 0f;
  private float zigzagOffset;
  private float orbitAngle;

  private bool roamingToCenter = true;
  private Vector2 orbitCenter = Vector2.zero;
  private float orbitThreshold = 1.5f; // how close we must get to 0,0 before orbiting

  void Start()
  {
    rb = GetComponent<Rigidbody2D>();
    player = GameObject.Find("PlayerShip")?.transform;
    zigzagOffset = Random.Range(0f, Mathf.PI * 2f);
  }

  void FixedUpdate()
  {
    if (player == null) return;

    Vector2 toPlayer = (player.position - transform.position).normalized;
    float distanceToPlayer = Vector2.Distance(transform.position, player.position);

    if (distanceToPlayer <= detectionRange)
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

  void FireBullet(Vector2 shootDir)
  {
    if (bulletPrefab == null) return;

    // Calculate angle in degrees from direction vector
    float angle = Mathf.Atan2(shootDir.y, shootDir.x) * Mathf.Rad2Deg;

    // Create rotation that looks toward the player
    Quaternion rotation = Quaternion.Euler(0, 0, angle - 90f);

    // Instantiate with that rotation
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
    if (col.gameObject.CompareTag("Projectile"))
    {
      Destroy(col.gameObject);
      health -= 1f;

      if (health <= 0f)
      {
        Destroy(gameObject); // TODO: explosion effect, screen shake, billboard message
      }
    }
  }

}
