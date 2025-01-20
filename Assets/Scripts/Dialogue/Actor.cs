using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dialogue
{
    [CreateAssetMenu(fileName = "New actor", menuName = "Dialogue/Actor", order = 0)]
    public class Actor : ScriptableObject
    {
        public string actorName;
        public Sprite sprite;
    }
}
