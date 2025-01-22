using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using DG.Tweening;

namespace Dialogue
{
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance;

        [SerializeField] private InputAction skipDialogIA;

        [Header("UI")]
        [SerializeField] private Image actorImage;
        [SerializeField] private TextMeshProUGUI actorNameText;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private RectTransform backgroundBox;

        [Header("Data")]
        [SerializeField] private Message[] currentMessages;
        [SerializeField] private Actor[] currentActors;

        [Header("Animation")]
        [SerializeField, Range(0, 1)] private float openBoxAnimationDuration = .5f;
        [SerializeField, Range(0, 1)] private float closeBoxAnimationDuration = .5f;
        [SerializeField, Range(0, .3f)] private float textTypingAnimationSpeed = .05f;

        [Header("Debug")]
        [SerializeField] private bool printDebug = false;

        private int _activeMessage = 0;

        private bool _isActive = false;
        private bool _isTyping = false;

        public bool isActive
        {
            get
            {
                return _isActive;
            }
            private set
            {
                _isActive = value;

                if (_isActive)
                {
                    OpenDialogueAnimation();
                }
                else
                {
                    CloseDialogueAnimation();
                }
            }
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogError($"Il y a plus d'une instance de {gameObject.GetType()} dans cette scène !");
                return;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                NextMessage();
            }

            //if (skipDialogIA.WasPerformedThisFrame() && isActive)
            //{
            //    NextMessage();
            //}
        }

        /// <summary>
        /// Ouvre un nouveau dialogue s'il n'y en a aucun en cours.
        /// </summary>
        /// <param name="messages">Les messages à afficher dans le dialogue.</param>
        /// <param name="actors">Les acteurs qui participent au dialogue.</param>
        public void OpenDialogue(Message[] messages, Actor[] actors)
        {
            if (isActive)
                return;

            currentMessages = messages;
            currentActors = actors;
            _activeMessage = 0;

            isActive = true;

            DisplayMessage();

            if (printDebug)
                Debug.Log($"Started conversation! Loaded messages: {messages.Length}");
        }

        /// <summary>
        /// Affiche le message dans la bulle de dialogue.
        /// </summary>
        private void DisplayMessage()
        {
            if (!isActive)
                return;

            Message messageToDisplay = currentMessages[_activeMessage];

            if (!_isTyping)
            { 
                StartCoroutine(TypeLineAnimation());
            }
            else
            {
                _isTyping = false;

                messageText.text = messageToDisplay.text;
            }

            Actor actorToDisplay = currentActors[messageToDisplay.actorID];
            actorNameText.text = actorToDisplay.actorName;
            actorImage.sprite = actorToDisplay.sprite;
        }

        /// <summary>
        /// Affiche le message suivant du dialogue.
        /// </summary>
        public void NextMessage()
        {
            if (!_isActive)
                return;

            if (_isTyping)
            {
                DisplayMessage();
            }
            else
            {
                _activeMessage++;

                if (_activeMessage < currentMessages.Length)
                {
                    DisplayMessage();
                }
                else
                {
                    isActive = false;

                    if (printDebug)
                        Debug.Log("Conversation ended!");
                }
            }
        }

        #region UI
        /// <summary>
        /// Met à jour la visibilité de la boîte de dialogue.
        /// </summary>
        private void UpdateVisibility()
        {
            backgroundBox.gameObject.SetActive(isActive);
        }
        #endregion

        #region Animation
        private void OpenDialogueAnimation()
        {
            UpdateVisibility();

            backgroundBox.transform.localScale = Vector3.zero;

            backgroundBox.DOScale(Vector3.one, openBoxAnimationDuration);

            if (printDebug)
                Debug.Log("Opening dialogue animation");
        }

        private void CloseDialogueAnimation()
        {
            backgroundBox.DOScale(Vector3.zero, closeBoxAnimationDuration).SetEase(Ease.InOutExpo).OnComplete(() =>
            {
                UpdateVisibility();

                backgroundBox.transform.localScale = Vector3.zero;
            });

            if (printDebug)
                Debug.Log("Closing dialogue animation");
        }

        private IEnumerator TypeLineAnimation()
        {
            messageText.text = "";

            _isTyping = true;

            foreach (char c in currentMessages[_activeMessage].text.ToCharArray())
            {
                if (!_isTyping)
                    yield break;

                messageText.text += c;

                yield return new WaitForSeconds(textTypingAnimationSpeed);
            }

            _isTyping = false;
        }
        #endregion
    }
}
