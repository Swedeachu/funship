using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class PlayerHealthController : MonoBehaviour
{
  [Header("Health Settings")]
  public float maxHealth = 100f;
  public float currentHealth = 100f;

  [Header("UI")]
  public TextMeshProUGUI healthText;

  [Header("Smoke Particle Settings")]
  public ParticleSystem smokeSystem; // Assign via Inspector
  public float smokeStartThreshold = 95f;
  public float maxSmokeRate = 30f;

  [Header("Explosion Bullet Settings")]
  public GameObject bulletPrefab; 
  public int explosionBulletCount = 24;
  public float explosionBulletSpeed = 10f;

  public GameObject explosionPrefab;

  public static bool BULLET_TIME = false;
  public static bool DEAD = false;

  private Renderer[] renderers; // For toggling visibility

  private int iFrames = 0;

  void Start()
  {
    currentHealth = maxHealth;
    UpdateHealthText();

    renderers = GetComponentsInChildren<Renderer>();

    if (smokeSystem != null)
    {
      smokeSystem.Stop();
    }
  }

  void Update()
  {
    BULLET_TIME = currentHealth <= 25.0f;
  }

  void FixedUpdate()
  {
    iFrames--;

    if (!DEAD && currentHealth < maxHealth)
    {
      currentHealth += 0.01f;
      currentHealth = Mathf.Min(currentHealth, maxHealth);
      currentHealth = Mathf.Round(currentHealth * 100f) / 100f;

      UpdateHealthText();
    }
  }

  void OnCollisionEnter2D(Collision2D collision)
  {
    if (collision.gameObject.GetComponent<TargetScript>() != null)
    {
      TakeDamage(10f);
    }
  }

  public void TakeDamage(float amount)
  {
    if (DEAD) return;

    // we have hit cool down of 60 ticks
    if (iFrames > 0)
    {
      return;
    }
    iFrames = 60;

    TelemetryManager.Instance.AddDamageTaken(amount);

    currentHealth = Mathf.Max(0f, currentHealth - amount);
    currentHealth = Mathf.Round(currentHealth * 100f) / 100f;
    UpdateHealthText();

    if (currentHealth <= 0f)
    {
      DEAD = true;

      BillboardService.Instance?.ShowText("YOU DIED", 2f);

      TelemetryManager.Instance.AddDestruction();
      TelemetryManager.Instance.IncrementDeaths();

      ExplosionProjectiles();

      // Spawn explosion
      if (explosionPrefab != null)
      {
        GameObject explosion = Instantiate(explosionPrefab, transform.position, transform.rotation);
        Destroy(explosion, 2f);
      }

      // Trigger screen shake
      Camera.main.GetComponent<ScreenShake>()?.Shake(2f, 0.8f, 0.8f);

      // Hide ship
      SetShipVisible(false);

      // Start respawn routine
      StartCoroutine(RespawnAfterDelay(5f));
    }
  }

  private void ExplosionProjectiles()
  {
    if (bulletPrefab == null) return;

    float angleStep = 360f / explosionBulletCount;

    for (int i = 0; i < explosionBulletCount; i++)
    {
      float angle = i * angleStep;
      float rad = angle * Mathf.Deg2Rad;
      Vector2 shootDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;

      Quaternion rotation = Quaternion.Euler(0, 0, angle - 90f);
      Vector3 spawnPos = transform.position;

      GameObject bullet = Instantiate(bulletPrefab, spawnPos, rotation);

      // Set scale like PlayerGunController (for visual size match)
      float sizeFactor = 1f + (PlayerController.sizeIndex * 1f);
      bullet.transform.localScale = Vector3.one * (1f * sizeFactor / 4f);

      // Apply velocity
      Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
      if (rb != null)
      {
        rb.velocity = shootDir * (explosionBulletSpeed * sizeFactor);
      }
    }
  }

  private IEnumerator RespawnAfterDelay(float delay)
  {
    yield return new WaitForSecondsRealtime(delay);

    // Respawn at origin
    transform.position = new Vector3(0, 0, -1); // -1 makes it visible for some reason, camera layering shit

    currentHealth = maxHealth;
    UpdateHealthText();

    // Show ship
    SetShipVisible(true);

    DEAD = false;
  }

  private void SetShipVisible(bool visible)
  {
    foreach (var rend in renderers)
    {
      rend.enabled = visible;
    }

    // since this counts as a respawn we show the text again via apply size
    if (visible)
    {
      var pc = GetComponent<PlayerController>();
      if (pc != null)
      {
        pc.ApplySize();
      }
    }
  }

  private void UpdateHealthText()
  {
    if (healthText != null)
    {
      if (Mathf.Approximately(currentHealth % 1f, 0f))
      {
        healthText.text = $"Health: {Mathf.RoundToInt(currentHealth)}";
      }
      else
      {
        healthText.text = $"Health: {currentHealth:0.00}";
      }
    }

    if (smokeSystem != null)
    {
      var emission = smokeSystem.emission;

      if (currentHealth < smokeStartThreshold)
      {
        float t = Mathf.InverseLerp(smokeStartThreshold, 0f, currentHealth);
        emission.rateOverTime = t * maxSmokeRate;

        if (!smokeSystem.isPlaying)
        {
          smokeSystem.Play();
        }
      }
      else
      {
        emission.rateOverTime = 0f;

        if (smokeSystem.isPlaying)
        {
          smokeSystem.Stop();
        }
      }
    }
  }

}
