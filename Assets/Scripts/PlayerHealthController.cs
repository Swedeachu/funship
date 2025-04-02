using UnityEngine;
using TMPro;

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

  public static bool BULLET_TIME = false;

  void Start()
  {
    currentHealth = maxHealth;
    UpdateHealthText();

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
    if (currentHealth < maxHealth)
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
    currentHealth = Mathf.Max(0f, currentHealth - amount);
    currentHealth = Mathf.Round(currentHealth * 100f) / 100f;
    UpdateHealthText();

    if (currentHealth <= 0f)
    {
      Debug.Log("Player died");
      // TODO: Handle death
    }
  }

  private void UpdateHealthText()
  {
    // Update health UI
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

    // Update smoke emission based on current health
    if (smokeSystem != null)
    {
      var emission = smokeSystem.emission;

      if (currentHealth < smokeStartThreshold)
      {
        float t = Mathf.InverseLerp(smokeStartThreshold, 0f, currentHealth); // 1 at full health, 0 at 0
        emission.rateOverTime = t * maxSmokeRate; // more smoke as health drops

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
