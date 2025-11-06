using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MonsterCouch.Gameplay
{
    /// <summary>
    /// Handles player movement and collision reporting while respecting on-screen bounds.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField]
        private InputActionReference moveAction = null!;

        [SerializeField]
        [Range(1f, 20f)]
        private float moveSpeed = 6f;

        private Camera _mainCamera = null!;
        private Vector2 _minBounds;
        private Vector2 _maxBounds;
        private Rigidbody2D _rigidbody = null!;
        private Vector2 _currentInput;

        public event Action<EnemyAgent>? EnemyCollided;

        public void Initialize(Camera mainCamera)
        {
            _mainCamera = mainCamera != null
                ? mainCamera
                : throw new ArgumentNullException(nameof(mainCamera));

            CacheBounds();
        }

        private void OnEnable()
        {
            moveAction?.action.Enable();
        }

        private void OnDisable()
        {
            moveAction?.action.Disable();
            _currentInput = Vector2.zero;
        }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _rigidbody.useAutoMass = false;
            _rigidbody.gravityScale = 0f;
            _rigidbody.angularDamping = 0f;
            _rigidbody.linearDamping = 0f;
            _rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
            _rigidbody.bodyType = RigidbodyType2D.Kinematic;
        }

        private void Update()
        {
            if (_mainCamera == null)
            {
                return;
            }

            Vector2 input = moveAction != null ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;
            if (input.sqrMagnitude > 1f)
            {
                input = input.normalized;
            }

            _currentInput = input;
        }

        private void FixedUpdate()
        {
            if (_mainCamera == null || _rigidbody == null)
            {
                return;
            }

            Vector2 currentPosition = _rigidbody.position;
            Vector2 displacement = _currentInput * (moveSpeed * Time.fixedDeltaTime);
            Vector2 targetPosition = ClampToScreenBounds(currentPosition + displacement);
            _rigidbody.MovePosition(targetPosition);
        }

        private void OnTriggerEnter2D(Collider2D other) => NotifyEnemyCollision(other);

        private void OnTriggerStay2D(Collider2D other) => NotifyEnemyCollision(other);

        private void CacheBounds()
        {
            float zOffset = Mathf.Abs(_mainCamera.transform.position.z - transform.position.z);
            Vector3 bottomLeft = _mainCamera.ViewportToWorldPoint(new Vector3(0f, 0f, zOffset));
            Vector3 topRight = _mainCamera.ViewportToWorldPoint(new Vector3(1f, 1f, zOffset));
            _minBounds = new Vector2(bottomLeft.x, bottomLeft.y);
            _maxBounds = new Vector2(topRight.x, topRight.y);
        }

        private Vector2 ClampToScreenBounds(Vector2 position)
        {
            float clampedX = Mathf.Clamp(position.x, _minBounds.x, _maxBounds.x);
            float clampedY = Mathf.Clamp(position.y, _minBounds.y, _maxBounds.y);
            return new Vector2(clampedX, clampedY);
        }

        private void NotifyEnemyCollision(Collider2D other)
        {
            if (!TryFindEnemyAgent(other, out var enemy))
            {
                return;
            }

            EnemyCollided?.Invoke(enemy);
        }

        private static bool TryFindEnemyAgent(Component source, out EnemyAgent enemy)
        {
            if (source.TryGetComponent(out enemy))
            {
                return true;
            }

            enemy = source.GetComponentInParent<EnemyAgent>();
            if (enemy != null)
            {
                return true;
            }

            enemy = source.GetComponentInChildren<EnemyAgent>();
            return enemy != null;
        }
    }
}
