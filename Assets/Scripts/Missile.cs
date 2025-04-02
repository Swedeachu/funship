using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour
{
  [Header("Missile Physics Settings")]
  public float jerk = 40f;
  public float acceleration = 15f;
  public float maxSpeed = 120f;
  public float turnSpeed = 360f;
  public float lifespan = 12f;
  public float bananaTurnMultiplier = 3f;

  [Header("Sliding Phase Settings")]
  public float slideTime = 1f;
  public float decelerationFactor = 2f;

  private Rigidbody2D rb;
  private Transform target;
  private float currentAcceleration = 0f;
  private float slideTimer = 0f;

  void Start()
  {
    rb = GetComponent<Rigidbody2D>();
    FindNearestTarget();
    Destroy(gameObject, lifespan);
  }

  void Update()
  {
    float deltaTime = Time.unscaledDeltaTime;

    if (slideTimer < slideTime)
    {
      slideTimer += deltaTime;
      rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, decelerationFactor * deltaTime);
      return;
    }

    FindNearestTarget();

    if (target != null)
    {
      Vector2 toTarget = (Vector2)(target.position - transform.position);
      float distance = toTarget.magnitude;
      Vector2 direction = toTarget.normalized;

      float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
      float currentAngle = rb.rotation;
      float angleDiff = Mathf.DeltaAngle(currentAngle, targetAngle);

      float dynamicMultiplier = (Mathf.Abs(angleDiff) > 90f) ? bananaTurnMultiplier : 1f;
      float dynamicTurnSpeed = turnSpeed * dynamicMultiplier * (1 + (1f / Mathf.Max(distance, 0.1f)));

      float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, dynamicTurnSpeed * deltaTime);
      rb.MoveRotation(newAngle);
    }

    currentAcceleration = Mathf.MoveTowards(currentAcceleration, acceleration, jerk * deltaTime);
    Vector2 newVelocity = rb.velocity + (Vector2)(transform.up * currentAcceleration * deltaTime);
    rb.velocity = Vector2.ClampMagnitude(newVelocity, maxSpeed);
  }

  private void FindNearestTarget()
  {
    // Find all objects tagged "Target" and "Enemy"
    GameObject[] targetObjects = GameObject.FindGameObjectsWithTag("Target");
    GameObject[] enemyObjects = GameObject.FindGameObjectsWithTag("Enemy");

    // Combine them into a single list
    List<GameObject> allTargets = new List<GameObject>();
    allTargets.AddRange(targetObjects);
    allTargets.AddRange(enemyObjects);

    float minDistance = Mathf.Infinity;
    Transform closestTarget = null;

    // Loop through each potential target and determine the closest one
    foreach (GameObject potentialTarget in allTargets)
    {
      float distance = Vector2.Distance(transform.position, potentialTarget.transform.position);
      if (distance < minDistance)
      {
        minDistance = distance;
        closestTarget = potentialTarget.transform;
      }
    }

    // If a target was found, assign it
    if (closestTarget != null)
    {
      target = closestTarget;
    }
  }

}
