using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace HelloWorld
{
    public class Player : NetworkBehaviour
    {
        public NetworkVariable<float> speed = new NetworkVariable<float>();
        public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();


        private void Start()
        {
            if (NetworkManager.Singleton.IsServer)
            {
                speed.Value = 2F;
            }
        }

        public override void OnNetworkSpawn()
        {

            if (IsOwner)
            {
                Move();
            }

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

        [ServerRpc]
        public void ShackeRequestServerRpc()
        {
            transform.position = new Vector3(transform.position.x, 0f + Mathf.PingPong(Time.time*25f, 1), transform.position.z);
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
                if (Input.GetKey(KeyCode.Space))
                {
                    ShackeRequestServerRpc();
                }
            }
        }
    }
}