using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace JahroConsole.View
{
    internal class TextHeightCaluculator
    {
        private const int MINIMUM_ITEM_SIZE = 20;
        private const int OFFSET_ITEM_MAIN = 10;

        private Dictionary<int, int> cachedTextHeightValues = new Dictionary<int, int>(1000);

        private TextGenerationSettings mainTextSettings;

        private TextGenerationSettings detailsTextSettings;

        private TextGenerator generator;

        private Text mainText;

        private Text detailsText;

        private static TextHeightCaluculator _instance;

        public float defaultWidth;

        private static readonly Regex richTextRegex = new Regex("<.*?>", RegexOptions.Compiled);

        public static TextHeightCaluculator Instance
        {
            get
            {
                if (_instance == null) _instance = new TextHeightCaluculator();
                return _instance;
            }
        }

        public TextHeightCaluculator()
        {
            generator = new TextGenerator();
        }

        public void SetTextComponents(Text mainText, Text detailsText)
        {
            this.mainText = mainText;
            this.detailsText = detailsText;
        }

        public void UpdateReferenceSize(float width)
        {
            defaultWidth = width;
            mainTextSettings = mainText.GetGenerationSettings(new Vector2(defaultWidth, 0));
            mainTextSettings.generateOutOfBounds = false;
            mainTextSettings.richText = true;
            detailsTextSettings = detailsText.GetGenerationSettings(new Vector2(defaultWidth, 0));
            detailsTextSettings.generateOutOfBounds = false;
            detailsTextSettings.richText = false;
            cachedTextHeightValues.Clear();
        }

        public int GetMainTextHeight(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                text = string.Empty;
            }
            int height;

            text = StripRichTextTags(text);
            var textCode = text.GetHashCode();
            if (cachedTextHeightValues.TryGetValue(textCode, out int cachedHeight))
            {
                height = cachedHeight;
            }
            else
            {
                height = Mathf.RoundToInt(generator.GetPreferredHeight(text, mainTextSettings));
                height += OFFSET_ITEM_MAIN;
                height = Mathf.Clamp(height, MINIMUM_ITEM_SIZE, int.MaxValue);
                cachedTextHeightValues.Add(textCode, height);
            }

            return height;
        }

        public int GetDetailsTextHeight(string text)
        {
            int height = Mathf.RoundToInt(generator.GetPreferredHeight(text, detailsTextSettings));
            height += OFFSET_ITEM_MAIN;
            return Mathf.Clamp(height, MINIMUM_ITEM_SIZE, int.MaxValue);
        }

        private string StripRichTextTags(string text)
        {
            return richTextRegex.Replace(text, string.Empty);
        }
    }
}