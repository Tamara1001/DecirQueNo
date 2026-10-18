using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DecirQueNo
{
    // Opening story and tutorial, one slide at a time. Everything is built in code on top of the room
    // background: Sam in the mirror on the left and a card with the text on the right.
    public class InstructionsUI : MonoBehaviour
    {
        private class Slide
        {
            public string Title;
            public string Body;
            public string Sam;

            // Optional examples of the word blocks: a label and the blocks of one phrase.
            public (string Label, string[] Blocks)[] Examples;
        }

        private static readonly Slide[] Slides =
        {
            new Slide
            {
                Title = "Eres Sam",
                Body = "Tu cuarto es pequeño, tus días son idénticos y tu mayor terror es volverte invisible.",
                Sam = "Sam_Feliz_Dia",
            },
            new Slide
            {
                Title = "Hasta esta noche",
                Body = "Llevas meses subiendo videos a una audiencia que casi no te registra...\nhasta esta noche.",
                Sam = "Sam_Feliz_Dia",
            },
            new Slide
            {
                Title = "Una notificación",
                Body = "Una notificación acaba de iluminar tu pantalla.\nAlguien con millones de ojos encima sabe quién eres y te está ofreciendo una salida hacia la cima.",
                Sam = "Sam_Blush_Dia",
            },
            new Slide
            {
                Title = "La meta",
                Body = "La meta parece simple: alcanzar 1.000.000 de seguidores.\nEl problema nunca fue llegar. El problema es lo que estarás dispuesta a conceder para no caerte en el camino.",
                Sam = "Sam_Triste_Dia",
            },
            new Slide
            {
                Title = "Cómo se juega",
                Body = "Te llegan notificaciones en el celular. Abre una y arma tu respuesta tocando los bloques de texto.\nEl tiempo corre desde que abres la conversación. Si no abres un mensaje a tiempo, desaparece y pierdes seguidores.",
                Sam = "Sam_Feliz_Dia",
                Examples = new[] { ("", new[] { "Sí,", "ya", "bajo." }) },
            },
            new Slide
            {
                Title = "Arma tu frase",
                Body = "Toca un bloque para agregarlo a tu frase.\nToca de nuevo un bloque de tu frase para quitarlo.\nTambién puedes arrastrar los bloques para ordenarlos como quieras, o sacarlos de la frase arrastrándolos afuera.",
                Sam = "Sam_Feliz_Dia",
                Examples = new[] { ("", new[] { "ya", "Sí,", "bajo." }), ("", new[] { "Sí,", "ya", "bajo." }) },
            },
            new Slide
            {
                Title = "El precio de ceder",
                Body = "Complacerlos es el camino rápido: pocos bloques y más seguidores, pero también más ansiedad.\nUn \"tal vez\" va a medias.\nDecir \"NO\" exige más esfuerzo: proteges tu mente, pero pierdes números.",
                Sam = "Sam_Blush_Dia",
                Examples = new[]
                {
                    ("Sí", new[] { "Sí,", "ya", "bajo." }),
                    ("Tal vez", new[] { "No sé", "si", "debería." }),
                    ("No", new[] { "No voy", "a bajar,", "vete", "de aquí", "ahora." }),
                },
            },
            new Slide
            {
                Title = "Elige bien",
                Body = "Cuanto más cedes, más ansiedad: los bloques se vuelven más chicos y se mezclan, y decir que no cuesta cada vez más.\nElige bien qué vas a sacrificar.",
                Sam = "Sam_Triste_Dia",
            },
        };

        private static readonly Color Navy = UiSprites.Navy;
        private static readonly Color Lilac = new Color(0.98f, 0.8f, 0.96f);
        private static readonly Color Pink = new Color(0.98f, 0.67f, 0.94f);
        private static readonly Color Accent = new Color(0.62f, 0.25f, 0.62f);
        private static readonly Color ChipColor = new Color(0.56f, 0.76f, 0.84f);

        private const float FadeSeconds = 0.18f;

        [SerializeField] private Button _startButton;
        [SerializeField] private Sprite _mirrorFrame;

        private Image _mirror;
        private TMP_Text _title;
        private TMP_Text _body;
        private RectTransform _examples;
        private CanvasGroup _content;
        private Button _backButton;
        private TMP_Text _nextLabel;
        private readonly List<Image> _dots = new List<Image>();
        private TMP_FontAsset _font;
        private int _index;
        private bool _changing;

        private void Start()
        {
            _font = _startButton.GetComponentInChildren<TMP_Text>().font;

            // The old single block of text is replaced by the slides.
            HideOld("TextPanel");
            HideOld("Text");

            BuildMirror();
            BuildCard();
            Show(0, false);
        }

        private void HideOld(string name)
        {
            Transform old = transform.Find(name);
            if (old != null)
            {
                old.gameObject.SetActive(false);
            }
        }

        // Same mirror as the Sleep scene.
        private void BuildMirror()
        {
            var sam = new GameObject("Mirror", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            sam.transform.SetParent(transform, false);
            var rect = (RectTransform)sam.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(-288f, -610f);
            rect.sizeDelta = new Vector2(636f, 900f);
            _mirror = sam.GetComponent<Image>();
            _mirror.preserveAspect = true;
            _mirror.raycastTarget = false;

            if (_mirrorFrame != null)
            {
                var frame = new GameObject("MirrorFrame", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                frame.transform.SetParent(transform, false);
                Stretch((RectTransform)frame.transform, 0f);
                var image = frame.GetComponent<Image>();
                image.sprite = _mirrorFrame;
                image.raycastTarget = false;
            }
        }

        private void BuildCard()
        {
            Image border = NewImage("Card", transform, Navy);
            var card = (RectTransform)border.transform;
            card.anchorMin = card.anchorMax = new Vector2(0.5f, 0.5f);
            card.anchoredPosition = new Vector2(560f, 0f);
            card.sizeDelta = new Vector2(780f, 820f);
            UiSprites.Round(border, 46f);

            Image fill = NewImage("Fill", card, Lilac);
            Stretch((RectTransform)fill.transform, 8f);
            UiSprites.Round(fill, 38f);

            // Everything that changes from slide to slide lives here so it can fade as one.
            var content = new GameObject("Content", typeof(RectTransform), typeof(CanvasGroup));
            content.transform.SetParent(card, false);
            Stretch((RectTransform)content.transform, 0f);
            _content = content.GetComponent<CanvasGroup>();

            _title = NewText("Title", content.transform, 66f, FontStyles.Bold, Accent);
            Place(_title.rectTransform, 0.05f, 0.95f, 0.85f, 0.97f);

            _body = NewText("Body", content.transform, 38f, FontStyles.Normal, Navy);
            _body.enableAutoSizing = true;
            _body.fontSizeMin = 28f;
            _body.fontSizeMax = 44f;
            _body.alignment = TextAlignmentOptions.Top;
            _body.lineSpacing = 12f;
            Place(_body.rectTransform, 0.07f, 0.93f, 0.47f, 0.84f);

            var examples = new GameObject("Examples", typeof(RectTransform), typeof(VerticalLayoutGroup));
            examples.transform.SetParent(content.transform, false);
            _examples = (RectTransform)examples.transform;
            Place(_examples, 0.04f, 0.96f, 0.235f, 0.47f);
            var layout = examples.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            BuildDots(card);
            BuildButtons(card);
        }

        private void BuildDots(Transform card)
        {
            var row = new GameObject("Dots", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(card, false);
            Place((RectTransform)row.transform, 0.3f, 0.7f, 0.175f, 0.225f);
            var layout = row.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 10f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            for (int i = 0; i < Slides.Length; i++)
            {
                Image dot = NewImage("Dot", row.transform, Navy);
                ((RectTransform)dot.transform).sizeDelta = new Vector2(18f, 18f);
                UiSprites.Round(dot, 9f);
                _dots.Add(dot);
            }
        }

        private void BuildButtons(Transform card)
        {
            // The scene's start button becomes "Siguiente / Empezar"; a copy is the back button.
            _startButton.onClick.RemoveAllListeners();
            _startButton.transform.SetParent(card, false);
            UiSprites.CardButton(_startButton, Pink);
            Place((RectTransform)_startButton.transform, 0.52f, 0.95f, 0.035f, 0.155f);
            _startButton.onClick.AddListener(Next);
            _nextLabel = _startButton.GetComponentInChildren<TMP_Text>();

            _backButton = Instantiate(_startButton, card);
            _backButton.name = "BackButton";
            _backButton.onClick.RemoveAllListeners();
            _backButton.onClick.AddListener(Previous);
            Place((RectTransform)_backButton.transform, 0.05f, 0.38f, 0.035f, 0.155f);
            _backButton.GetComponentInChildren<TMP_Text>().text = "Atrás";
            UiSprites.CardButton(_backButton, Lilac);
        }

        private void Next()
        {
            if (_changing)
            {
                return;
            }

            if (_index >= Slides.Length - 1)
            {
                GameManager.Instance.StartNewGame();
                return;
            }

            StartCoroutine(Change(_index + 1));
        }

        private void Previous()
        {
            if (!_changing && _index > 0)
            {
                StartCoroutine(Change(_index - 1));
            }
        }

        private IEnumerator Change(int index)
        {
            _changing = true;
            yield return Fade(1f, 0f);
            Show(index, true);
            yield return Fade(0f, 1f);
            _changing = false;
        }

        private IEnumerator Fade(float from, float to)
        {
            float time = 0f;
            while (time < FadeSeconds)
            {
                time += Time.unscaledDeltaTime;
                _content.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(time / FadeSeconds));
                yield return null;
            }

            _content.alpha = to;
        }

        private void Show(int index, bool keepAlpha)
        {
            _index = index;
            Slide slide = Slides[index];
            _title.text = slide.Title;
            _body.text = slide.Body;
            _mirror.sprite = Resources.Load<Sprite>("Characters/" + slide.Sam);
            _backButton.gameObject.SetActive(index > 0);
            _nextLabel.text = index >= Slides.Length - 1 ? "Empezar" : "Siguiente";

            for (int i = 0; i < _dots.Count; i++)
            {
                Color color = i == index ? Accent : new Color(Navy.r, Navy.g, Navy.b, 0.3f);
                _dots[i].color = color;
            }

            BuildExamples(slide);

            // Without examples the text takes the whole card and sits in the middle.
            bool hasExamples = slide.Examples != null;
            Place(_body.rectTransform, 0.07f, 0.93f, hasExamples ? 0.5f : 0.25f, 0.84f);
            _body.alignment = hasExamples ? TextAlignmentOptions.Top : TextAlignmentOptions.Center;
            if (!keepAlpha)
            {
                _content.alpha = 1f;
            }
        }

        private void BuildExamples(Slide slide)
        {
            for (int i = _examples.childCount - 1; i >= 0; i--)
            {
                Destroy(_examples.GetChild(i).gameObject);
            }

            if (slide.Examples == null)
            {
                return;
            }

            foreach ((string label, string[] blocks) in slide.Examples)
            {
                var row = new GameObject("Row", typeof(RectTransform), typeof(HorizontalLayoutGroup));
                row.transform.SetParent(_examples, false);
                var layout = row.GetComponent<HorizontalLayoutGroup>();
                layout.spacing = 10f;
                layout.childAlignment = TextAnchor.MiddleCenter;
                layout.childControlWidth = true;
                layout.childControlHeight = true;
                layout.childForceExpandWidth = false;
                layout.childForceExpandHeight = false;

                if (!string.IsNullOrEmpty(label))
                {
                    TMP_Text tag = NewText("Label", row.transform, 26f, FontStyles.Bold, Accent);
                    tag.alignment = TextAlignmentOptions.MidlineRight;
                    tag.gameObject.AddComponent<LayoutElement>().minWidth = 120f;
                    tag.text = label;
                }

                foreach (string block in blocks)
                {
                    BuildChip(row.transform, block);
                }
            }
        }

        private void BuildChip(Transform parent, string text)
        {
            var chip = new GameObject("Chip", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(HorizontalLayoutGroup));
            chip.transform.SetParent(parent, false);
            var image = chip.GetComponent<Image>();
            image.color = ChipColor;
            image.raycastTarget = false;
            UiSprites.Round(image, 16f);

            var layout = chip.GetComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 6, 8);
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            TMP_Text label = NewText("Text", chip.transform, 26f, FontStyles.Normal, Navy);
            label.enableWordWrapping = false;
            label.text = text;
        }

        private TMP_Text NewText(string name, Transform parent, float size, FontStyles style, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var label = go.GetComponent<TextMeshProUGUI>();
            label.font = _font;
            label.fontSize = size;
            label.fontStyle = style;
            label.color = color;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
            return label;
        }

        private static Image NewImage(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static void Place(RectTransform rect, float xMin, float xMax, float yMin, float yMax)
        {
            rect.anchorMin = new Vector2(xMin, yMin);
            rect.anchorMax = new Vector2(xMax, yMax);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void Stretch(RectTransform rect, float inset)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(inset, inset);
            rect.offsetMax = new Vector2(-inset, -inset);
        }
    }
}
