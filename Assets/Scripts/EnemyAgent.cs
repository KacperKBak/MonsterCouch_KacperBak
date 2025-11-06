using UnityEngine;

namespace MonsterCouch.Gameplay
{
    /// <summary>
    /// Lightweight marker component for enemies along with their runtime state.
    /// </summary>
    public sealed class EnemyAgent : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer spriteRenderer = null!;

        public bool IsFrozen { get; private set; }
        public Vector2 Velocity { get; set; }
        public float WanderPhase { get; private set; }

        private void Awake()
        {
            if (spriteRenderer == null)
            {
                TryGetComponent(out spriteRenderer);
            }
        }

        public void ResetState(Vector2 position, Vector3 scale, float wanderPhase)
        {
            transform.position = position;
            transform.localScale = scale;
            WanderPhase = wanderPhase;
            Velocity = Vector2.zero;
            SetFrozen(false);
        }

        public void SetWanderPhase(float phase)
        {
            WanderPhase = phase;
        }

        public void SetFrozen(bool isFrozen)
        {
            IsFrozen = isFrozen;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = isFrozen ? new Color(0.8f, 0.8f, 0.8f, 1f) : Color.red;
            }

            if (isFrozen)
            {
                Velocity = Vector2.zero;
            }
        }
    }
}
