using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace HelloWorld
{
    public class HelloWorldPlayer : NetworkBehaviour
    {
        public NetworkVariable<float> speed = new NetworkVariable<float>();
        public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
        public NetworkVariable<int> takenMaterialId = new NetworkVariable<int>();
        public SpriteRenderer spriteRenderer = new SpriteRenderer();
        public List<int> takenIds = new List<int>();
        public List<Material> materials = new List<Material>();
        public NetworkVariable<int> conectedPlayers = new NetworkVariable<int>();


        private void Awake()
        {
            if (IsOwner)
            {
                if (NetworkManager.Singleton.IsServer)
                {
                    takenMaterialId.Value = -1;
                }
                else
                {
                    StartTakenMaterialIdServerRpc();
                }
            }
                

        }


        private void Start()
        {
            if (NetworkManager.Singleton.IsServer)
            {
                speed.Value = 2F;
            }
            
        }

        public override void OnNetworkSpawn()
        {
            spriteRenderer = gameObject.GetComponent<SpriteRenderer>();

            if (IsOwner)
            {
                
                if (NetworkManager.Singleton.IsServer)
                {
                    conectedPlayers.Value++;
                }
                else
                {
                    ConectPlayerServerRpc();
                }
                Move();
                SetMaterial();
            }
        }

        public override void OnNetworkDespawn()
        {
            if (NetworkManager.Singleton.IsServer)
            {
                conectedPlayers.Value++;
            }
            else
            {
                DisconectPlayerServerRpc();
            }
        }
        public void Move()
        {
            if (NetworkManager.Singleton.IsServer)
            {
                var randomPosition = GetRandomPositionOnPlane();
                transform.position = randomPosition;
                Position.Value = randomPosition;
            }
            else
            {
                SubmitPositionRequestServerRpc();
            }
        }

        [ServerRpc]
        void SubmitPositionRequestServerRpc(ServerRpcParams rpcParams = default)
        {
            Position.Value = GetRandomPositionOnPlane();
        }

        public void MoveAD(Vector3 direction)
        {
            //Checks if is the owner hwo is trying to move.
            if (IsOwner)
            {
                //If is the owner checks if is the server or one of the clients. If is the server changes his position, if not ask the server to change his position.
                if (NetworkManager.Singleton.IsServer)
                {
                    Position.Value += (direction * speed.Value * Time.deltaTime);
                }
                else
                {
                    SubmitADPositionRequestServerRpc(direction);
                }
            }

        }

        [ServerRpc]
        void SubmitADPositionRequestServerRpc(Vector3 direction, ServerRpcParams rpcParams = default)
        {
            
                Position.Value += (direction * speed.Value * Time.deltaTime);
        }

        static Vector3 GetRandomPositionOnPlane()
        {
            return new Vector3(Random.Range(-3f, 3f), 1f, Random.Range(-3f, 3f));
        }

        public void SetMaterial()
        {
            if (conectedPlayers.Value< 10)
            {
                if (IsOwner)
                {
                    if (NetworkManager.Singleton.IsServer)
                    {
                        do
                        {
                            takenMaterialId.Value = Random.Range(0, materials.Count);

                        } while (takenIds.Contains(takenMaterialId.Value));
                    }
                    else
                    {
                        SubmitSetMaterialRequestServerRpc();
                    }
                }
            }

        }

        [ServerRpc]
        void SubmitSetMaterialRequestServerRpc(ServerRpcParams rpcParams = default)
        {
            if (takenMaterialId.Value != -1)
            {
                takenIds.Remove(takenMaterialId.Value);
            }
            do
            {
                takenMaterialId.Value = Random.Range(0, materials.Count);

            } while (takenIds.Contains(takenMaterialId.Value));
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

        public void UpdateTakenIdsList()
        {
            takenIds = new List<int>();
            foreach (ulong uid in NetworkManager.Singleton.ConnectedClientsIds)
            {
                takenIds.Add(NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(uid).GetComponent<HelloWorldPlayer>().takenMaterialId.Value);
            }
        }

        void Update()
        {
            //Detects the inputs of the keys and sets the movement direction.
            if (Input.GetKey(KeyCode.D))
            { 
                MoveAD(Vector3.right);
            }
            if (Input.GetKey(KeyCode.A))
            {
                MoveAD(-Vector3.right);
            }
            if (Input.GetKey(KeyCode.W))
            {
                MoveAD(Vector3.up);
            }
            if (Input.GetKey(KeyCode.S))
            {
                MoveAD(-Vector3.up);
            }
            transform.position = Position.Value;
            spriteRenderer.material = materials[takenMaterialId.Value];
            UpdateTakenIdsList();

        }
    }
}