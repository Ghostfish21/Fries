using UnityEngine;
using UnityEngine.UI;

namespace Fries.Chat.Ui {
    public class TransparencyPropComm : CommandBase {
        private ChatCore.Writer writer;
        private Image image;
        
        public TransparencyPropComm(Image image) : base("chatTransparency", "chattransparency") {
            writer = ChatCore.create("ChatboxSetTransparency");
            this.image = image;
        }

        protected override void execute(string senderId, string[] args) {
            float transparency = float.Parse(args[0]);
            if (transparency > 1) transparency /= 255f;
            
            if (transparency is < 0 or > 1) {
                writer.write("Chatbox transparent value must be between 0 and 1 or between 0 and 255!");
                return;
            }

            PlayerPrefs.SetFloat("ChatTransparency", transparency);
            image.color = new Color(image.color.r, image.color.g, image.color.b, transparency);
            writer.write("Chatbox transparency set to " + transparency + "!");
        }
    }
}