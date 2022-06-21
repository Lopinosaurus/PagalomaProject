using System.Collections.Generic;
using MainGame.PlayerScripts.Roles;
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MainGame.PlayerScripts
{
    public class SpectatorMode : MonoBehaviour
    {
        // Spectator settings
        public bool isSpectatorModeEnabled;
        [SerializeField] private LayerMask spectatorLayer;
        private PhotonView _photonView;
        private PlayerInput _playerInput;
        private Transform chosenPlayer;
        private Camera defaultCam;
        private int index;

        // Camera references
        private GameObject spectatorCamClone;
        private GameObject spectatorCamHolder;

        private void Awake()
        {
            // Scripts
            // _playerAnimation = GetComponent<PlayerAnimation>();
            // _playerLook = GetComponent<PlayerLook>();
            // _playerController = GetComponent<PlayerController>();
            _photonView = GetComponent<PhotonView>();
            _playerInput = GetComponent<PlayerInput>();

            // Input system
            if (_photonView.IsMine)
            {
                // Default cam
                defaultCam = GetComponent<Role>()._cameraHolder.GetComponentInChildren<Camera>();

                // Enable everything
                Setup();

                // Change left side in the list
                _playerInput.actions["ChangeSpectatorLeft"].performed += ctx =>
                {
                    if (!RoomManager.Instance) return;

                    List<Role> list = RoomManager.Instance.players;

                    if (list.Count > 0 && ctx.ReadValueAsButton())
                    {
                        index++;
                        index %= list.Count;

                        chosenPlayer = list[index].transform;
                    }
                };

                // Change right side in the list
                _playerInput.actions["ChangeSpectatorRight"].performed += ctx =>
                {
                    if (!RoomManager.Instance) return;

                    List<Role> list = RoomManager.Instance.players;

                    if (list.Count > 0 && ctx.ReadValueAsButton())
                    {
                        index--;
                        index += list.Count;
                        index %= list.Count;

                        chosenPlayer = list[index].transform;
                    }
                };

                spectatorCamClone.transform.rotation = Quaternion.identity;
            }
            {
                Destroy(this);
            }
        }

        private void LateUpdate()
        {
            if (!isSpectatorModeEnabled)
            {
                defaultCam.enabled = true;
                spectatorCamClone.SetActive(false);
                spectatorCamHolder.transform.position = transform.position + Vector3.up;
                return;
            }

            defaultCam.enabled = false;
            _playerInput.enabled = true;
            spectatorCamClone.SetActive(true);
            GetComponent<Role>().deathText.enabled = false;

            if (chosenPlayer != null)
            {
                Vector3 offset = Vector3.up * 1 + chosenPlayer.TransformDirection(Vector3.back * 2);
                Vector3 chosenPlayerPosition = chosenPlayer.position + Vector3.up;
                Vector3 dest = chosenPlayerPosition + offset;

                Vector3 dir = dest - chosenPlayerPosition;
                if (Physics.Raycast(chosenPlayerPosition,
                        dir,
                        out RaycastHit hit,
                        dir.magnitude,
                        7))
                {
                    dest = hit.point - dir.normalized * 0.5f;
                    spectatorCamHolder.transform.position = dest;
                }

                spectatorCamHolder.transform.position =
                    Vector3.Lerp(spectatorCamHolder.transform.position, dest, Time.deltaTime * 2);

                spectatorCamHolder.transform.LookAt(chosenPlayerPosition);
                Vector3 rotationEulerAngles = spectatorCamHolder.transform.rotation.eulerAngles;
                rotationEulerAngles.x = 10;
                spectatorCamHolder.transform.rotation = Quaternion.Euler(rotationEulerAngles);
            }
        }

        private void Setup()
        {
            // Spectator cam
            spectatorCamHolder = new GameObject("spectatorCamHolder");
            spectatorCamClone = Instantiate(defaultCam.gameObject, spectatorCamHolder.transform, false);
            spectatorCamClone.layer = (int) Mathf.Log(spectatorLayer, 2);

            spectatorCamClone.SetActive(false);

            spectatorCamClone.GetComponent<Camera>();
            Destroy(spectatorCamClone.GetComponent<AudioListener>());
        }
    }
}