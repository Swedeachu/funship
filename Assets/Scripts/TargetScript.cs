using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))] // Ensure Rigidbody2D is attached
public class TargetScript : MonoBehaviour
{

  [Header("Target Settings")]
  public int health = 3;

  [Header("Knockback Settings")]
  public float knockbackForce = 5f; // How hard the target gets knocked back

  private Rigidbody2D rb;

  void Awake()
  {
    rb = GetComponent<Rigidbody2D>();
  }

  void OnTriggerEnter2D(Collider2D collision)
  {
    // Check for projectile tag
    if (collision.CompareTag("Projectile"))
    {
      // Destroy the projectile
      Destroy(collision.gameObject);

      // Knockback logic
      if (rb != null)
      {
        // Try to use the projectile's velocity if it has a Rigidbody2D
        Rigidbody2D projectileRb = collision.GetComponent<Rigidbody2D>();
        if (projectileRb != null)
        {
          Vector2 knockDirection = projectileRb.velocity.normalized;
          rb.AddForce(knockDirection * knockbackForce, ForceMode2D.Impulse);
        }
        else
        {
          // Fallback to direction from projectile to target if no velocity found
          Vector2 fallbackDirection = (transform.position - collision.transform.position).normalized;
          rb.AddForce(fallbackDirection * knockbackForce, ForceMode2D.Impulse);
        }
      }

      // Reduce health
      health--;

      // Destroy target if health is depleted
      if (health <= 0)
      {
        Destroy(gameObject);
      }
    }
  }

}
