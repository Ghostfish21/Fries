
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
using Fries.Inspector;
using System;
using System.Collections.Generic;
#endif

namespace Fries {
    public class TextureChannelCombiner : MonoBehaviour {
#if UNITY_EDITOR
        public SelectableInputType RChannel;
        public SelectableInputType GChannel;
        public SelectableInputType BChannel;
        public SelectableInputType AChannel;

        [IgnoreInInspector]
        public Texture2D RChannelT2d;
        [IgnoreInInspector]
        public float RChannelFloat;
        [IgnoreInInspector]
        public Texture2D GChannelT2d;
        [IgnoreInInspector]
        public float GChannelFloat;
        [IgnoreInInspector]
        public Texture2D BChannelT2d;
        [IgnoreInInspector]
        public float BChannelFloat;
        [IgnoreInInspector]
        public Texture2D AChannelT2d;
        [IgnoreInInspector]
        public float AChannelFloat;

        public string exportPath = "";

        [AButton("Combine")] [IgnoreInInspector]
        public Action combineChannels;
        private void Reset() {
            combineChannels = combine;

            if (RChannel == null)
                RChannel = new SelectableInputType(this) {
                    inputTypes = new List<string> { "Texture2D", "Float" },
                    inputProperties = new List<string> { "RChannelT2d", "RChannelFloat" }
                };
            if (GChannel == null)
                GChannel = new SelectableInputType(this) {
                    inputTypes = new List<string> { "Texture2D", "Float" },
                    inputProperties = new List<string> { "GChannelT2d", "GChannelFloat" }
                };
            if (BChannel == null)
                BChannel = new SelectableInputType(this) {  
                    inputTypes = new List<string> { "Texture2D", "Float" },
                    inputProperties = new List<string> { "BChannelT2d", "BChannelFloat" }
                };
            if (AChannel == null)
                AChannel = new SelectableInputType(this) {
                    inputTypes = new List<string> { "Texture2D", "Float" },
                    inputProperties = new List<string> { "AChannelT2d", "AChannelFloat" }
                };
            
        }

        private List<Texture2D> texturesToCheck = new List<Texture2D>();

        public void combine() {
            // 验证纹理尺寸一致性，如果用的是float就不检查，用的是Texture2D就检查
            texturesToCheck.Clear();
            if (RChannel.getSelectedType() == "Texture2D") texturesToCheck.Add(RChannel.getValue() as Texture2D);
            if (GChannel.getSelectedType() == "Texture2D") texturesToCheck.Add(GChannel.getValue() as Texture2D);
            if (BChannel.getSelectedType() == "Texture2D") texturesToCheck.Add(BChannel.getValue() as Texture2D);
            if (AChannel.getSelectedType() == "Texture2D") texturesToCheck.Add(AChannel.getValue() as Texture2D);

            foreach (var texture in texturesToCheck) {
                if (texture == null) {
                    Debug.LogError("All textures must be assigned!");
                    return;
                }
            }

            int width = 1;
            int height = 1;
            if (texturesToCheck.Count != 0) {
                width = texturesToCheck[0].width;
                height = texturesToCheck[0].height;
                foreach (var texture in texturesToCheck) {
                    if (texture.width != width || texture.height != height) {
                        Debug.LogError("All textures must have the same dimensions!");
                        return;
                    }
                }
            }

            // 创建新纹理
            Texture2D combinedTex = new Texture2D(width, height, TextureFormat.ARGB32, false);

            Color getColor(SelectableInputType channel, int x, int y) {
                if (channel.getSelectedType() == "Texture2D") return (channel.getValue() as Texture2D).GetPixel(x, y);
                else {
                    float f = (float) channel.getValue();
                    return new Color(f, f, f, f);
                }
            }

            // 合并通道
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    Color r = getColor(RChannel, x, y);
                    Color g = getColor(GChannel, x, y);
                    Color b = getColor(BChannel, x, y);
                    Color a = getColor(AChannel, x, y);
                    
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