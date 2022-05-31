using Photon.Pun;
using UnityEngine;

namespace MainGame.PlayerScripts
{
    public class SpectatorMode : MonoBehaviour
    {
        [SerializeField] private Material anonymousMaterial;
        private PhotonView _photonView;

        // Spectator settings
        public bool isSpectatorModeEnabled;
        private int index;

        // Camera references
        private GameObject spectatorCamClone;
        private GameObject spectatorCamHolder;
        private Camera defaultCam;
        [SerializeField] private LayerMask spectatorLayer;
        private Transform chosenPlayer;

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
        
        private void Awake()
        {
            // Scripts
            // _playerAnimation = GetComponent<PlayerAnimation>();
            // _playerLook = GetComponent<PlayerLook>();
            // _playerController = GetComponent<PlayerController>();
            _photonView = GetComponent<PhotonView>();

            // Default cam
            defaultCam = GetComponentInChildren<Camera>();

            // Only works for the local player
            if (!_photonView.IsMine) Destroy(this);
            
            // Enable everything
            Setup();
            
            spectatorCamClone.transform.rotation = Quaternion.identity;
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
            else
            {
                defaultCam.enabled = false;
                spectatorCamClone.SetActive(true);

                foreach (var role in RoomManager.Instance.players)
                {
                    if (role.gameObject.GetComponent<PhotonView>().IsMine) continue;
                    
                    role.gameObject.transform.Find("VillagerRender").GetChild(0).GetComponent<SkinnedMeshRenderer>()
                        .materials[1] = anonymousMaterial;
                }
            }

            // Changes player choice with given input
            UpdateChosenPlayer();

            if (chosenPlayer != null)
            {
                Vector3 offset = Vector3.up * 2 + chosenPlayer.TransformDirection(Vector3.back * 3);
                var chosenPlayerPosition = chosenPlayer.position;
                Vector3 dest = chosenPlayerPosition + offset;

                Vector3 dir = (dest - chosenPlayerPosition);
                if (Physics.Raycast(chosenPlayerPosition,
                        dir,
                        out RaycastHit hit,
                        dir.magnitude,
                        7))
                {
                    dest = hit.point - dir.normalized * 0.5f;
                    spectatorCamHolder.transform.position = dest;
                }
                
                spectatorCamHolder.transform.position = Vector3.Lerp(spectatorCamHolder.transform.position, dest, Time.deltaTime * 2);
                
                spectatorCamHolder.transform.LookAt(chosenPlayerPosition);
                Vector3 rotationEulerAngles = spectatorCamHolder.transform.rotation.eulerAngles;
                rotationEulerAngles.x = 10;
                spectatorCamHolder.transform.rotation = Quaternion.Euler(rotationEulerAngles);
            }
        }

        private void UpdateChosenPlayer()
        {
            var list = RoomManager.Instance.players;
            
            if (list.Count == 0) return;
            
            // TODO use input system instead
            if (Input.GetMouseButtonDown(0))
            {
                index++;
                index %= list.Count;
            }
            else if (Input.GetMouseButtonDown(1))
            {
                index--;
                index += list.Count;
                index %= list.Count;
            }

            chosenPlayer = list[index].gameObject.transform;
        }
    }
}