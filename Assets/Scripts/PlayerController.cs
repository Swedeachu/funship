using UnityEngine;

public class PlayerController : MonoBehaviour
{
  [Header("Movement Settings")]
  public float baseAcceleration = 5f;
  public float jerk = 10f;
  public float decelMultiplier = 2f;
  public float maxSpeed = 5f;
  public float linearFriction = 3f;

  [Header("Rotation Settings")]
  public float baseRotAcceleration = 100f;
  public float rotJerk = 200f;
  public float rotDecelMultiplier = 2f;
  public float maxRotSpeed = 180f;
  public float rotationalFriction = 10f;

  [Header("Gunship Sizes")]
  public Vector3[] scales = { new Vector3(1, 1, 1), new Vector3(2, 2, 1), new Vector3(4, 4, 1) };
  public float[] speedFactors = { 1f, 0.5f, 0.25f };

  public static int sizeIndex = 0;

  private Rigidbody2D rb;
  private float currentAcceleration;
  private float currentSpeed;
  private float currentRotAcceleration;
  private float currentRotSpeed;

  // Reference to the PlayerHealthController to get current health
  private PlayerHealthController healthController;

  void Start()
  {
    rb = GetComponent<Rigidbody2D>();
    healthController = GetComponent<PlayerHealthController>();
    ApplySize();
  }

  void Update()
  {
    if (Input.GetKeyDown(KeyCode.Alpha1)) { sizeIndex = 0; ApplySize(); }
    if (Input.GetKeyDown(KeyCode.Alpha2)) { sizeIndex = 1; ApplySize(); }
    if (Input.GetKeyDown(KeyCode.Alpha3)) { sizeIndex = 2; ApplySize(); }

    // Update time scale if bullet time is active
    if (PlayerHealthController.BULLET_TIME)
    {
      Time.timeScale = 0.3f; // Slows everything
      Time.fixedDeltaTime = 0.02f * Time.timeScale; // Physics stays synced
    }
    else
    {
      Time.timeScale = 1f;
      Time.fixedDeltaTime = 0.02f;
    }
  }

  void FixedUpdate()
  {
    // Bullet time override multiplier: keeps ship at full speed
    float timeScaleComp = (Time.timeScale > 0f) ? 1f / Time.timeScale : 1f;

    float healthRatio = 1f;
    if (healthController != null)
    {
      healthRatio = Mathf.Clamp01(healthController.currentHealth / healthController.maxHealth);
    }

    // Movement input
    float vertical = Input.GetAxisRaw("Vertical");
    bool decel = (vertical != 0 && currentSpeed != 0 && Mathf.Sign(currentSpeed) != Mathf.Sign(vertical));
    float targetAccel = vertical * baseAcceleration * speedFactors[sizeIndex] * (decel ? decelMultiplier : 1f);

    // Reduce movement based on health
    targetAccel *= Mathf.Lerp(0.3f, 1f, healthRatio); // Less maneuverable at low health

    currentAcceleration = Mathf.MoveTowards(currentAcceleration, targetAccel, jerk * Time.fixedDeltaTime);
    currentSpeed += currentAcceleration * Time.fixedDeltaTime;
    currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed * speedFactors[sizeIndex], maxSpeed * speedFactors[sizeIndex]);

    if (vertical == 0)
    {
      currentSpeed = Mathf.MoveTowards(currentSpeed, 0, linearFriction * Time.fixedDeltaTime);
    }

    // Rotation input
    float horizontal = -Input.GetAxisRaw("Horizontal");
    bool rotDecel = (horizontal != 0 && currentRotSpeed != 0 && Mathf.Sign(currentRotSpeed) != Mathf.Sign(horizontal));
    float targetRotAccel = horizontal * baseRotAcceleration * speedFactors[sizeIndex] * (rotDecel ? rotDecelMultiplier : 1f);

    // Reduce rotation ability based on health
    targetRotAccel *= Mathf.Lerp(0.3f, 1f, healthRatio);

    currentRotAcceleration = Mathf.MoveTowards(currentRotAcceleration, targetRotAccel, rotJerk * Time.fixedDeltaTime);
    currentRotSpeed += currentRotAcceleration * Time.fixedDeltaTime;
    currentRotSpeed = Mathf.Clamp(currentRotSpeed, -maxRotSpeed * speedFactors[sizeIndex], maxRotSpeed * speedFactors[sizeIndex]);

    if (horizontal == 0)
    {
      currentRotSpeed = Mathf.MoveTowards(currentRotSpeed, 0, rotationalFriction * Time.fixedDeltaTime);
    }

    // Apply velocity and rotation – use timeScaleComp to ignore bullet time slowdown
    rb.velocity = transform.up * currentSpeed * timeScaleComp;
    rb.MoveRotation(rb.rotation + currentRotSpeed * Time.fixedDeltaTime * timeScaleComp);
  }

  void ApplySize()
  {
    transform.localScale = scales[sizeIndex];
    currentSpeed = 0f;
    currentAcceleration = 0f;
    currentRotSpeed = 0f;
    currentRotAcceleration = 0f;

    // Determine ship size label
    string sizeLabel = sizeIndex switch
    {
      0 => "Small Ship",
      1 => "Medium Ship",
      2 => "Big Ship",
      _ => ""
    };

    // Display it on the screen using BillboardService
    BillboardService.Instance?.ShowText(sizeLabel);
  }

}
