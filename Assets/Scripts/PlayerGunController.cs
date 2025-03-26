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

  // Kick/recoil settings
  [Header("Kick Settings")]
  public float maxKickAngle = 6f; // maximum added inaccuracy from recoil at full ramp

  void Update()
  {
    if (Input.GetKey(KeyCode.Space))
    {
      if (isCoolingDown)
      {
        StopCoroutine(cooldownRoutine);
        isCoolingDown = false;
      }

      timeDown += Time.deltaTime;

      float sizeFactor = 1f + (PlayerController.sizeIndex * 1f);
      float minFireRate = baseMinFireRate / sizeFactor;
      float maxFireRate = baseMaxFireRate / sizeFactor;

      float fireRate = Mathf.Lerp(minFireRate, maxFireRate, timeDown / buildUpTime);

      if (Time.time >= nextFireTime)
      {
        Shoot(sizeFactor, fireRate, minFireRate);
        nextFireTime = Time.time + fireRate;
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

  private IEnumerator SlowlyStopShooting()
  {
    isCoolingDown = true;

    float decayTime = 1.5f;
    float initialTimeDown = timeDown;
    float elapsed = 0f;

    while (timeDown > 0f)
    {
      elapsed += Time.deltaTime;
      float decayRatio = 1f - (elapsed / decayTime);
      timeDown = Mathf.Max(0f, initialTimeDown * decayRatio);

      float sizeFactor = 1f + (PlayerController.sizeIndex * 1f);
      float minFireRate = baseMinFireRate / sizeFactor;
      float maxFireRate = baseMaxFireRate / sizeFactor;
      float fireRate = Mathf.Lerp(minFireRate, maxFireRate, timeDown / buildUpTime);

      if (Time.time >= nextFireTime)
      {
        Shoot(sizeFactor, fireRate, minFireRate);
        nextFireTime = Time.time + fireRate;
      }

      yield return null;
    }

    timeDown = 0f;
    isCoolingDown = false;
  }

  private void Shoot(float sizeFactor, float fireRate, float minFireRate)
  {
    sizeFactor++;

    float maxOffset = baseBulletOffsetAngle * (PlayerController.sizeIndex + 1);
    float randomOffset = Random.Range(0.5f * maxOffset, maxOffset);
    float angleOffset = nextShotLeft ? randomOffset : -randomOffset;
    nextShotLeft = !nextShotLeft;

    // --- KICK INFLUENCE ---
    float kickRatio = Mathf.InverseLerp(minFireRate, baseMaxFireRate, fireRate);
    float kickAngle = Random.Range(-maxKickAngle, maxKickAngle) * (1f - kickRatio); // more random at high fire rate
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
