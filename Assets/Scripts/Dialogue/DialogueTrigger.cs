using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dialogue
{
    public class DialogueTrigger : MonoBehaviour
    {
        [SerializeField] private Message[] messages;
        [SerializeField] private Actor[] actors;

        public void StartDialogue()
        {
            DialogueManager.Instance.OpenDialogue(messages, actors);
        }
    }
}
