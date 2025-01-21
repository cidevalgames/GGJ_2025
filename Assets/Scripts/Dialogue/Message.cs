using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dialogue
{
    [CreateAssetMenu(fileName = "New message", menuName = "Dialogue/Message", order = 0)]
    public class Message : ScriptableObject
    {
        public int actorID;
        [TextArea(5, 10)]
        public string text;
    }
}
