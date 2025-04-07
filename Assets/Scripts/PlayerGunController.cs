using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGunController : MonoBehaviour
{
  public GameObject bulletPrefab;
  public GameObject missilePrefab;
  public Transform firePoint;
  public float buildUpTime = 3f;

  public float baseBulletSpeed = 15f;
  public float baseMinFireRate = 0.5f;
  public float baseMaxFireRate = 0.1f;
  public float baseBulletSize = 1f;

  public int missileCount = 5;
  public float missileSpreadAngle = 45f;

  private float timeDown = 0f;
  private float nextFireTime = 0f;

  private bool nextShotLeft = true;
  public float baseBulletOffsetAngle = 2f;

  private Coroutine cooldownRoutine = null;
  private bool isCoolingDown = false;

  [Header("Kick Settings")]
  public float maxKickAngle = 6f;

  // Timer for missile barrage when AI is controlling
  private float missileTimer = 0f;

  void Start()
  {
    // Initialize missile timer with a random interval between 5 and 10 seconds.
    missileTimer = Random.Range(5f, 10f);
  }

  void Update()
  {
    if (PlayerHealthController.DEAD) return;

    if (PlayerController.aiControlling)
    {
      aiControls();
      return;
    }

    if (Input.GetKey(KeyCode.Space))
    {
      if (isCoolingDown)
      {
        StopCoroutine(cooldownRoutine);
        isCoolingDown = false;
      }

      timeDown += Time.unscaledDeltaTime;

      float sizeFactor = 1f + (PlayerController.sizeIndex * 1f);
      float minFireRate = baseMinFireRate / sizeFactor;
      float maxFireRate = baseMaxFireRate / sizeFactor;

      float fireRate = Mathf.Lerp(minFireRate, maxFireRate, timeDown / buildUpTime);

      if (Time.unscaledTime >= nextFireTime)
      {
        Shoot(sizeFactor, fireRate, minFireRate);
        nextFireTime = Time.unscaledTime + fireRate;
      }
    }
    else if (!isCoolingDown && timeDown > 0f)
    {
      cooldownRoutine = StartCoroutine(SlowlyStopShooting());
    }

    if (Input.GetKeyDown(KeyCode.Return))
    {
      LaunchMissileBarrage();
    }
  }

  /// <summary>
  /// AI controls for the gun: automatically fires bullets by simulating a held fire button,
  /// and launches missile barrages on a randomized timer.
  /// </summary>
  private void aiControls()
  {
    // Simulate holding down the fire button by incrementing the buildup timer.
    timeDown += Time.unscaledDeltaTime;

    float sizeFactor = 1f + (PlayerController.sizeIndex * 1f);
    float minFireRate = baseMinFireRate / sizeFactor;
    float maxFireRate = baseMaxFireRate / sizeFactor;
    float fireRate = Mathf.Lerp(minFireRate, maxFireRate, timeDown / buildUpTime);

    if (Time.unscaledTime >= nextFireTime)
    {
      Shoot(sizeFactor, fireRate, minFireRate);
      nextFireTime = Time.unscaledTime + fireRate;
    }

    // Handle missile barrage: decrement missile timer and launch when it expires.
    missileTimer -= Time.unscaledDeltaTime;
    if (missileTimer <= 0f)
    {
      LaunchMissileBarrage();
      missileTimer = Random.Range(5f, 10f);
    }
  }

  private IEnumerator SlowlyStopShooting()
  {
    isCoolingDown = true;

    float decayTime = 1.5f;
    float initialTimeDown = timeDown;
    float elapsed = 0f;

    while (timeDown > 0f)
    {
      elapsed += Time.unscaledDeltaTime;
      float decayRatio = 1f - (elapsed / decayTime);
      timeDown = Mathf.Max(0f, initialTimeDown * decayRatio);

      float sizeFactor = 1f + (PlayerController.sizeIndex * 1f);
      float minFireRate = baseMinFireRate / sizeFactor;
      float maxFireRate = baseMaxFireRate / sizeFactor;
      float fireRate = Mathf.Lerp(minFireRate, maxFireRate, timeDown / buildUpTime);

      if (Time.unscaledTime >= nextFireTime)
      {
        Shoot(sizeFactor, fireRate, minFireRate);
        nextFireTime = Time.unscaledTime + fireRate;
      }

      yield return null;
    }

    timeDown = 0f;
    isCoolingDown = false;
  }

  private void Shoot(float sizeFactor, float fireRate, float minFireRate)
  {
    if (PlayerHealthController.DEAD) return;

    TelemetryManager.Instance.AddShotsFired(1);

    sizeFactor++;

    float maxOffset = baseBulletOffsetAngle * (PlayerController.sizeIndex + 1);
    float randomOffset = Random.Range(0.5f * maxOffset, maxOffset);
    float angleOffset = nextShotLeft ? randomOffset : -randomOffset;
    nextShotLeft = !nextShotLeft;

    float kickRatio = Mathf.InverseLerp(minFireRate, baseMaxFireRate, fireRate);
    float kickAngle = Random.Range(-maxKickAngle, maxKickAngle) * (1f - kickRatio);
    angleOffset += kickAngle;

    Quaternion offsetRotation = firePoint.rotation * Quaternion.Euler(0f, 0f, angleOffset);
    Vector3 spawnPosition = firePoint.position + (firePoint.up * 0.5f);
    GameObject bullet = Instantiate(bulletPrefab, spawnPosition, offsetRotation);

    Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
    if (rb != null)
    {
      rb.velocity = offsetRotation * Vector2.up * (baseBulletSpeed * sizeFactor);
    }

    bullet.transform.localScale = Vector3.one * (baseBulletSize * sizeFactor / 4);
  }

  private void LaunchMissileBarrage()
  {
    int mCount = missileCount * (PlayerController.sizeIndex + 1);
    float startAngle = -missileSpreadAngle * (mCount - 1) / 2f;
    float sizeFactor = 1f + (PlayerController.sizeIndex * 0.5f);

    TelemetryManager.Instance.AddMissilesFired(mCount);

    for (int i = 0; i < mCount; i++)
    {
      Quaternion spreadRotation = Quaternion.Euler(0, 0, startAngle + (i * missileSpreadAngle) + Random.Range(-5f, 5f));
      Vector3 spawnPosition = firePoint.position + (firePoint.up * Random.Range(0.3f, 1.0f));
      GameObject missile = Instantiate(missilePrefab, spawnPosition, firePoint.rotation * spreadRotation);

      Rigidbody2D rb = missile.GetComponent<Rigidbody2D>();
      if (rb != null)
      {
        rb.velocity = missile.transform.up * (5f * sizeFactor);
      }
    }
  }

}
