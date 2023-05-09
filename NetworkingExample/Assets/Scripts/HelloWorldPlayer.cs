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
                //for (int i = 0; i <= 9; i++)
                //{
                //    materials.Add(Resources.Load("Material " + i, typeof(Material)) as Material);
                //    Debug.Log(materials[i]);
                //}

                Move();
                SetMaterial();
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

        private void SetMaterial()
        {
            if (takenIds.Count < 10)
            {
                if (IsOwner)
                {
                    if (NetworkManager.Singleton.IsServer)
                    {
                        do
                        {
                            takenMaterialId.Value = Random.Range(0, materials.Count);

                        } while (takenIds.Contains(takenMaterialId.Value));
                        UpdateTakenIdsList();
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
            do
            {
                takenMaterialId.Value = Random.Range(0, materials.Count);

            } while (takenIds.Contains(takenMaterialId.Value));
            Debug.Log("Hola son Client");
            UpdateTakenIdsList();
        }

        public void UpdateTakenIdsList()
        {
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
        }
    }
}