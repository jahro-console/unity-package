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
        private const float CACHE_INVALIDATION_THRESHOLD = 0.01f; // 1% width change invalidates cache

        private Dictionary<int, int> cachedTextHeightValues = new Dictionary<int, int>(1000);
        private Dictionary<string, string> strippedTextCache = new Dictionary<string, string>(500);
        private float lastCachedWidth = -1f;

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
            if (lastCachedWidth > 0f && Mathf.Abs(width - lastCachedWidth) / lastCachedWidth < CACHE_INVALIDATION_THRESHOLD)
            {
                return;
            }

            defaultWidth = width;
            mainTextSettings = mainText.GetGenerationSettings(new Vector2(defaultWidth, 0));
            mainTextSettings.generateOutOfBounds = false;
            mainTextSettings.richText = true;
            detailsTextSettings = detailsText.GetGenerationSettings(new Vector2(defaultWidth, 0));
            detailsTextSettings.generateOutOfBounds = false;
            detailsTextSettings.richText = false;

            if (lastCachedWidth > 0f)
            {
                cachedTextHeightValues.Clear();
                strippedTextCache.Clear();
            }

            lastCachedWidth = width;
        }

        public int GetMainTextHeight(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return MINIMUM_ITEM_SIZE;
            }

            // Use cached stripped text to avoid repeated regex processing
            if (!strippedTextCache.TryGetValue(text, out string strippedText))
            {
                strippedText = StripRichTextTags(text);
                strippedTextCache[text] = strippedText;
            }

            var textCode = strippedText.GetHashCode();
            if (cachedTextHeightValues.TryGetValue(textCode, out int cachedHeight))
            {
                return cachedHeight;
            }

            int height = Mathf.RoundToInt(generator.GetPreferredHeight(strippedText, mainTextSettings));
            height += OFFSET_ITEM_MAIN;
            height = Mathf.Clamp(height, MINIMUM_ITEM_SIZE, int.MaxValue);
            cachedTextHeightValues.Add(textCode, height);

            return height;
        }

        public void ClearCache()
        {
            cachedTextHeightValues.Clear();
            strippedTextCache.Clear();
        }

        public bool ShouldInvalidateCache(float currentWidth)
        {
            return lastCachedWidth <= 0f || Mathf.Abs(currentWidth - lastCachedWidth) / lastCachedWidth >= CACHE_INVALIDATION_THRESHOLD;
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