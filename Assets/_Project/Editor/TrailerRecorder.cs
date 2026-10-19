using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DecirQueNo.EditorTools
{
    // Records a slow, human-looking playthrough to numbered JPG frames (Trailer/frames) plus clip markers
    // (Trailer/markers.txt). It moves the real pointer through the Input System, so hover and press effects
    // happen. The OS cursor is not part of a screenshot, so a copy of the cursor is drawn on top.
    // Menu: DecirQueNo > Trailer > Record gameplay (open MainMenu first).
    public static class TrailerRecorder
    {
        private const string Flag = "TrailerRecorder.Run";

        [MenuItem("DecirQueNo/Trailer/Record gameplay")]
        public static void Record()
        {
            SetGameViewSize(1920, 1080);
            SessionState.SetBool(Flag, true);
            EditorApplication.isPlaying = true;
        }

        // Fixed 1920x1080 Game view so frames are rendered at that size, not upscaled from a small panel.
        private static void SetGameViewSize(int width, int height)
        {
            var flags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic |
                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static;
            System.Type sizesType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizes");
            System.Type singleton = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
            object sizes = singleton.GetProperty("instance").GetValue(null);
            object group = sizesType.GetMethod("GetGroup").Invoke(sizes, new object[] { 0 }); // Standalone
            System.Type groupType = group.GetType();
            int count = (int)groupType.GetMethod("GetTotalCount").Invoke(group, null);
            int index = -1;
            for (int i = 0; i < count; i++)
            {
                object size = groupType.GetMethod("GetGameViewSize").Invoke(group, new object[] { i });
                System.Type st = size.GetType();
                if ((int)st.GetProperty("width").GetValue(size) == width && (int)st.GetProperty("height").GetValue(size) == height &&
                    st.GetProperty("sizeType").GetValue(size).ToString() == "FixedResolution")
                {
                    index = i;
                    break;
                }
            }

            if (index < 0)
            {
                System.Type st = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSize");
                System.Type kind = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizeType");
                object fixedRes = System.Enum.Parse(kind, "FixedResolution");
                object size = System.Activator.CreateInstance(st, new object[] { fixedRes, width, height, "Trailer 1080p" });
                groupType.GetMethod("AddCustomSize").Invoke(group, new[] { size });
                index = count;
            }

            System.Type gameView = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
            EditorWindow window = EditorWindow.GetWindow(gameView);
            gameView.GetMethod("SizeSelectionCallback", flags).Invoke(window, new object[] { index, null });
        }

        [InitializeOnLoadMethod]
        private static void Hook()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Flag, false))
                {
                    SessionState.SetBool(Flag, false);
                    var go = new GameObject("TrailerDriver");
                    Object.DontDestroyOnLoad(go);
                    go.AddComponent<TrailerDriver>();
                }
            };
        }
    }

    public class TrailerDriver : MonoBehaviour
    {
        private const int Fps = 24;
        private const string OutDir = "Trailer";

        private bool _recording;
        private int _frame;
        private string _frames;
        private readonly StringBuilder _markers = new StringBuilder();

        private Vector2 _pos;
        private bool _down;
        private RawImage _cursorImage;
        private Texture2D _arrow;
        private Texture2D _hand;
        private bool _hand_;
        private readonly List<RaycastResult> _hits = new List<RaycastResult>();

        private IEnumerator Start()
        {
            Time.captureFramerate = Fps;
            _frames = Path.Combine(OutDir, "frames");
            if (Directory.Exists(_frames)) Directory.Delete(_frames, true);
            Directory.CreateDirectory(_frames);

            BuildCursor();
            _pos = new Vector2(Screen.width * 0.7f, Screen.height * 0.6f);
            SetMouse();
            yield return null;
            StartCoroutine(CaptureLoop());

            yield return Script();

            File.WriteAllText(Path.Combine(OutDir, "markers.txt"), _markers.ToString());
            Debug.Log($"[Trailer] done, {_frame} frames, {Screen.width}x{Screen.height}");
            Time.captureFramerate = 0;
            EditorApplication.isPlaying = false;
        }

        // ---------------------------------------------------------------- the story

        private IEnumerator Script()
        {
            Mark("menu");
            _recording = true;
            yield return Wait(3.0f);
            yield return ClickButton("PlayButton", "Button");

            Mark("instructions");
            yield return WaitScene(GameManager.InstructionsScene);
            for (int i = 0; i < 14 && Scene() == GameManager.InstructionsScene; i++)
            {
                yield return Wait(Random.Range(1.6f, 2.4f));
                if (Scene() != GameManager.InstructionsScene) break;
                yield return ClickButton("Button");
            }

            Mark("day1");
            yield return WaitScene(GameManager.GameScene);
            yield return Wait(2.5f);
            yield return PlayMessages(2, 0.30f, true);

            // Night after day 1.
            Mark("sleep1");
            GameManager.Instance.EndDay();
            yield return WaitScene(GameManager.SleepScene);
            yield return SleepScreens(GameManager.GameScene, 9);

            // Day 2 with the mind falling apart.
            Mark("day2_low");
            yield return Wait(1.5f);
            GameManager.Instance.Stats.AddHealth(-(GameManager.Instance.Stats.Health - 22));
            yield return Wait(3f);
            yield return PlayMessages(2, 0.85f, false);

            Mark("sleep2_low");
            GameManager.Instance.EndDay();
            yield return WaitScene(GameManager.SleepScene);
            yield return SleepScreens(GameManager.GameScene, 9);

            // Happy ending: fast-forwarded with crafted stats, only the ending is recorded.
            yield return Ending("sad");
            yield return Ending("neutral");
            yield return Ending("happy");
        }

        private IEnumerator Ending(string kind)
        {
            _recording = false;
            GameManager.Instance.BackToMenu();
            yield return WaitScene(GameManager.MainMenuScene);
            yield return Wait(0.5f);
            GameManager.Instance.StartNewGame();
            yield return WaitScene(GameManager.GameScene);
            yield return Wait(0.5f);

            GameManager gm = GameManager.Instance;
            MessageData msg = gm.GetDayMessages(1)[0];
            if (kind == "happy")
            {
                gm.Stats.Apply(PhraseType.No, msg);
                for (int i = 0; i < 5; i++) gm.Stats.Apply(PhraseType.Evasive, msg);
            }
            else if (kind == "neutral")
            {
                for (int i = 0; i < 4; i++) gm.Stats.Apply(PhraseType.Maybe, msg);
                for (int i = 0; i < 2; i++) gm.Stats.Apply(PhraseType.Evasive, msg);
            }
            else
            {
                for (int i = 0; i < 6; i++) gm.Stats.Apply(PhraseType.Yes, msg);
                gm.Stats.AddFollowers(gm.Config.goalFollowers);
            }

            gm.EndDay(true);
            yield return WaitScene(GameManager.SleepScene);
            _recording = true;
            Mark("sleep_" + kind);
            yield return SleepScreens(GameManager.EndingScene, 10);
            Mark("ending_" + kind);
            yield return Wait(14f);
        }

        private IEnumerator SleepScreens(string untilScene, int maxClicks)
        {
            for (int i = 0; i < maxClicks && Scene() != untilScene; i++)
            {
                yield return Wait(Random.Range(2.5f, 3.5f));
                if (Scene() != untilScene) yield return ClickButton("Button");
            }

            yield return WaitScene(untilScene);
        }

        // Opens the next messages and answers them like a hesitant person.
        private IEnumerator PlayMessages(int count, float mistakeChance, bool showReorder)
        {
            for (int n = 0; n < count; n++)
            {
                NotificationItemUI item = null;
                float waited = 0f;
                while (item == null && waited < 60f)
                {
                    item = FindObjectOfType<NotificationItemUI>();
                    if (item != null && item.IsExpired) item = null;
                    yield return Wait(0.2f);
                    waited += 0.2f;
                }

                if (item == null) yield break;
                yield return Wait(Random.Range(1.2f, 2.2f));
                yield return ClickRect((RectTransform)item.transform, () => item.Click(),
                    () => FindObjectOfType<ConversationController>() is { IsOpen: true });
                yield return Wait(Random.Range(2.5f, 3.5f)); // reading the message

                var conversation = FindObjectOfType<ConversationController>();
                var ui = FindObjectOfType<ConversationUI>();
                if (conversation == null || !conversation.IsOpen) continue;

                PhraseType wanted = n == 0 ? PhraseType.Maybe : PhraseType.Evasive;
                PhraseOption option = null;
                foreach (PhraseType type in new[] { wanted, PhraseType.Maybe, PhraseType.Yes })
                {
                    foreach (PhraseOption o in conversation.Bank.Options)
                    {
                        if (o.Type == type) { option = o; break; }
                    }

                    if (option != null) break;
                }

                if (option == null) option = conversation.Bank.Options[0];

                yield return Wait(Random.Range(1.5f, 2.5f)); // thinking
                List<string> words = new List<string>(option.Words);

                // A wrong word first, then take it back.
                if (Random.value < mistakeChance)
                {
                    WordButtonUI wrong = FindWrongButton(ui, words);
                    if (wrong != null)
                    {
                        yield return ClickWord(wrong);
                        yield return Wait(Random.Range(0.8f, 1.4f));
                        conversation.Builder.RemoveAt(conversation.Builder.Count - 1);
                        yield return Wait(0.6f);
                    }
                }

                // Optionally swap the first two words, notice, and fix the order.
                bool swap = showReorder && words.Count >= 3 && n == 0;
                if (swap)
                {
                    (words[0], words[1]) = (words[1], words[0]);
                }

                foreach (string word in words)
                {
                    WordButtonUI button = FindBankButton(ui, word);
                    if (button == null) continue;
                    yield return ClickWord(button);
                    yield return Wait(Random.Range(0.35f, 0.9f));
                }

                if (swap)
                {
                    yield return Wait(1.3f);
                    yield return DragToStart(conversation, ui, words);
                    yield return Wait(0.9f);
                }

                yield return Wait(Random.Range(0.8f, 1.4f));
                if (ui.SendInteractable)
                {
                    yield return ClickRect(FindSendRect(ui), () => ui.ClickSend(), () => !conversation.IsOpen);
                }
                else
                {
                    ui.ClickSend();
                }

                yield return Wait(Random.Range(3.5f, 5f)); // watching the reply
            }
        }

        // ---------------------------------------------------------------- helpers

        // Drags the second chip of the phrase to the left of the first one with the real pointer.
        private IEnumerator DragToStart(ConversationController conversation, ConversationUI ui, List<string> words)
        {
            var chips = new List<WordButtonUI>();
            foreach (WordButtonUI b in ui.GetComponentsInChildren<WordButtonUI>())
            {
                bool inBank = false;
                foreach (WordButtonUI bank in ui.BankButtons) if (bank == b) inBank = true;
                if (!inBank) chips.Add(b);
            }

            chips.Sort((a, b) =>
            {
                Vector2 pa = ScreenPoint((RectTransform)a.transform), pb = ScreenPoint((RectTransform)b.transform);
                int row = Mathf.RoundToInt(pb.y / 10f).CompareTo(Mathf.RoundToInt(pa.y / 10f));
                return row != 0 ? row : pa.x.CompareTo(pb.x);
            });

            if (chips.Count >= 2)
            {
                var first = (RectTransform)chips[0].transform;
                var second = (RectTransform)chips[1].transform;
                Vector2 from = ScreenPoint(second);
                Vector2 firstCenter = ScreenPoint(first);
                Vector2 target = new Vector2(firstCenter.x - first.rect.width * 0.3f * Screen.width / 1920f, firstCenter.y);

                yield return MoveTo(from, Random.Range(0.5f, 0.8f));
                yield return Wait(0.15f);
                _down = true;
                SetMouse();
                yield return Wait(0.2f);
                // a small pull first, then the drag to the new spot
                yield return MoveTo(from + new Vector2(0f, -18f * Screen.height / 1080f), 0.2f);
                yield return MoveTo(target, Random.Range(0.7f, 1.0f));
                yield return Wait(0.2f);
                _down = false;
                SetMouse();
                yield return Wait(0.5f);
            }

            // Fallback so the take is still right if the drag did not land.
            if (conversation.Builder.GetWords()[0] != words[1])
            {
                conversation.Builder.Move(1, 0);
            }
        }

        private static string Scene() => SceneManager.GetActiveScene().name;

        private IEnumerator WaitScene(string scene)
        {
            float t = 0f;
            while (Scene() != scene && t < 40f)
            {
                yield return null;
                t += 1f / Fps;
            }

            yield return Wait(0.4f);
        }

        private IEnumerator Wait(float seconds)
        {
            int frames = Mathf.RoundToInt(seconds * Fps);
            for (int i = 0; i < frames; i++)
            {
                IdleWobble();
                yield return null;
            }
        }

        private static WordButtonUI FindBankButton(ConversationUI ui, string word)
        {
            foreach (WordButtonUI b in ui.BankButtons)
            {
                if (b.Word == word && b.Interactable) return b;
            }

            return null;
        }

        private static WordButtonUI FindWrongButton(ConversationUI ui, List<string> used)
        {
            var candidates = new List<WordButtonUI>();
            foreach (WordButtonUI b in ui.BankButtons)
            {
                if (b.Interactable && !used.Contains(b.Word)) candidates.Add(b);
            }

            return candidates.Count > 0 ? candidates[Random.Range(0, candidates.Count)] : null;
        }

        private static RectTransform FindSendRect(ConversationUI ui)
        {
            var field = typeof(ConversationUI).GetField("_sendButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var button = field?.GetValue(ui) as Button;
            return button != null ? (RectTransform)button.transform : null;
        }

        private IEnumerator ClickWord(WordButtonUI button)
        {
            var conv = FindObjectOfType<ConversationController>();
            int before = conv != null && conv.Builder != null ? conv.Builder.Count : 0;
            yield return ClickRect((RectTransform)button.transform, () => button.Click(),
                () => conv == null || conv.Builder == null || conv.Builder.Count != before);
        }

        private IEnumerator ClickButton(params string[] names)
        {
            foreach (Button b in FindObjectsOfType<Button>())
            {
                foreach (string name in names)
                {
                    if (b.name == name && b.interactable && b.gameObject.activeInHierarchy)
                    {
                        string sceneBefore = Scene();
                        Button target = b;
                        yield return ClickRect((RectTransform)b.transform, () => target.onClick.Invoke(),
                            () => Scene() != sceneBefore || target == null || !target.gameObject.activeInHierarchy);
                        yield break;
                    }
                }
            }
        }

        // Moves to the rect, presses with the real pointer and checks it worked; falls back to the direct call.
        private IEnumerator ClickRect(RectTransform rect, System.Action fallback, System.Func<bool> worked = null)
        {
            if (rect == null) { fallback(); yield break; }
            Vector2 target = ScreenPoint(rect) + new Vector2(Random.Range(-6f, 6f), Random.Range(-5f, 5f));
            yield return MoveTo(target, Random.Range(0.55f, 1.0f));
            yield return Wait(0.12f);
            Scene beforeScene = SceneManager.GetActiveScene();
            _down = true;
            SetMouse();
            yield return Wait(0.12f);
            _down = false;
            SetMouse();
            yield return Wait(0.45f);
            // The real press sometimes does not land (editor focus): do it directly so the take goes on.
            if (worked != null && !worked()) fallback();
            _ = beforeScene;
        }

        private static Vector2 ScreenPoint(RectTransform rect)
        {
            Canvas canvas = rect.GetComponentInParent<Canvas>();
            Camera cam = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            Vector3 world = rect.TransformPoint(rect.rect.center);
            return RectTransformUtility.WorldToScreenPoint(cam, world);
        }

        private IEnumerator MoveTo(Vector2 target, float duration)
        {
            Vector2 start = _pos;
            Vector2 dir = target - start;
            Vector2 normal = new Vector2(-dir.y, dir.x).normalized;
            float bulge = Random.Range(-0.12f, 0.12f) * dir.magnitude;
            int frames = Mathf.Max(2, Mathf.RoundToInt(duration * Fps));
            for (int i = 1; i <= frames; i++)
            {
                float t = i / (float)frames;
                float e = t * t * (3f - 2f * t);
                _pos = start + dir * e + normal * (bulge * Mathf.Sin(Mathf.PI * e));
                SetMouse();
                yield return null;
            }
        }

        private void IdleWobble()
        {
            _pos += new Vector2(Random.Range(-0.6f, 0.6f), Random.Range(-0.6f, 0.6f));
            SetMouse();
        }

        private void SetMouse()
        {
            if (Mouse.current == null) return;
            var state = new MouseState { position = _pos };
            if (_down) state = state.WithButton(MouseButton.Left);
            InputState.Change(Mouse.current, state);
            UpdateCursorVisual();
        }

        // ---------------------------------------------------------------- cursor overlay

        private void BuildCursor()
        {
            _arrow = Resources.Load<Texture2D>("UI/Cursor/cursor_arrow");
            _hand = Resources.Load<Texture2D>("UI/Cursor/cursor_hand");
            var canvasGo = new GameObject("TrailerCursorCanvas", typeof(Canvas));
            canvasGo.transform.SetParent(transform);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32000;
            var imgGo = new GameObject("Cursor", typeof(RectTransform), typeof(RawImage));
            imgGo.transform.SetParent(canvasGo.transform, false);
            _cursorImage = imgGo.GetComponent<RawImage>();
            _cursorImage.raycastTarget = false;
            _cursorImage.texture = _arrow;
            var rt = (RectTransform)imgGo.transform;
            rt.anchorMin = rt.anchorMax = Vector2.zero;
            rt.sizeDelta = new Vector2(64f, 64f) * Mathf.Max(1f, Screen.height / 1080f);
        }

        private void UpdateCursorVisual()
        {
            if (_cursorImage == null) return;
            bool hand = IsOverButton();
            _cursorImage.texture = hand ? _hand : _arrow;
            var rt = (RectTransform)_cursorImage.transform;
            Vector2 hot = hand ? new Vector2(19f, 5f) : new Vector2(19f, 13f);
            rt.pivot = new Vector2(hot.x / 64f, 1f - hot.y / 64f);
            rt.position = _pos;
        }

        private bool IsOverButton()
        {
            if (EventSystem.current == null) return false;
            var data = new PointerEventData(EventSystem.current) { position = _pos };
            _hits.Clear();
            EventSystem.current.RaycastAll(data, _hits);
            foreach (RaycastResult hit in _hits)
            {
                var s = hit.gameObject.GetComponentInParent<Selectable>();
                if (s != null) return s.IsInteractable();
            }

            return false;
        }

        // ---------------------------------------------------------------- capture

        private void Mark(string name)
        {
            _markers.AppendLine($"{name} {_frame}"); File.WriteAllText(Path.Combine(OutDir, "markers.txt"), _markers.ToString());
        }

        private IEnumerator CaptureLoop()
        {
            var end = new WaitForEndOfFrame();
            while (true)
            {
                yield return end;
                if (!_recording) continue;
                Texture2D tex = ScreenCapture.CaptureScreenshotAsTexture();
                File.WriteAllBytes(Path.Combine(_frames, $"f{_frame:D6}.jpg"), tex.EncodeToJPG(90));
                Destroy(tex);
                _frame++;
            }
        }
    }
}
