using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetScript : MonoBehaviour
{

  [Header("Target Settings")]
  public int health = 3; // How many hits the target can take

  void OnTriggerEnter2D(Collider2D collision)
  {
    // Check if the object that hit this target is a Projectile
    if (collision.tag == "Projectile")
    {
      Destroy(collision.gameObject); // Destroy the bullet or missile

      health--; // Decrement health

      if (health <= 0)
      {
        Destroy(gameObject); // Destroy the target if health reaches 0
      }
    }
  }

}
