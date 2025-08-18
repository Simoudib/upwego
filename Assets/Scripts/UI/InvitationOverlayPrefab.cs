using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UpWeGo
{
    /// <summary>
    /// Simple component for invitation overlay prefabs.
    /// This helps organize the UI elements and provides easy access to them.
    /// </summary>
    public class InvitationOverlayPrefab : MonoBehaviour
    {
        [Header("UI Elements")]
        public TextMeshProUGUI inviterNameText;
        public TextMeshProUGUI invitationMessageText;
        public Button acceptButton;
        public Button declineButton;
        public CanvasGroup canvasGroup;

        [Header("Animation Settings")]
        public float fadeInDuration = 0.3f;
        public bool animateOnShow = true;

        void Awake()
        {
            // Auto-find components if not assigned
            if (inviterNameText == null || invitationMessageText == null || acceptButton == null || declineButton == null)
            {
                AutoFindComponents();
            }

            // Add CanvasGroup if missing
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }
        }

        void Start()
        {
            if (animateOnShow)
            {
                StartCoroutine(FadeInAnimation());
            }
        }

        private void AutoFindComponents()
        {
            // Find text components
            TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length > 0 && inviterNameText == null)
                inviterNameText = texts[0];
            if (texts.Length > 1 && invitationMessageText == null)
                invitationMessageText = texts[1];

            // Find button components
            Button[] buttons = GetComponentsInChildren<Button>();
            if (buttons.Length > 0 && acceptButton == null)
                acceptButton = buttons[0];
            if (buttons.Length > 1 && declineButton == null)
                declineButton = buttons[1];

            Debug.Log($"Auto-found components - Texts: {texts.Length}, Buttons: {buttons.Length}");
        }

        private System.Collections.IEnumerator FadeInAnimation()
        {
            if (canvasGroup == null) yield break;

            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = true;

            float elapsedTime = 0f;
            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
                yield return null;
            }

            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
        }

        public void SetInviterName(string name)
        {
            if (inviterNameText != null)
                inviterNameText.text = name;
        }

        public void SetInvitationMessage(string message)
        {
            if (invitationMessageText != null)
                invitationMessageText.text = message;
        }

        public void SetupButtons(System.Action onAccept, System.Action onDecline)
        {
            if (acceptButton != null)
            {
                acceptButton.onClick.RemoveAllListeners();
                acceptButton.onClick.AddListener(() => onAccept?.Invoke());
            }

            if (declineButton != null)
            {
                declineButton.onClick.RemoveAllListeners();
                declineButton.onClick.AddListener(() => onDecline?.Invoke());
            }
        }
    }
}

