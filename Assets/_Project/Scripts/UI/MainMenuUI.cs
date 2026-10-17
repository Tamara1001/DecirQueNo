using UnityEngine;
using UnityEngine.UI;

namespace DecirQueNo
{
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private Button _playButton;

        private void Start()
        {
            _playButton.onClick.AddListener(() => GameManager.Instance.LoadScene(GameManager.InstructionsScene));
        }
    }
}
