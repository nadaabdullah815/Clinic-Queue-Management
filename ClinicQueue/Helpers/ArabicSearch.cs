using System.Text;

namespace ClinicQueue.Helpers;

public static class ArabicSearch
{
    // يحوّل نص البحث إلى نمط LIKE يتجاهل الفروق بين أشكال الحروف العربية المتشابهة
    public static string ToLikePattern(string input)
    {
        var sb = new StringBuilder("%");
        var lastWasSpace = false;

        foreach (var ch in input.Trim())
        {
            // تجاهل التشكيل والتطويل
            if ((ch >= '\u064B' && ch <= '\u0652') || ch == '\u0640') continue;

            // توحيد المسافات المتكررة
            if (char.IsWhiteSpace(ch))
            {
                if (!lastWasSpace) sb.Append(' ');
                lastWasSpace = true;
                continue;
            }
            lastWasSpace = false;

            switch (ch)
            {
                case 'ا': case 'أ': case 'إ': case 'آ': sb.Append("[اأإآ]"); break;
                case 'ي': case 'ى': sb.Append("[يى]"); break;
                case 'ة': case 'ه': sb.Append("[ةه]"); break;

                // حروف خاصة بـ LIKE نهرّبها لتُعامل كنص عادي
                case '%': sb.Append("[%]"); break;
                case '_': sb.Append("[_]"); break;
                case '[': sb.Append("[[]"); break;

                default: sb.Append(ch); break;
            }
        }

        return sb.Append('%').ToString();
    }
}