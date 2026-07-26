// Copyright 2025 AxonGenesis All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if TMPRO_3_OR_NEWER
using TMPro;
#endif

using UnityEngine;

namespace AxonGenesis
{
    /// <summary>
    /// A script to automatically handle the UI panel layout for the Timeflow Demo App.
    /// </summary>
    [ExecuteInEditMode]
    public class DemoPanelLayout : AxonGenesisBehavior
    {
        public DemoPanelContent Content;

        [Header("OFFSETS")]
        public Vector3 SidePlacement = Vector3.one;
        public Vector2 TextRatio = Vector2.one;
        public float TextBaseWidth = 20f;
        public float Depth = 0.5f;
        public float BorderSize = 0.1f;

        [Header("SETUP")]
        public Transform Screen;
        public Transform Border;
        public Transform LeftSide;
        public Transform RightSide;
        public Transform Footer;
        public Transform DemoButton;
        public Transform DimScreen;

#if TMPRO_3_OR_NEWER
        public TextMeshPro Title;
        public TextMeshPro Text;
        public TextMeshProUGUI LoadText;
#endif
        protected override void OnAwake()
        {
            base.OnAwake();
            UpdateLayout();
        }

        private void Update()
        {
            if (!Application.isPlaying) {
                UpdateLayout();
            }
        }

        private void UpdateLayout()
        {
            if (Content == null) {
                return;
            }

            transform.localScale = new Vector3(Content.BaseScale, Content.BaseScale, Content.BaseScale);

            if (Content.Width < 1f) Content.Width = 1f;
            if (Content.Height < 1f) Content.Height = 1f;
            if (Screen != null) {
                Screen.localScale = new Vector3(Content.Width, Content.Height, 1f);
            }
            if (Border != null) {
                float borderWidth = BorderSize;
                float borderHeight = BorderSize;
                Border.localScale = new Vector3(Content.Width + borderWidth, Content.Height + borderHeight, 1f);
            }
            if (Footer != null) {
                Footer.gameObject.SetActive(Content.ShowFooter);
                if (Content.ShowFooter) {
                    Footer.localPosition = new Vector3(0f, -Content.Height + Content.FooterOffset, Depth);
                }
                Footer.localScale = new Vector3(Content.FooterScale, Content.FooterScale, Content.FooterScale);
            }
            if (DemoButton != null) {
                DemoButton.gameObject.SetActive(Content.ShowLoadButton);
            }
            if (DimScreen != null) {
                DimScreen.gameObject.SetActive(Content.DimScreen);
            }
            if (LeftSide != null) {
                LeftSide.localPosition = new Vector3(Content.Width * SidePlacement.x, -Content.Height * SidePlacement.y, SidePlacement.z);
            }
            if (RightSide != null) {
                RightSide.localPosition = new Vector3(-Content.Width * SidePlacement.x, Content.Height * SidePlacement.y, SidePlacement.z);
            }

#if TMPRO_3_OR_NEWER
            if (Title != null) {
                Title.text = Content.TitleText;
                Title.fontSize = Content.TitleFontSize;
                Title.rectTransform.localPosition = new Vector3(0f, Content.TitleOffset, Depth);
                Title.rectTransform.sizeDelta = new Vector2(TextBaseWidth + (Content.Width * TextRatio.x), Content.Height * TextRatio.y);
            }
            if (Text != null) {
                Text.text = Content.BodyText;
                Text.fontSize = Content.BodyFontSize;
                Text.rectTransform.localPosition = new Vector3(0f, Content.BodyOffset, Depth);
                Text.rectTransform.sizeDelta = new Vector2(TextBaseWidth + (Content.Width * TextRatio.x), Content.Height * TextRatio.y);
            }
            if (LoadText != null) {
                LoadText.text = Content.LoadText;
            }
#endif
        }

        public void Close()
        {
            if (DemoAppHome.Home != null) {
                DemoAppHome.Home.ZoomHome();
            }
        }

        public void LoadDemo()
        {
            if (DemoAppHome.Home != null) {
                DemoAppHome.Home.LoadCurrent();
            }
        }

        public void GotoHome()
        {
            if (DemoAppHome.Home != null) {
                DemoAppHome.Home.ZoomHome();
            }
        }

        public void GotoNext()
        {
            if (DemoAppHome.Home != null) {
                DemoAppHome.Home.ZoomNext();
            }
        }

        public void GotoPrev()
        {
            if (DemoAppHome.Home != null) {
                DemoAppHome.Home.ZoomPrev();
            }
        }
    }

}//AxonGenesis
