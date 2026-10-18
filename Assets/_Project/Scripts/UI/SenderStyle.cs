using UnityEngine;

namespace DecirQueNo
{
    // Notification banner of each sender and the text color that reads well on it.
    public static class SenderStyle
    {
        private static readonly Color Navy = new Color(0.02f, 0.125f, 0.24f);

        public static Sprite Banner(string sender)
        {
            return Resources.Load<Sprite>("UI/" + BannerName(sender));
        }

        public static Color BannerText(string sender)
        {
            // Orchid is the only light banner.
            return BannerName(sender) == "Banner_Valerie" ? Navy : Color.white;
        }

        private static string BannerName(string sender)
        {
            string key = sender != null ? sender.ToLowerInvariant() : "";
            if (key.Contains("vip") || key.Contains("contacto"))
            {
                return "Banner_Vip";
            }

            return key.Contains("christian") || key.Contains("cristian") ? "Banner_Cristian" : "Banner_Valerie";
        }
    }
}
