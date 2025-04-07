using UnityEngine;

public class PlayerController : MonoBehaviour
{
  private float baseAcceleration = 10f;
  private float jerk = 10f;
  private float decelMultiplier = 2f;
  private float maxSpeed = 10f;
  private float linearFriction = 3f;

  private float baseRotAcceleration = 140f;
  private float rotJerk = 500f;
  private float rotDecelMultiplier = 2.5f;
  private float maxRotSpeed = 220f;
  private float rotationalFriction = 25f;

  private Vector3[] scales = {
    new Vector3(1, 1, 1),
    new Vector3(2, 2, 1),
    new Vector3(3, 3, 1)
  };

  private float[] speedFactors = { 1f, 0.5f, 0.25f };

  public static int sizeIndex = 0;

  private Rigidbody2D rb;
  private float currentAcceleration;
  private float currentSpeed;
  private float currentRotAcceleration;
  private float currentRotSpeed;

  // so other components can access it easily
  public static bool aiControlling = false;

  private PlayerHealthController healthController;

  void Start()
  {
    rb = GetComponent<Rigidbody2D>();
    healthController = GetComponent<PlayerHealthController>();
    ApplySize();
  }

  void Update()
  {
    // TODO: apparently this is supposed to be "A" but that conflicts with WASD controls so I added the shift
    if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.A))
    {
      Debug.Log("Automode activated");
      aiControlling = !aiControlling;
    }

    if (PlayerHealthController.DEAD) return;

    if (Input.GetKeyDown(KeyCode.Alpha1)) { sizeIndex = 0; ApplySize(); }
    if (Input.GetKeyDown(KeyCode.Alpha2)) { sizeIndex = 1; ApplySize(); }
    if (Input.GetKeyDown(KeyCode.Alpha3)) { sizeIndex = 2; ApplySize(); }

    if (PlayerHealthController.BULLET_TIME && !PlayerHealthController.DEAD)
    {
      Time.timeScale = 0.15f; // was 0.3
      Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }
    else
    {
      Time.timeScale = 1f;
      Time.fixedDeltaTime = 0.02f;
    }
  }

  void FixedUpdate()
  {
    if (PlayerHealthController.DEAD)
    {
      rb.velocity = Vector2.zero;
      rb.angularVelocity = 0f;
      return;
    }

    if (aiControlling)
    {
      aiControls();
      return;
    }

    float timeScaleComp = (Time.timeScale > 0f) ? 1f / Time.timeScale : 1f;

    float healthRatio = (healthController != null)
      ? Mathf.Clamp01(healthController.currentHealth / healthController.maxHealth)
      : 1f;

    float vertical = Input.GetAxisRaw("Vertical");
    bool decel = (vertical != 0 && currentSpeed != 0 && Mathf.Sign(currentSpeed) != Mathf.Sign(vertical));
    float targetAccel = vertical * baseAcceleration * speedFactors[sizeIndex] * (decel ? decelMultiplier : 1f);
    targetAccel *= Mathf.Lerp(0.3f, 1f, healthRatio);

    currentAcceleration = Mathf.MoveTowards(currentAcceleration, targetAccel, jerk * Time.fixedDeltaTime);
    currentSpeed += currentAcceleration * Time.fixedDeltaTime;
    currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed * speedFactors[sizeIndex], maxSpeed * speedFactors[sizeIndex]);

    if (vertical == 0)
    {
      currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, linearFriction * Time.fixedDeltaTime);
    }

    float horizontal = -Input.GetAxisRaw("Horizontal");
    bool rotDecel = (horizontal != 0 && currentRotSpeed != 0 && Mathf.Sign(currentRotSpeed) != Mathf.Sign(horizontal));
    float targetRotAccel = horizontal * baseRotAcceleration * speedFactors[sizeIndex] * (rotDecel ? rotDecelMultiplier : 1f);
    targetRotAccel *= Mathf.Lerp(0.3f, 1f, healthRatio);

    currentRotAcceleration = Mathf.MoveTowards(currentRotAcceleration, targetRotAccel, rotJerk * Time.fixedDeltaTime);
    currentRotSpeed += currentRotAcceleration * Time.fixedDeltaTime;
    currentRotSpeed = Mathf.Clamp(currentRotSpeed, -maxRotSpeed * speedFactors[sizeIndex], maxRotSpeed * speedFactors[sizeIndex]);

    // sharper stopping when no rotation input
    if (horizontal == 0)
    {
      currentRotSpeed = Mathf.MoveTowards(currentRotSpeed, 0f, rotationalFriction * Time.fixedDeltaTime);
    }

    rb.velocity = transform.up * currentSpeed * timeScaleComp;
    rb.MoveRotation(rb.rotation + currentRotSpeed * Time.fixedDeltaTime * timeScaleComp);
  }

  /// <summary>
  /// AI controls for movement: the ship purposefully aims at the nearest enemy and weaves slightly around it.
  /// It computes a desired facing angle toward the target, applies a reduced sine-based weave for evasion,
  /// and adjusts forward thrust based on distance for a more skilled steering behavior.
  /// </summary>
  private void aiControls()
  {
    // Find the nearest enemy ship (tagged "Enemy")
    Transform target = GetNearestEnemy();
    float simulatedVertical = 1f; // forward thrust input (1 = full throttle)
    float simulatedHorizontal = 0f; // rotation input

    if (target != null)
    {
      // Calculate the vector from this ship to the target
      Vector2 toTarget = (Vector2)target.position - (Vector2)transform.position;
      float distanceToTarget = toTarget.magnitude;

      // Compute the desired facing angle (in degrees)
      // Subtract 90 because transform.up points 90 from the x-axis in Unity
      float desiredAngle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg - 90f;
      float currentAngle = transform.eulerAngles.z;
      // Determine the shortest angle difference between current and desired angles
      float angleDiff = Mathf.DeltaAngle(currentAngle, desiredAngle);

      // More aggressive turning: use a smaller divisor for higher sensitivity
      float turnInput = Mathf.Clamp(angleDiff / 20f, -1f, 1f);

      // Reduced weaving offset for precision aiming when a target is present
      float weaveFrequency = 3f;
      float weaveAmplitude = 0.5f;
      float weaveOffset = 0.2f * Mathf.Sin(Time.time * weaveFrequency) * weaveAmplitude;

      // Combine the turning input with the reduced weaving offset
      simulatedHorizontal = turnInput + weaveOffset;

      // Adjust forward thrust based on distance:
      // - Too close (< 2f): reduce throttle to avoid overshooting.
      // - Too far (> 8f): full throttle to close the gap.
      // - Otherwise, use moderate throttle.
      if (distanceToTarget < 2f)
      {
        simulatedVertical = 0.5f;
      }
      else if (distanceToTarget > 8f)
      {
        simulatedVertical = 1f;
      }
      else
      {
        simulatedVertical = 0.8f;
      }
    }
    else
    {
      // No target found: default to a gentle weaving pattern and moderate forward speed.
      simulatedVertical = 0.8f;
      simulatedHorizontal = Mathf.Sin(Time.time * 2f) * 0.5f;
    }

    // Process the simulated inputs similarly to the player-controlled movement:
    float timeScaleComp = (Time.timeScale > 0f) ? 1f / Time.timeScale : 1f;
    float healthRatio = (healthController != null)
        ? Mathf.Clamp01(healthController.currentHealth / healthController.maxHealth)
        : 1f;

    // --- Vertical (forward/backward) processing ---
    bool decel = (simulatedVertical != 0 && currentSpeed != 0 && Mathf.Sign(currentSpeed) != Mathf.Sign(simulatedVertical));
    float targetAccel = simulatedVertical * baseAcceleration * speedFactors[sizeIndex] * (decel ? decelMultiplier : 1f);
    targetAccel *= Mathf.Lerp(0.3f, 1f, healthRatio);

    currentAcceleration = Mathf.MoveTowards(currentAcceleration, targetAccel, jerk * Time.fixedDeltaTime);
    currentSpeed += currentAcceleration * Time.fixedDeltaTime;
    currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed * speedFactors[sizeIndex], maxSpeed * speedFactors[sizeIndex]);

    if (simulatedVertical == 0)
    {
      currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, linearFriction * Time.fixedDeltaTime);
    }

    // --- Horizontal (rotation) processing ---
    bool rotDecel = (simulatedHorizontal != 0 && currentRotSpeed != 0 && Mathf.Sign(currentRotSpeed) != Mathf.Sign(simulatedHorizontal));
    float targetRotAccel = simulatedHorizontal * baseRotAcceleration * speedFactors[sizeIndex] * (rotDecel ? rotDecelMultiplier : 1f);
    targetRotAccel *= Mathf.Lerp(0.3f, 1f, healthRatio);

    currentRotAcceleration = Mathf.MoveTowards(currentRotAcceleration, targetRotAccel, rotJerk * Time.fixedDeltaTime);
    currentRotSpeed += currentRotAcceleration * Time.fixedDeltaTime;
    currentRotSpeed = Mathf.Clamp(currentRotSpeed, -maxRotSpeed * speedFactors[sizeIndex], maxRotSpeed * speedFactors[sizeIndex]);

    if (simulatedHorizontal == 0)
    {
      currentRotSpeed = Mathf.MoveTowards(currentRotSpeed, 0f, rotationalFriction * Time.fixedDeltaTime);
    }

    // --- Apply the computed movement and rotation ---
    rb.velocity = transform.up * currentSpeed * timeScaleComp;
    rb.MoveRotation(rb.rotation + currentRotSpeed * Time.fixedDeltaTime * timeScaleComp);
  }

  /// <summary>
  /// Finds and returns the Transform of the nearest enemy (tagged as "Enemy").
  /// Returns null if no enemy is found.
  /// </summary>
  private Transform GetNearestEnemy()
  {
    GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
    Transform nearest = null;
    float bestDistance = Mathf.Infinity;

    foreach (GameObject enemy in enemies)
    {
      float dist = Vector2.Distance(transform.position, enemy.transform.position);
      if (dist < bestDistance)
      {
        bestDistance = dist;
        nearest = enemy.transform;
      }
    }

    return nearest;
  }

  public void ApplySize()
  {
    transform.localScale = scales[sizeIndex];
    currentSpeed = 0f;
    currentAcceleration = 0f;
    currentRotSpeed = 0f;
    currentRotAcceleration = 0f;

    string sizeLabel = sizeIndex switch
    {
      0 => "Small Ship",
      1 => "Medium Ship",
      2 => "Big Ship",
      _ => ""
    };

    BillboardService.Instance?.ShowText(sizeLabel);
  }

  void OnCollisionEnter2D(Collision2D collision)
  {
    if (PlayerHealthController.DEAD) return;

    if (collision.gameObject.CompareTag("Barrier"))
    {
      // Get the first contact point
      ContactPoint2D contact = collision.GetContact(0);
      Vector2 normal = contact.normal;

      // Set knockback strength and spin strength
      float knockbackStrength = 6f;
      float spinStrength = 120f;

      // Apply knockback: force ship away from the barrier using the contact normal
      rb.velocity = normal * knockbackStrength;
      currentSpeed = 0f;
      currentAcceleration = 0f;

      // Apply an angular velocity to spin the ship. Randomize spin direction.
      rb.angularVelocity = (Random.value > 0.5f ? spinStrength : -spinStrength);
      currentRotSpeed = 0f;
      currentRotAcceleration = 0f;
    }
  }

  public float GetCurrentAcceleration() { return currentAcceleration; }
  public float GetCurrentRotAcceleration() { return currentRotAcceleration; }
  public float GetCurrentRotSpeed() { return currentRotSpeed; }

}
