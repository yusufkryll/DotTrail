using System;
using Ra.Trail;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Ra.Subtitles
{
    public class SubtitleMaker : TrailObject<SubtitleMaker>
    {
        private Text textElement;
        private double typeWriterDuration = 0;

        public SubtitleMaker Configure(Text _textElement)
        {
            textElement = _textElement;
            return this;
        }
        
        public SubtitleMaker Show(string text)
        {
            After(() =>
            {
                textElement.text = ""; 
            });
            var typedText = text;
            While(() =>
            {
                if (typeWriterDuration is 0)
                {
                    textElement.text = text;
                    typedText = "";
                    return false;
                }
                if (typedText.Length <= 0) return false;
                textElement.text += typedText[0];
                typedText = typedText.Remove(0, 1);
                return true;
            }, () => typeWriterDuration);
            return this;
        }

        public SubtitleMaker ShowData(DefaultAsset dataAsset)
        {
            var dataPath = Application.dataPath;
            var path = dataPath.Remove(dataPath.LastIndexOf("Assets", StringComparison.Ordinal)) 
                       + AssetDatabase.GetAssetPath(dataAsset);
            path = path.Replace("/", "\\");
            var parser = new SubtitleParser();
            var contents = parser.Parse(path);
            SubtitleItem lastContent = null;
            After(() => textElement.enabled = true);
            foreach (var srtContent in contents)
            {
                if (lastContent != null)
                {
                    var emptyTime = (srtContent.StartOffset - lastContent.EndOffset).TotalSeconds;
                    if (emptyTime > 0.2f)
                    {
                        Wait(emptyTime / 2);
                        After(() => textElement.enabled = false);
                        Wait(emptyTime / 2);
                    }
                    else Wait(emptyTime);
                    After(() => textElement.enabled = true);    
                }
                Show(srtContent.Text);
                Wait((srtContent.EndOffset - srtContent.StartOffset).TotalSeconds);
                lastContent = srtContent;
            }
            After(() => textElement.enabled = false);
            return this;
        }
        
        public SubtitleMaker ShowList(params object[] list)
        {
            foreach (var el in list)
            {
                switch (el)
                {
                    case string text:
                        Show(text);
                        Wait();
                        break;
                    case int duration:
                        Wait(duration);
                        break;
                    case double duration:    
                        Wait(duration);
                        break;
                    case float duration:    
                        Wait(duration);
                        break;
                }
            }
            return this;
        }

        public SubtitleMaker SetTypewriter(double waitDuration)
        {
            After(() =>
            {
                typeWriterDuration = waitDuration;
            });
            return this;
        }
    }
}
