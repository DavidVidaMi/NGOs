using Unity.Netcode;
using UnityEngine;

namespace HelloWorld
{
    public class HelloWorldPlayer : NetworkBehaviour
    {
        public NetworkVariable<float> speed = new NetworkVariable<float>();
        public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();

        private void Start()
        {
            speed.Value = 2F;
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
        void SubmitPositionRequestServerRpc(ServerRpcParams rpcParams = default)
        {
            Position.Value = GetRandomPositionOnPlane();
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
        }
    }
}