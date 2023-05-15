using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace HelloWorld
{
    public class Player : NetworkBehaviour
    {
        public NetworkVariable<float> speed = new NetworkVariable<float>();
        public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
        public NetworkVariable<int> takenMaterialId = new NetworkVariable<int>();
        public SpriteRenderer spriteRenderer = new SpriteRenderer();
        public List<Material> materials = new List<Material>();
        public NetworkVariable<int> conectedPlayers = new NetworkVariable<int>();


        private void Start()
        {
            if (NetworkManager.Singleton.IsServer)
            {
                speed.Value = 2F;
                Debug.Log(takenMaterialId.Value);
            }
        }

        public override void OnNetworkSpawn()
        {
            spriteRenderer = gameObject.GetComponent<SpriteRenderer>();

            if (IsOwner)
            {
                StartTakenMaterialIdServerRpc();
                Debug.Log(takenMaterialId.Value);
                ConectPlayerServerRpc();
                Move();
                SetMaterial();
            }
        }

        public override void OnNetworkDespawn()
        {
            DisconectPlayerServerRpc();
        }
        public void Move()
        {
            SubmitPositionRequestServerRpc();
        }

        [ServerRpc]
        void SubmitPositionRequestServerRpc(ServerRpcParams rpcParams = default)
        {
            Position.Value = new Vector3(Random.Range(-3f, 3f), 1f, Random.Range(-3f, 3f));
        }

        [ServerRpc]
        public void MoveADRequestServerRpc(Vector3 direction)
        {
                transform.position += (direction * speed.Value * Time.deltaTime);
        }

        public void SetMaterial()
        {
            if (conectedPlayers.Value < materials.Count)
            {
                if (IsOwner)
                {
                    SubmitSetMaterialRequestServerRpc();
                }
            }

        }

        [ServerRpc]
        void SubmitSetMaterialRequestServerRpc(ServerRpcParams rpcParams = default)
        {
            if (takenMaterialId.Value != -1)
            {
                GameManager.instance.takenIds.Remove(takenMaterialId.Value);
            }
            do
            {
                takenMaterialId.Value = Random.Range(0, materials.Count);

           } while (GameManager.instance.takenIds.Contains(takenMaterialId.Value));
            GameManager.instance.UpdateTakenIdsList();
        }

        [ServerRpc]
        void StartTakenMaterialIdServerRpc(ServerRpcParams rpcParams = default)
        {
            takenMaterialId.Value = -1;
        }

        [ServerRpc]
        void DisconectPlayerServerRpc(ServerRpcParams rpcParams = default)
        {
            conectedPlayers.Value--;
        }

        [ServerRpc]
        void ConectPlayerServerRpc(ServerRpcParams rpcParams = default)
        {
            conectedPlayers.Value++;
        }

        

        void Update()
        {
            if (IsOwner)
            {
                //Detects the inputs of the keys and sets the movement direction.
                if (Input.GetKey(KeyCode.D))
                {
                    MoveADRequestServerRpc(Vector3.right);
                }
                if (Input.GetKey(KeyCode.A))
                {
                    MoveADRequestServerRpc(-Vector3.right);
                }
                if (Input.GetKey(KeyCode.W))
                {
                    MoveADRequestServerRpc(Vector3.up);
                }
                if (Input.GetKey(KeyCode.S))
                {
                    MoveADRequestServerRpc(-Vector3.up);
                }
            }
            spriteRenderer.material = materials[takenMaterialId.Value];
        }
    }
}