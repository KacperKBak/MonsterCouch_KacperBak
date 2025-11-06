using System;
using System.Collections.Generic;
using UnityEngine;

namespace MonsterCouch.Gameplay
{
    public sealed class EnemyManager : MonoBehaviour
    {
        [SerializeField]
        private EnemyAgent enemyPrefab = null!;

        [SerializeField]
        [Min(1)]
        private int enemyCount = 1000;

        [SerializeField]
        [Range(0.5f, 20f)]
        private float maxSpeed = 6f;

        [SerializeField]
        [Range(0.5f, 30f)]
        private float acceleration = 10f;

        [SerializeField]
        [Range(0f, 1f)]
        private float wanderStrength = 0.35f;

        [SerializeField]
        [Range(0.1f, 10f)]
        private float wanderFrequency = 1.5f;

        [SerializeField]
        [Range(0f, 1f)]
        private float boundaryComfortPadding = 0.2f;

        [SerializeField]
        [Range(0f, 2f)]
        private float spawnPadding = 0.25f;

        private readonly List<EnemyAgent> _enemies = new();
        private PlayerController _player = null!;
        private Camera _camera = null!;
        private Vector2 _minBounds;
        private Vector2 _maxBounds;
        private bool _isInitialized;

        public void Initialize(PlayerController player, Camera camera)
        {
            _player = player ?? throw new ArgumentNullException(nameof(player));
            _camera = camera ?? throw new ArgumentNullException(nameof(camera));

            _player.EnemyCollided += HandleEnemyCollision;
            CacheBounds();
            SpawnEnemiesIfNeeded();
            ResetEnemies();
            _isInitialized = true;
        }

        private void OnDisable()
        {
            if (_player != null)
            {
                _player.EnemyCollided -= HandleEnemyCollision;
            }
        }

        private void Update()
        {
            if (!_isInitialized)
            {
                return;
            }

            Vector3 playerPosition = _player.transform.position;
            float deltaTime = Time.deltaTime;
            float time = Time.time;

            for (int i = 0; i < _enemies.Count; i++)
            {
                EnemyAgent enemy = _enemies[i];
                if (enemy == null || enemy.IsFrozen)
                {
                    continue;
                }

                Vector3 enemyPosition = enemy.transform.position;
                Vector3 direction = enemyPosition - playerPosition;
                Vector2 fleeDirection = direction.sqrMagnitude > Mathf.Epsilon
                    ? ((Vector2)direction).normalized
                    : UnityEngine.Random.insideUnitCircle.normalized;

                Vector2 wanderVector = GetWanderVector(enemy.WanderPhase, time);
                Vector2 boundaryBias = GetBoundaryBias((Vector2)enemyPosition);

                Vector2 combinedDirection = fleeDirection + wanderVector * wanderStrength + boundaryBias;
                if (combinedDirection.sqrMagnitude > 0.001f)
                {
                    combinedDirection.Normalize();
                }

                Vector2 desiredVelocity = combinedDirection * maxSpeed;
                enemy.Velocity = Vector2.MoveTowards(enemy.Velocity, desiredVelocity, acceleration * deltaTime);
                Vector2 newPosition = (Vector2)enemyPosition + enemy.Velocity * deltaTime;
                newPosition = ResolveBounds(newPosition, enemy);
                enemy.transform.position = new Vector3(newPosition.x, newPosition.y, enemy.transform.position.z);
            }
        }

        public void ResetEnemies()
        {
            foreach (EnemyAgent enemy in _enemies)
            {
                if (enemy == null)
                {
                    continue;
                }

                Vector2 spawnPosition = GetRandomSpawnPosition();
                float wanderPhase = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
                enemy.ResetState(spawnPosition, Vector3.one * 0.2f, wanderPhase);
            }
        }

        private void SpawnEnemiesIfNeeded()
        {
            if (enemyPrefab == null)
            {
                throw new InvalidOperationException("Enemy prefab is not assigned.");
            }

            if (_enemies.Count >= enemyCount)
            {
                return;
            }

            int remaining = enemyCount - _enemies.Count;
            for (int i = 0; i < remaining; i++)
            {
                EnemyAgent instance = Instantiate(enemyPrefab, GetRandomSpawnPosition(), Quaternion.identity, transform);
                instance.transform.localScale = Vector3.one * 0.2f;
                instance.SetWanderPhase(UnityEngine.Random.Range(0f, Mathf.PI * 2f));
                instance.SetFrozen(false);
                _enemies.Add(instance);
            }
        }

