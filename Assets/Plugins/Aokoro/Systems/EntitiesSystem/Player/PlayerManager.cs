using Aokoro.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using NaughtyAttributes;

namespace Aokoro.Entities.Player
{
    public class PlayerManager : Entity
    {
        public event Action<string, string> OnMapChange;
        public event Action OnRespawn;

        [HideInInspector]
        public PlayerInput playerInput;
        public string DefaultActionMap;

        [HideInInspector]
        public Animator anim;
        [HideInInspector]
        public Rigidbody rb;
        [Space]
        [ReadOnly, BoxGroup("DEBUG")]
        public InputActionMap currentMap;
        private AudioListener audioListener;
        public AudioListener AudioListener
        {
            get
            {
                if (audioListener == null)
                    audioListener = Camera.main.GetComponent<AudioListener>();

                return audioListener;
            }
        }

        public static PlayerManager LocalPlayer
        {
            get
            {
                if (localPlayer == null)
                {
                    GameObject player = GameObject.FindGameObjectWithTag("Player");
                    if (player != null)
                        localPlayer = player.GetComponent<PlayerManager>();

                }

                return localPlayer;
            }
        }
        private static PlayerManager localPlayer;


        protected override void Awake()
        {
            if (LocalPlayer != this)
            {
                Destroy(gameObject);
                return;
            }

            playerInput = GetComponent<PlayerInput>();
            
            anim = GetComponent<Animator>();
            rb = GetComponent<Rigidbody>();

            SetupCursorForPlayer();
        }

        protected virtual void SetupCursorForPlayer()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public virtual void OnAwake()
        {
            SetupInputs();
            Initiate<PlayerManager>();
        }

        protected virtual void Start()
        {
            ChangeActionMap();
            playerInput.currentActionMap?.Enable();
        }

        protected void SetupInputs()
        {
            playerInput.actions = GenerateInputActionAsset();
            playerInput.ActivateInput();
        }

        protected virtual InputActionAsset GenerateInputActionAsset()
        {
            var asset = ScriptableObject.CreateInstance<InputActionAsset>();
            IPlayerInputAssetProvider[] inputProviders = GetComponentsInChildren<IPlayerInputAssetProvider>();

            for (int i = 0; i < inputProviders.Length; i++)
            {
                IPlayerInputAssetProvider inputProvider = inputProviders[i];
                InputActionAsset subAsset = inputProvider.ActionAsset;

                //ControlSchemes
                foreach (var scheme in subAsset.controlSchemes)
                {
                    if (!asset.FindControlScheme(scheme.name).HasValue)
                        asset.AddControlScheme(scheme);
                }

                foreach (InputActionMap map in subAsset.actionMaps)
                {
                    InputActionMap mapCopy = map.Clone();
                    mapCopy.Disable();
                    asset.AddActionMap(mapCopy);
                }
            }

            for (int i = 0; i < inputProviders.Length; i++)
                inputProviders[i].BindToNewActions(asset);

            asset.Enable();
            return asset;
        }

        public void Respawn(Vector3 position, Quaternion rotation)
        {
            rb.position = position;
            rb.rotation = rotation;
            OnRespawn?.Invoke();
        }

        public void ChangeActionMap() => ChangeActionMap(DefaultActionMap);

        public void ChangeActionMap(string targetMap)
        {
            if (playerInput.actions != null)
            {
                if (playerInput.actions.FindActionMap(targetMap) != null)
                {
                    InputActionMap lastActionMap = playerInput.currentActionMap;
                    lastActionMap?.Disable();

                    if (lastActionMap != null)
                    {
                        string currentMap = lastActionMap.name;
                        OnMapChange?.Invoke(currentMap, targetMap);
                    }

                    playerInput.SwitchCurrentActionMap(targetMap);
                    currentMap = playerInput.currentActionMap;
                }
            }
        }
    }
}
