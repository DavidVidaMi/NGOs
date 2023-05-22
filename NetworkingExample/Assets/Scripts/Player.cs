using System.Collections;
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

        [ServerRpc]
        void VantageServerRpc(ServerRpcParams rpcParams = default)
        {
            if (IsOwner)
            {
                StartCoroutine(VantageCoroutine());
            }
        }

        [ClientRpc]
        public void AdvantageClientRpc(int framekey, ClientRpcParams clientRpcParams = default)
        {
            VantageServerRpc();
            Debug.Log("Chamado o cliente ");
        }

        [ServerRpc]
        void DisvantageServerRpc(ServerRpcParams rpcParams = default)
        {
            if (IsOwner)
            {
                StartCoroutine(DisvantageCoroutine());
            }
        }

        [ClientRpc]
        public void DisvantageClientRpc(int framekey, ClientRpcParams clientRpcParams = default)
        {
            DisvantageServerRpc();
            Debug.Log("Chamado o cliente ");
        }

        private IEnumerator VantageCoroutine()
        {
            while (true)
            {
                speed.Value *= 2;
                yield return new WaitForSeconds(5f);
            }
        }

        private IEnumerator DisvantageCoroutine()
        {
            while (true)
            {
                speed.Value /= 2;
                yield return new WaitForSeconds(5f);
            }
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