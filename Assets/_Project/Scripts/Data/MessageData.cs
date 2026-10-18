using System.Collections.Generic;
using UnityEngine;

namespace DecirQueNo
{
    // One tree of the script: an incoming message, the phrases the player can build and the sender's reply to each.
    // Phrases are lists of fragments (1 to 3 words each). The word bank is the union of all phrases.
    [CreateAssetMenu(menuName = "DecirQueNo/Message", fileName = "Message_")]
    public class MessageData : ScriptableObject
    {
        public string sender = "Valerie";
        [TextArea] public string body = "";

        [Header("Script")]
        [Tooltip("Antagonist level, 1 to 3. Picks the effects from GameConfig.levels.")]
        public int level = 1;

        [Tooltip("The last message of the run: it is the only one with a real No.")]
        public bool isFinal;

        [Header("Phrases (fragments, in order)")]
        [Tooltip("Complying answer (the checkmark in the board).")]
        public List<string> yesWords = new List<string>();

        [Tooltip("Evasive answer, the 'almost a no' (the siren in the board). From level 2 it is the lukewarm No.")]
        public List<string> evasiveWords = new List<string>();

        [Tooltip("Real No. Only the final message has one.")]
        public List<string> noWords = new List<string>();

        [Header("Replies of the sender")]
        [TextArea] public string yesReply = "";
        [TextArea] public string evasiveReply = "";
        [TextArea] public string noReply = "";

        [Header("Validation")]
        [Tooltip("Fragments exactly as written in the Miro board. Used by tests to spot banks that cannot be built.")]
        public List<string> sourceBank = new List<string>();

        public IReadOnlyList<string> GetWords(PhraseType type)
        {
            switch (type)
            {
                case PhraseType.Yes: return yesWords;
                case PhraseType.Maybe: return evasiveWords;
                default: return noWords;
            }
        }

        public string GetReply(PhraseType type)
        {
            switch (type)
            {
                case PhraseType.Yes: return yesReply;
                case PhraseType.Maybe: return evasiveReply;
                default: return noReply;
            }
        }
    }
}