        private void HandleEnemyCollision(EnemyAgent enemy)
        {
            if (enemy == null)
            {
                return;
            }

            enemy.SetFrozen(true);
        }

        private void CacheBounds()
        {
            float zOffset = Mathf.Abs(_camera.transform.position.z - transform.position.z);
            Vector3 bottomLeft = _camera.ViewportToWorldPoint(new Vector3(0f, 0f, zOffset));
            Vector3 topRight = _camera.ViewportToWorldPoint(new Vector3(1f, 1f, zOffset));
            _minBounds = new Vector2(bottomLeft.x, bottomLeft.y);
            _maxBounds = new Vector2(topRight.x, topRight.y);
        }

        private Vector2 GetRandomSpawnPosition()
        {
            float minX = _minBounds.x + spawnPadding;
            float maxX = _maxBounds.x - spawnPadding;
            float minY = _minBounds.y + spawnPadding;
            float maxY = _maxBounds.y - spawnPadding;

            if (minX > maxX)
            {
                float centerX = (_minBounds.x + _maxBounds.x) * 0.5f;
                minX = maxX = centerX;
            }

            if (minY > maxY)
            {
                float centerY = (_minBounds.y + _maxBounds.y) * 0.5f;
                minY = maxY = centerY;
            }

            float x = UnityEngine.Random.Range(minX, maxX);
            float y = UnityEngine.Random.Range(minY, maxY);
            return new Vector2(x, y);
        }

        private Vector2 GetWanderVector(float phase, float time)
        {
            float angle = phase + time * wanderFrequency;
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }

        private Vector2 GetBoundaryBias(Vector2 position)
        {
            if (boundaryComfortPadding <= 0f)
            {
                return Vector2.zero;
            }

            float comfortX = Mathf.Lerp(_minBounds.x, _maxBounds.x, 0.5f);
            float comfortY = Mathf.Lerp(_minBounds.y, _maxBounds.y, 0.5f);

            Vector2 bias = Vector2.zero;

            float distanceToMinX = position.x - _minBounds.x;
            float distanceToMaxX = _maxBounds.x - position.x;
            float distanceToMinY = position.y - _minBounds.y;
            float distanceToMaxY = _maxBounds.y - position.y;

            if (distanceToMinX < boundaryComfortPadding)
            {
                float strength = 1f - (distanceToMinX / boundaryComfortPadding);
                bias.x += Mathf.Lerp(0f, 1f, strength);
            }
            else if (distanceToMaxX < boundaryComfortPadding)
            {
                float strength = 1f - (distanceToMaxX / boundaryComfortPadding);
                bias.x -= Mathf.Lerp(0f, 1f, strength);
            }

            if (distanceToMinY < boundaryComfortPadding)
            {
                float strength = 1f - (distanceToMinY / boundaryComfortPadding);
                bias.y += Mathf.Lerp(0f, 1f, strength);
            }
            else if (distanceToMaxY < boundaryComfortPadding)
            {
                float strength = 1f - (distanceToMaxY / boundaryComfortPadding);
                bias.y -= Mathf.Lerp(0f, 1f, strength);
            }

            if (bias == Vector2.zero)
            {
                return Vector2.zero;
            }

            Vector2 towardsCenter = new Vector2(comfortX - position.x, comfortY - position.y).normalized;
            bias += towardsCenter * 0.2f;
            return bias.normalized;
        }

        private Vector2 ResolveBounds(Vector2 position, EnemyAgent enemy)
        {
            float minX = _minBounds.x;
            float maxX = _maxBounds.x;
            float minY = _minBounds.y;
            float maxY = _maxBounds.y;

            Vector2 velocity = enemy.Velocity;

            if (position.x <= minX)
            {
                position.x = minX;
                velocity = Vector2.Reflect(velocity, Vector2.right);
            }
            else if (position.x >= maxX)
            {
                position.x = maxX;
                velocity = Vector2.Reflect(velocity, Vector2.left);
            }

            if (position.y <= minY)
            {
                position.y = minY;
                velocity = Vector2.Reflect(velocity, Vector2.up);
            }
            else if (position.y >= maxY)
            {
                position.y = maxY;
                velocity = Vector2.Reflect(velocity, Vector2.down);
            }

            enemy.Velocity = velocity;
            return position;
        }
    }
}
