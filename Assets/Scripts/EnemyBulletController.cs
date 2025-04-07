using UnityEngine;

public class EnemyBulletController : MonoBehaviour
{

  public float speed = 0.25f;
  public float zigzagAmplitude = 1f;
  public float zigzagFrequency = 6f;
  public float orbitRadius = 1f;
  public float orbitSpeed = 180f;
  public float lifespan = 5f;

  private EnemyShipController.MovementPattern pattern;
  private Vector2 forward, right;
  private float birthTime;
  private float orbitAngle;
  private float zigzagOffset;

  void Start()
  {
    birthTime = Time.time;
    Destroy(gameObject, lifespan);
  }

  public void SetPattern(EnemyShipController.MovementPattern movePattern, Vector2 fwd, Vector2 rgt)
  {
    pattern = movePattern;
    forward = fwd.normalized;
    right = rgt.normalized;
    zigzagOffset = Random.Range(0f, Mathf.PI * 2f);
  }

  void FixedUpdate()
  {
    float t = Time.time - birthTime;
    Vector2 move = forward * speed;

    switch (pattern)
    {
      case EnemyShipController.MovementPattern.ZigZag:
        move += right * Mathf.Sin(t * zigzagFrequency + zigzagOffset) * zigzagAmplitude;
        break;

      case EnemyShipController.MovementPattern.Orbit:
        orbitAngle += orbitSpeed * Time.deltaTime;
        Vector2 orbitOffset = new Vector2(Mathf.Cos(orbitAngle * Mathf.Deg2Rad), Mathf.Sin(orbitAngle * Mathf.Deg2Rad)) * orbitRadius;
        move += orbitOffset;
        break;
    }

    transform.position += (Vector3)(move * Time.deltaTime);
  }

  void OnCollisionEnter2D(Collision2D collision)
  {
    PlayerHealthController playerHealth = collision.gameObject.GetComponent<PlayerHealthController>();
    if (playerHealth != null)
    {
      playerHealth.TakeDamage(Random.Range(3, 5));
      Destroy(gameObject);
    }
  }

}
