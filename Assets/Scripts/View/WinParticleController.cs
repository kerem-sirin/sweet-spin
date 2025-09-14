using UnityEngine;
using System.Collections;

namespace SweetSpin.VFX
{
    /// <summary>
    /// Controls instantiated particle systems for win effects.
    /// Sets color based on payline, waits for completion, then destroys.
    /// </summary>
    public class WinParticleController : MonoBehaviour
    {
        [SerializeField] private ParticleSystem winParticleSystem;

        private float destroyDelay = 0.5f; // Extra safety margin after particles finish

        void Awake()
        {
            if (winParticleSystem == null)
            {
                Debug.LogError("WinParticleController: No ParticleSystem component found!");
                Destroy(gameObject);
                return;
            }
        }

        /// <summary>
        /// Initialize the particle system with a specific color and start playing
        /// </summary>
        public void Initialize(Color frameColor)
        {
            if (winParticleSystem == null) return;

            // Set the start color
            var main = winParticleSystem.main;
            main.startColor = frameColor;

            // Play the particle system
            winParticleSystem.Play();

            // Start destruction coroutine
            StartCoroutine(DestroyWhenComplete());
        }

        /// <summary>
        /// Calculate the total duration of the particle system
        /// </summary>
        private float GetDuration()
        {
            if (winParticleSystem == null) return 0f;

            var main = winParticleSystem.main;

            // Account for lifetime and duration
            float duration = main.duration;
            float lifetime = main.startLifetime.constantMax;

            // If looping is disabled, total time is duration + lifetime
            if (!main.loop)
            {
                return duration + lifetime + destroyDelay;
            }

            // If looping, we shouldn't auto-destroy
            Debug.LogWarning("WinParticleController: Particle system is set to loop. Auto-destroy disabled.");
            return -1f;
        }

        /// <summary>
        /// Wait for particle system to complete, then destroy
        /// </summary>
        private IEnumerator DestroyWhenComplete()
        {
            // Wait while particle system is playing
            while (winParticleSystem != null && winParticleSystem.IsAlive(true))
            {
                yield return null;
            }

            // Add extra delay to ensure all particles have faded
            yield return new WaitForSeconds(GetDuration());

            // Destroy the GameObject
            Destroy(gameObject);
        }
    }
}