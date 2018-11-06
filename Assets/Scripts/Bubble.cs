using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    //every bubble in our game
    class Bubble : MonoBehaviour
    {
        public Material OriginalBubbleMaterial { get; private set; } //cache the original color

        private Color _color = Color.white;

        public int column, row;

        public Color color
        {
            get { return _color; }
            set
            {
                _color = value;
                AnimateAppear(1);
            }
        }

        void Start()
        {

            GetComponent<Image>().material = Instantiate<Material>(GetComponent<Image>().material);
            AnimateAppear(1);
        }

        void AnimateAppear(float value)
        {
            float[] alphas = { 1 * value, 1.0f * value, 0, 0 };
            float[] alphaAnchors = { 0, 0.3f * value, 0.574f * value, 1 * value };
            Render(alphas, alphaAnchors, this.color, value);
        }

        void Render(float[] alphas, float[] alphaAnchors, Color c, float value = 1)
        {
            Color[] cores = { c, c, c };

            float[] colorAnchors = { 0, 0.5f * value, 1 * value };

            Material m = gameObject.GetComponent<Image>().material;

            m.SetColorArray("_Colors", cores);
            m.SetFloatArray("_ColorAnchors", colorAnchors);
            m.SetFloatArray("_Alphas", alphas);
            m.SetFloatArray("_AlphaAnchors", alphaAnchors);
            m.SetInt("_ColorCount", cores.Length);
            m.SetInt("_AlphaCount", alphas.Length);
        }

    }
}
