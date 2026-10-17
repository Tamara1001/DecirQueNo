using UnityEngine;
using UnityEngine.UI;

namespace DecirQueNo
{
    public class InstructionsUI : MonoBehaviour
    {
        [SerializeField] private Button _startButton;

        private void Start()
        {
            _startButton.onClick.AddListener(GameManager.Instance.StartNewGame);
        }
    }
}
