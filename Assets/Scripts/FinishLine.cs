using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class FinishLine : MonoBehaviour
{
    [Header("Celebration Effects")]
    [SerializeField] public ParticleSystem[] celebrationParticles;
    [SerializeField] public AudioClip victoryClip;
    [SerializeField] [Range(0f, 1f)] public float audioVolume = 0.8f;

    private float startTime;
    private bool levelFinished = false;

    private void Start()
    {
        startTime = Time.time;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (levelFinished) return;

        if (other.CompareTag("Player"))
        {
            levelFinished = true;
            float elapsed = Time.time - startTime;

            // 1. Disable player movement and death system
            var controller = other.GetComponent<ThirdPersonController>();
            if (controller != null)
            {
                controller.enabled = false;
            }

            var death = other.GetComponent<PlayerDeath>();
            if (death != null)
            {
                death.enabled = false;
            }

            // 2. Play celebration particle effects
            if (celebrationParticles != null)
            {
                foreach (var ps in celebrationParticles)
                {
                    if (ps != null) ps.Play();
                }
            }

            // 3. Play victory audio clip
            if (victoryClip != null)
            {
                AudioSource.PlayClipAtPoint(victoryClip, transform.position, audioVolume);
            }

            // 4. Spawn and show victory UI Canvas
            CreateVictoryUI(elapsed);

            // 5. Unlock and show cursor for UI interaction
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            Debug.Log($"Finish Line Crossed! Time: {elapsed:F2} seconds.");
        }
    }

    private void CreateVictoryUI(float timeElapsed)
    {
        // Create root UI Canvas GameObject
        GameObject canvasGo = new GameObject("VictoryCanvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGo.AddComponent<CanvasScaler>();
        canvasGo.AddComponent<GraphicRaycaster>();

        // Create dark overlay panel background
        GameObject panelGo = new GameObject("Panel");
        panelGo.transform.SetParent(canvasGo.transform, false);
        Image panelImage = panelGo.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.75f);
        
        RectTransform panelRect = panelGo.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        // Create Title Text (TMP)
        GameObject titleGo = new GameObject("TitleText");
        titleGo.transform.SetParent(panelGo.transform, false);
        TextMeshProUGUI titleText = titleGo.AddComponent<TextMeshProUGUI>();
        titleText.text = "QUALIFIED!";
        titleText.fontSize = 72;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = new Color(1f, 0.85f, 0f); // Vibrant Gold
        titleText.fontStyle = FontStyles.Bold | FontStyles.Italic;

        RectTransform titleRect = titleGo.GetComponent<RectTransform>();
        titleRect.anchoredPosition = new Vector2(0f, 100f);
        titleRect.sizeDelta = new Vector2(600f, 120f);

        // Create Time Display Text (TMP)
        GameObject timeGo = new GameObject("TimeText");
        timeGo.transform.SetParent(panelGo.transform, false);
        TextMeshProUGUI timeText = timeGo.AddComponent<TextMeshProUGUI>();
        timeText.text = $"YOUR TIME: {timeElapsed:F2}s";
        timeText.fontSize = 32;
        timeText.alignment = TextAlignmentOptions.Center;
        timeText.color = Color.white;

        RectTransform timeRect = timeGo.GetComponent<RectTransform>();
        timeRect.anchoredPosition = new Vector2(0f, 0f);
        timeRect.sizeDelta = new Vector2(600f, 60f);

        // Create Restart Button
        GameObject buttonGo = new GameObject("RestartButton");
        buttonGo.transform.SetParent(panelGo.transform, false);
        Image buttonImage = buttonGo.AddComponent<Image>();
        buttonImage.color = new Color(1f, 0f, 0.5f); // Vibrant Fall Guys Pink
        
        Button button = buttonGo.AddComponent<Button>();
        button.onClick.AddListener(() => {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        });

        RectTransform buttonRect = buttonGo.GetComponent<RectTransform>();
        buttonRect.anchoredPosition = new Vector2(0f, -100f);
        buttonRect.sizeDelta = new Vector2(220f, 60f);

        // Add text inside Button (TMP)
        GameObject buttonTextGo = new GameObject("Text");
        buttonTextGo.transform.SetParent(buttonGo.transform, false);
        TextMeshProUGUI buttonText = buttonTextGo.AddComponent<TextMeshProUGUI>();
        buttonText.text = "PLAY AGAIN";
        buttonText.fontSize = 24;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.color = Color.white;
        buttonText.fontStyle = FontStyles.Bold;

        RectTransform buttonTextRect = buttonTextGo.GetComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.sizeDelta = Vector2.zero;
    }
}
