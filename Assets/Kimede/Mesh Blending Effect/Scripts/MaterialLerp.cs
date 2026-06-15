using UnityEngine;
using System;
using System.Collections;
namespace Kimede
{

    public class MaterialLerp
    {
        public static void DoMaterialLerp(MonoBehaviour context, Material mat, string property, float start, float end, float duration = 2f)
        {
            context.StartCoroutine(LerpFloat(mat, property, start, end, duration));
        }

        public static void DoMaterialLerp(MonoBehaviour context, Material mat, string property, int start, int end, float duration = 2f)
        {
            context.StartCoroutine(LerpInt(mat, property, start, end, duration));
        }

        public static void DoMaterialLerp(MonoBehaviour context, Material mat, string property, Color start, Color end, float duration = 2f)
        {
            context.StartCoroutine(LerpColor(mat, property, start, end, duration));
        }

        private static IEnumerator LerpFloat(Material mat, string property, float start, float end, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = t * t * (3f - 2f * t);

                mat.SetFloat(property, Mathf.Lerp(start, end, t));
                yield return null;
            }
            mat.SetFloat(property, end);
        }

        private static IEnumerator LerpInt(Material mat, string property, int start, int end, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = t * t * (3f - 2f * t);

                mat.SetInt(property, Mathf.RoundToInt(Mathf.Lerp(start, end, t)));
                yield return null;
            }
            mat.SetInt(property, end);
        }

        private static IEnumerator LerpColor(Material mat, string property, Color start, Color end, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = t * t * (3f - 2f * t);

                mat.SetColor(property, Color.Lerp(start, end, t));
                yield return null;
            }
            mat.SetColor(property, end);
        }
    }
}
