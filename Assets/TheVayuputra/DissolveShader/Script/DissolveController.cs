using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System;

namespace TheVayuputra
{
    public class DissolveController : MonoBehaviour
    {
        private static readonly int DissolveAmountId = Shader.PropertyToID("_Cutoff");
        private static readonly int EdgeColorId = Shader.PropertyToID("_Edge_Color");
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        private const float VisibleValue = 1f;
        private const float DissolvedValue = 0f;

        public Renderer[] targetRenderers;

        [SerializeField] private float duration = 1f;
        [SerializeField] private Ease dissolveEase = Ease.Linear;

        public ParticleSystem dissolveParticleSystem;
        public ParticleSystem reverseDissolveParticleSystem;

        [SerializeField] private Transform findRenderersOverrideParent;

        private Material[] materials;
        private Sequence currentTweenSequence;

        private void Awake()
        {
            if (targetRenderers == null || targetRenderers.Length == 0)
                FindAllRenderers();
            else
                InitializeRenderers();
        }

        private void OnValidate()
        {
            if (targetRenderers == null || targetRenderers.Length == 0)
                FindAllRenderers();
        }

        [Gaskellgames.Button]
        private void FindAllRenderers()
        {
            Transform parentTransform =
                findRenderersOverrideParent != null
                    ? findRenderersOverrideParent
                    : transform;

            targetRenderers =
                parentTransform.GetComponentsInChildren<Renderer>(true);

            InitializeRenderers();
        }

        private void InitializeRenderers()
        {
            if (targetRenderers == null || targetRenderers.Length == 0)
                return;

            List<Material> allMaterials = new();

            foreach (var targetRenderer in targetRenderers)
            {
                if (targetRenderer == null)
                    continue;

                foreach (var mat in targetRenderer.materials)
                {
                    if (mat == null || !mat.HasProperty(DissolveAmountId))
                        continue;

                    mat.SetFloat(DissolveAmountId, VisibleValue);
                    allMaterials.Add(mat);
                }
            }

            materials = allMaterials.ToArray();
        }

        /// <summary>
        /// Trackable by MMF_Function.
        /// MMF_Function passes its own CancellationToken.
        /// </summary>
        public async UniTask PlayDissolve(CancellationToken cancellationToken = default)
        {
            PlayDissolveParticles();

            await StartDissolve(
                DissolvedValue,
                cancellationToken);
        }

        /// <summary>
        /// Trackable by MMF_Function.
        /// </summary>
        public async UniTask ReverseDissolve(CancellationToken cancellationToken = default)
        {
            PlayReverseDissolveParticles();

            await StartDissolve(
                VisibleValue,
                cancellationToken);
        }

        /// <summary>
        /// Instantaneous function.
        /// </summary>
        public void SetMaterial(Material newMaterial)
        {
            if (newMaterial == null ||
                targetRenderers == null ||
                targetRenderers.Length == 0)
            {
                return;
            }

            List<Material> allMaterials = new();

            for (int i = 0; i < targetRenderers.Length; i++)
            {
                Renderer targetRenderer = targetRenderers[i];

                if (targetRenderer == null)
                    continue;

                Material[] newMaterialsArray =
                    new Material[targetRenderer.sharedMaterials.Length];

                for (int j = 0; j < newMaterialsArray.Length; j++)
                {
                    newMaterialsArray[j] = newMaterial;
                }

                targetRenderer.materials = newMaterialsArray;

                foreach (var mat in targetRenderer.materials)
                {
                    if (mat == null)
                        continue;

                    allMaterials.Add(mat);

                    if (mat.HasProperty(DissolveAmountId))
                        mat.SetFloat(DissolveAmountId, VisibleValue);
                }
            }

            materials = allMaterials.ToArray();
        }

        // ============================================================
        // DISSOLVE
        // ============================================================

        private async UniTask StartDissolve(
            float targetValue,
            CancellationToken cancellationToken)
        {
            if (materials == null || materials.Length == 0)
                return;

            // Cancel/kill previous dissolve.
            if (currentTweenSequence != null &&
                currentTweenSequence.IsActive())
            {
                currentTweenSequence.Kill();
            }

            currentTweenSequence = DOTween.Sequence();

            foreach (var material in materials)
            {
                _ = currentTweenSequence.Join(
                    material.DOFloat(
                        targetValue,
                        DissolveAmountId,
                        duration)
                    .SetEase(dissolveEase)
                );
            }

            try
            {
                // Wait until this specific sequence finishes,
                // or until MMF cancels this invocation.
                await currentTweenSequence
                    .WithCancellation(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // MMF_Function requested cancellation.
                if (currentTweenSequence != null &&
                    currentTweenSequence.IsActive())
                {
                    currentTweenSequence.Kill();
                }

                throw;
            }
        }

        // ============================================================
        // PARTICLES
        // ============================================================

        private void PlayDissolveParticles()
        {
            if (dissolveParticleSystem == null)
                return;

            dissolveParticleSystem.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear);

            dissolveParticleSystem.Play(true);
        }

        private void PlayReverseDissolveParticles()
        {
            if (reverseDissolveParticleSystem == null)
                return;

            reverseDissolveParticleSystem.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear);

            reverseDissolveParticleSystem.Play(true);
        }

        // ============================================================
        // CLEANUP
        // ============================================================

        private void OnDestroy()
        {
            if (currentTweenSequence != null &&
                currentTweenSequence.IsActive())
            {
                currentTweenSequence.Kill();
            }
        }
    }
}
