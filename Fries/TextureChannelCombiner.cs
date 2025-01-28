
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
using Fries.Inspector;
using System;
#endif

namespace Fries {
    public class TextureChannelCombiner : MonoBehaviour {
#if UNITY_EDITOR
        public Texture2D RChannel;
        public Texture2D GChannel;
        public Texture2D BChannel;
        public Texture2D AChannel;
        public string exportPath = "";

        [AButton("Combine")] [IgnoreInInspector]
        public Action combineChannels;
        private void Reset() {
            combineChannels = combine;
        }

        public void combine() {
            // 检查所有纹理是否已赋值
            if (RChannel == null || GChannel == null || BChannel == null || AChannel == null) {
                Debug.LogError("All channel textures must be assigned!");
                return;
            }

            // 验证纹理尺寸一致性
            int width = RChannel.width;
            int height = RChannel.height;
            if (GChannel.width != width || GChannel.height != height ||
                BChannel.width != width || BChannel.height != height ||
                AChannel.width != width || AChannel.height != height) {
                Debug.LogError("All textures must have the same dimensions!");
                return;
            }

            // 创建新纹理
            Texture2D combinedTex = new Texture2D(width, height, TextureFormat.ARGB32, false);

            // 合并通道
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    Color r = RChannel.GetPixel(x, y);
                    Color g = GChannel.GetPixel(x, y);
                    Color b = BChannel.GetPixel(x, y);
                    Color a = AChannel.GetPixel(x, y);
                    
                    combinedTex.SetPixel(x, y, new Color(r.r, g.g, b.b, a.a));
                }
            }
            combinedTex.Apply();

            // 处理保存路径
            if (string.IsNullOrEmpty(exportPath))
                exportPath = "CombinedTexture";

            string fullPath = Path.Combine(Application.dataPath, exportPath + ".png");
            string directory = Path.GetDirectoryName(fullPath);
            
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            // 保存文件
            byte[] pngData = combinedTex.EncodeToPNG();
            File.WriteAllBytes(fullPath, pngData);

            // 刷新资源数据库
            AssetDatabase.Refresh();
            Debug.Log($"Texture saved to: Assets/{exportPath}.png");
        }
#endif
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(TextureChannelCombiner))]
    public class TextureChannelCombinerInspector : AnInspector { }
#endif
}