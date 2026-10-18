using System.Text;

namespace DecirQueNo
{
    // Emojis need a font that has them (the default TMP font does not). Until one is added to the project,
    // the screen shows the text without them. The game logic always uses the original text.
    public static class TextDisplay
    {
        public static string ForScreen(string text, bool showEmojis)
        {
            if (showEmojis || string.IsNullOrEmpty(text))
            {
                return text;
            }

            var sb = new StringBuilder(text.Length);
            foreach (char c in text)
            {
                if (!IsEmojiPart(c))
                {
                    sb.Append(c);
                }
            }

            // Collapse the double spaces the removed emojis leave behind.
            string clean = sb.ToString().Trim();
            while (clean.Contains("  "))
            {
                clean = clean.Replace("  ", " ");
            }

            return clean;
        }

        private static bool IsEmojiPart(char c)
        {
            return char.IsSurrogate(c)
                || c == '‍'
                || c == '️'
                || (c >= '←' && c <= '⇿')
                || (c >= '⌀' && c <= '⏿')
                || (c >= '☀' && c <= '➿')
                || (c >= '⬀' && c <= '⯿');
        }
    }
}
