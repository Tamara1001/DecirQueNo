using System.Collections.Generic;
using UnityEngine;

namespace DecirQueNo
{
    // All messages a run can draw from. Each run picks and shuffles them with its seed.
    [CreateAssetMenu(menuName = "DecirQueNo/Message Pool", fileName = "MessagePool")]
    public class MessagePool : ScriptableObject
    {
        public List<MessageData> messages = new List<MessageData>();
    }
}
