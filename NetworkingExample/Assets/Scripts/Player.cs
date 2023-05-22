using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace HelloWorld
{
    public class Player : NetworkBehaviour
    {
        public NetworkVariable<float> speed = new NetworkVariable<float>();
        public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
        public NetworkVariable<bool> hasAdvantage = new NetworkVariable<bool>();
        public NetworkVariable<bool> hasDisvantage = new NetworkVariable<bool>();

        private SpriteRenderer spriteRenderer;


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

        [ClientRpc]
        public void AdvantageClientRpc(int framekey, ClientRpcParams clientRpcParams = default)
        {
            if(IsOwner)
                VantageServerRpc();
        }

        [ServerRpc]
        void VantageServerRpc(ServerRpcParams rpcParams = default)
        {
            speed.Value *= 2;
            hasAdvantage.Value = true;
            StartCoroutine(VantageCoroutine());
            
        }
        
        private IEnumerator VantageCoroutine()
        {

            while (hasAdvantage.Value)
            {
                Debug.Log("Esto repítese?");
                yield return new WaitForSeconds(5f);
                speed.Value /= 2;
                hasAdvantage.Value = false;
            }
        }
        [ClientRpc]
        public void DisvantageClientRpc(int framekey, ClientRpcParams clientRpcParams = default)
        {
            if (IsOwner)
                DisvantageServerRpc();
        }

        [ServerRpc]
        void DisvantageServerRpc(ServerRpcParams rpcParams = default)
        {
            speed.Value /= 2;
            hasDisvantage.Value = true;
            StartCoroutine(DisvantageCoroutine());
           
        }

        private IEnumerator DisvantageCoroutine()
        {
            
            while (hasDisvantage.Value)
            {
                yield return new WaitForSeconds(5f);
                speed.Value *= 2;
                hasDisvantage.Value = false;
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
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (hasAdvantage.Value && !hasDisvantage.Value)
            {
                spriteRenderer.color = Color.red;
            }
            else if (hasDisvantage.Value && !hasAdvantage.Value)
            {
                spriteRenderer.color = Color.blue;
            }
            else if (!hasDisvantage.Value && !hasAdvantage.Value)
            {
                spriteRenderer.color = Color.white;
            }
            else
            {
                spriteRenderer.color = Color.green;
            }
        }
    }
}