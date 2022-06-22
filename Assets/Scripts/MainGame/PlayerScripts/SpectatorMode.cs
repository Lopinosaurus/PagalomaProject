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
        private Transform _chosenPlayer;
        private Camera _defaultCam;
        private int _index;

        // Camera references
        private GameObject _spectatorCamClone;
        private GameObject _spectatorCamHolder;

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
                _defaultCam = GetComponentInChildren<Camera>();

                // Enable everything
                Setup();

                // Change left side in the list
                _playerInput.actions["ChangeSpectatorLeft"].performed += ctx =>
                {
                    if (!RoomManager.Instance) return;

                    List<Role> list = RoomManager.Instance.players;

                    if (list.Count > 0 && ctx.ReadValueAsButton())
                    {
                        _index++;
                        _index %= list.Count;

                        _chosenPlayer = list[_index].transform;
                    }
                };

                // Change right side in the list
                _playerInput.actions["ChangeSpectatorRight"].performed += ctx =>
                {
                    if (!RoomManager.Instance) return;

                    List<Role> list = RoomManager.Instance.players;

                    if (list.Count > 0 && ctx.ReadValueAsButton())
                    {
                        _index--;
                        _index += list.Count;
                        _index %= list.Count;

                        _chosenPlayer = list[_index].transform;
                    }
                };

                _spectatorCamClone.transform.rotation = Quaternion.identity;
            }
            {
                Destroy(this);
            }
        }

        private void LateUpdate()
        {
            if (!isSpectatorModeEnabled)
            {
                _defaultCam.enabled = true;
                _spectatorCamClone.SetActive(false);
                _spectatorCamHolder.transform.position = transform.position + Vector3.up;
                return;
            }

            _defaultCam.enabled = false;
            _playerInput.enabled = true;
            _spectatorCamClone.SetActive(true);
            GetComponent<Role>().deathText.enabled = false;

            if (_chosenPlayer != null)
            {
                Vector3 offset = Vector3.up * 1 + _chosenPlayer.TransformDirection(Vector3.back * 2);
                Vector3 chosenPlayerPosition = _chosenPlayer.position + Vector3.up;
                Vector3 dest = chosenPlayerPosition + offset;

                Vector3 dir = dest - chosenPlayerPosition;
                if (Physics.Raycast(chosenPlayerPosition,
                        dir,
                        out RaycastHit hit,
                        dir.magnitude,
                        7))
                {
                    dest = hit.point - dir.normalized * 0.5f;
                    _spectatorCamHolder.transform.position = dest;
                }

                _spectatorCamHolder.transform.position =
                    Vector3.Lerp(_spectatorCamHolder.transform.position, dest, Time.deltaTime * 2);

                _spectatorCamHolder.transform.LookAt(chosenPlayerPosition);
                Vector3 rotationEulerAngles = _spectatorCamHolder.transform.rotation.eulerAngles;
                rotationEulerAngles.x = 10;
                _spectatorCamHolder.transform.rotation = Quaternion.Euler(rotationEulerAngles);
            }
        }

        private void Setup()
        {
            // Spectator cam
            _spectatorCamHolder = new GameObject("spectatorCamHolder");
            _spectatorCamClone = Instantiate(_defaultCam.gameObject, _spectatorCamHolder.transform, false);
            _spectatorCamClone.layer = (int) Mathf.Log(spectatorLayer, 2);

            _spectatorCamClone.SetActive(false);

            _spectatorCamClone.GetComponent<Camera>();
            Destroy(_spectatorCamClone.GetComponent<AudioListener>());
        }
    }
}