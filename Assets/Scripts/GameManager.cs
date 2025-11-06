using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace MonsterCouch.Gameplay
{
    public sealed class GameManager : MonoBehaviour
    {
        [SerializeField]
        private PlayerController playerController = null!;

        [SerializeField]
        private EnemyManager enemyManager = null!;

        [SerializeField]
        private Camera mainCamera = null!;

        [SerializeField]
        private string mainMenuSceneName = "MainMenu";

        private void Awake()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main
                    ?? throw new InvalidOperationException("Main camera is required for the gameplay scene.");
            }

            if (playerController == null)
            {
                throw new InvalidOperationException("PlayerController reference is not assigned.");
            }

            if (enemyManager == null)
            {
                throw new InvalidOperationException("EnemyManager reference is not assigned.");
            }

            playerController.Initialize(mainCamera);
            enemyManager.Initialize(playerController, mainCamera);
        }

        private void Update()
        {
            if (Keyboard.current?.escapeKey.wasPressedThisFrame == true)
            {
                SceneManager.LoadScene(mainMenuSceneName);
            }
        }
    }
}
