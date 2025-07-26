using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

namespace HelpSystem.Markdown {
    public static class MarkdownRenderer {
        private static Dictionary<string, Texture2D> _imageCache = new Dictionary<string, Texture2D>();

        public static void Render(string markdownContent) {
            string[] lines = markdownContent.Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.None);

            foreach (string line in lines) {
                string processedLine = line;

                // Headings: # Heading 1, ## Heading 2, etc.
                Match headingMatch = Regex.Match(processedLine, @"^\s*(#{1,6})\s+(.+)$");
                if (headingMatch.Success) {
                    int level = headingMatch.Groups[1].Value.Length; // 几级标题
                    string headingText = headingMatch.Groups[2].Value; // 实际文字

                    // 根据级别获取一个 GUIStyle（可自行缓存提高效率）
                    GUIStyle headingStyle = GetHeadingStyle(level);

                    EditorGUILayout.LabelField(headingText, headingStyle);
                    continue; // 本行已经渲染完，处理下一行
                }
                
                var m = Regex.Match(line,
                    @"^\s*\[Button:\s*""(?<text>[^""]*)""\s*,\s*/(?<cmd>[^\]]+)\]\s*$");

                if (m.Success) {
                    string text = m.Groups["text"].Value; // "Button Text"
                    string command = m.Groups["cmd"].Value.Trim(); // InspectorHighlight S1 Fingerprint2 TTEST sm
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(text, GUILayout.MinWidth(EditorGUIUtility.currentViewWidth * 0.8f))) {
                        btnInfo.command?.Execute();
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    processedLine = "";
                }

                // Bold: **text** or __text__
                processedLine = Regex.Replace(processedLine, @"\*\*(.*?)\*\*", "<b>$1</b>");
                processedLine = Regex.Replace(processedLine, @"__(.*?)__", "<b>$1</b>");

                // Italic: *text* or _text_
                processedLine = Regex.Replace(processedLine, @"\*(.*?)\*", "<i>$1</i>");
                processedLine = Regex.Replace(processedLine, @"_(.*?)_", "<i>$1</i>");

                // Images: ![alt text](path/to/image.png)
                MatchCollection imageMatches = Regex.Matches(processedLine, @"!\[.*?\]\((.*?)\)");
                foreach (Match match in imageMatches) {
                    string imagePath = match.Groups[1].Value;
                    Texture2D image = GetImage(imagePath);
                    if (image) {
                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        GUILayout.Label(image, GUILayout.MaxWidth(EditorGUIUtility.currentViewWidth * 0.8f),
                            GUILayout.MaxHeight(EditorGUIUtility.currentViewWidth * 0.8f));
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                    }

                    // Remove image markdown from the line so it's not rendered as text
                    processedLine = processedLine.Replace(match.Value, "");
                }

                // Render the processed text line
                if (!string.IsNullOrWhiteSpace(processedLine)) {
                    EditorGUILayout.LabelField(processedLine, GetRichTextStyle());
                }
            }
        }

        private static Texture2D GetImage(string path) {
            if (_imageCache.ContainsKey(path)) {
                return _imageCache[path];
            }

            Texture2D image = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (image) {
                _imageCache.Add(path, image);
            }
            else {
                Debug.LogWarning($"Image not found at path: {path}");
            }

            return image;
        }

        private static GUIStyle GetRichTextStyle() {
            GUIStyle style = new GUIStyle(EditorStyles.label);
            style.richText = true;
            style.wordWrap = true;
            return style;
        }

        public static void ClearCache() {
            _imageCache.Clear();
        }

        private static GUIStyle GetHeadingStyle(int level) {
            // 先复制一份基础样式
            GUIStyle style = new GUIStyle(EditorStyles.boldLabel) {
                wordWrap = true,
                richText = true
            };

            // 不同级别做简单区分（可按需微调）
            switch (level) {
                case 1:
                    style.fontSize = 20;
                    style.margin = new RectOffset(0, 0, 6, 4);
                    break;
                case 2:
                    style.fontSize = 18;
                    style.margin = new RectOffset(0, 0, 6, 4);
                    break;
                case 3:
                    style.fontSize = 16;
                    style.margin = new RectOffset(0, 0, 4, 2);
                    break;
                default: // 4‑6 级
                    style.fontSize = 14;
                    style.margin = new RectOffset(0, 0, 2, 2);
                    break;
            }

            return style;
        }
    }
}