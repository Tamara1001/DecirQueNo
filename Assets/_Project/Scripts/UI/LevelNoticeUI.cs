using System.Collections;
using TMPro;
using UnityEngine;

namespace DecirQueNo
{
    // Tells the player when saying No got easier or harder, which is the point of the game.
    [RequireComponent(typeof(CanvasGroup))]
    public class LevelNoticeUI : MonoBehaviour
    {
        private const float ShowSeconds = 3f;
        private const float FadeSeconds = 0.5f;

        private static readonly Color EasierColor = new Color(0.6f, 1f, 0.7f);
        private static readonly Color HarderColor = new Color(1f, 0.55f, 0.55f);

        [SerializeField] private TMP_Text _text;

        private CanvasGroup _group;
        private PlayerStats _stats;
        private Coroutine _routine;

        private void Awake()
        {
            _group = GetComponent<CanvasGroup>();
            _group.alpha = 0f;
        }

        private void Start()
        {
            GameManager manager = GameManager.Instance;
            manager.EnsureGame();
            _stats = manager.Stats;
            _stats.OnNoLevelChanged += HandleLevelChanged;
        }

        private void OnDestroy()
        {
            if (_stats != null)
            {
                _stats.OnNoLevelChanged -= HandleLevelChanged;
            }
        }

        private void HandleLevelChanged(int oldIndex, int newIndex)
        {
            bool easier = newIndex < oldIndex;
            _text.text = easier ? "Decir que no te cuesta menos" : "Decir que no te cuesta mas";
            _text.color = easier ? EasierColor : HarderColor;

            if (_routine != null)
            {
                StopCoroutine(_routine);
            }

            _routine = StartCoroutine(Show());
        }

        private IEnumerator Show()
        {
            _group.alpha = 1f;
            yield return StartCoroutine(UiFx.Pop(transform, 0.8f, 0.2f));

            float wait = 0f;
            while (wait < ShowSeconds)
            {
                wait += Time.unscaledDeltaTime;
                yield return null;
            }

            float time = 0f;
            while (time < FadeSeconds)
            {
                time += Time.unscaledDeltaTime;
                _group.alpha = 1f - Mathf.Clamp01(time / FadeSeconds);
                yield return null;
            }

            _group.alpha = 0f;
            _routine = null;
        }
    }
}
