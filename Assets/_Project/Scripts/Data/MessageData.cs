using System.Collections.Generic;
using UnityEngine;

namespace DecirQueNo
{
    [CreateAssetMenu(menuName = "DecirQueNo/Message", fileName = "Message_")]
    public class MessageData : ScriptableObject
    {
        public string sender = "Nombre_1";
        public string timeAgo = "Hace 1";
        [TextArea] public string body = "Hacé misión_1";

        [Header("Responses")]
        [Tooltip("Yes phrase. One word.")]
        public List<string> yesWords = new List<string> { "Si" };

        [Tooltip("Maybe phrase (hints at yes). 2 to 4 words, varies per message.")]
        public List<string> maybeWords = new List<string> { "Tal", "vez" };

        [Tooltip("Long No phrase. The current No level uses the first N words (see GameConfig).")]
        public List<string> noWords = new List<string>();

        [Header("Insistence")]
        [Tooltip("Insistence level. Raised when the player says No to this sender.")]
        public int pressure;

        [Tooltip("Multiplies the global effects of GameConfig for this message.")]
        public float effectMultiplier = 1f;

        [Tooltip("Message queued when the player says No. Optional.")]
        public MessageData insistence;
    }
}
