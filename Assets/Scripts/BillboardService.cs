using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BillboardService : MonoBehaviour
{
  public static BillboardService Instance { get; private set; }

  [Header("Text Prefab")]
  public GameObject textPrefab;

  private List<RectTransform> activeTexts = new List<RectTransform>();

  // Increased spacing for clarity
  private float textSpacing = 90f;

  private void Awake()
  {
    if (Instance != null && Instance != this)
    {
      Destroy(gameObject);
      return;
    }

    Instance = this;
  }

  public void ShowText(string msg, float delay = 3f)
  {
    if (textPrefab == null)
    {
      Debug.LogWarning("Text prefab not assigned to BillboardService.");
      return;
    }

    GameObject instance = Instantiate(textPrefab, transform);
    TextMeshProUGUI tmp = instance.GetComponent<TextMeshProUGUI>();
    if (tmp != null)
    {
      tmp.text = msg;
    }

    RectTransform rect = instance.GetComponent<RectTransform>();
    if (rect != null)
    {
      // Set pivot and anchor to center so we can offset properly from screen center
      rect.anchorMin = new Vector2(0.5f, 0.5f);
      rect.anchorMax = new Vector2(0.5f, 0.5f);
      rect.pivot = new Vector2(0.5f, 0.5f);
      rect.anchoredPosition = Vector2.zero;

      activeTexts.Add(rect);
      UpdateTextPositions();

      StartCoroutine(RemoveAfterDelay(instance, delay, rect));
    }
    else
    {
      StartCoroutine(RemoveAfterDelay(instance, delay, null));
    }
  }

  private IEnumerator RemoveAfterDelay(GameObject obj, float delay, RectTransform rect)
  {
    yield return new WaitForSecondsRealtime(delay);

    if (rect != null)
    {
      activeTexts.Remove(rect);
      UpdateTextPositions();
    }

    if (obj != null)
    {
      Destroy(obj);
    }
  }

  private void UpdateTextPositions()
  {
    float fixedX = 300f; // fixed horizontal X position
    float baseY = 300f; // starting Y position for the first message (top of stack)

    int count = activeTexts.Count;
    if (count == 0) return;

    for (int i = 0; i < count; i++)
    {
      float yOffset = baseY - (i * textSpacing); // stack downward
      activeTexts[i].anchoredPosition = new Vector2(fixedX, yOffset);
    }
  }

}
