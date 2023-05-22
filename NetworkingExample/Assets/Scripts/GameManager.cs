using System.Collections.Generic;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace HelloWorld
{
    public class GameManager : NetworkBehaviour
    {
        public static GameManager instance;
        public List<int> takenIds;

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 300));
            if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            {
                StartButtons();
            }
            else
            {
                StatusLabels();

                SubmitNewPosition();
            }

            GUILayout.EndArea();
        }

        private void Awake()
        {
            takenIds = new List<int>();
            instance = this;
        }

        void StartButtons()
        {
            if (GUILayout.Button("Host"))
            {
                NetworkManager.Singleton.StartHost();
                StartCoroutine(VantageAdvantage());
            }
            if (GUILayout.Button("Client")) NetworkManager.Singleton.StartClient();
        }

        static void StatusLabels()
        {
            var mode = NetworkManager.Singleton.IsHost ? "Host" : "Client";

            GUILayout.Label("Transport: " +
                NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);
            GUILayout.Label("Mode: " + mode);
        }

        static void SubmitNewPosition()
        {
            if (GUILayout.Button(NetworkManager.Singleton.IsServer ? "Move" : "Request Position Change"))
            {
                var playerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
                var player = playerObject.GetComponent<Player>();
                player.Move();
            }
        }

        IEnumerator VantageAdvantage()
        {
            while(true)
            {
                float chanceVA = Random.Range(0f, 1f);
                if (chanceVA < 0.2f)
                {
                    NetworkClient client = NetworkManager.Singleton.ConnectedClientsList[Random.Range(0, NetworkManager.Singleton.ConnectedClientsIds.Count)];
                    //O send solo se utiliza para cando queres chamar a varios clientes a partir de unha lista de clientes
                    /*ClientRpcParams clientRpcParams = new ClientRpcParams
                    {
                        Send = new ClientRpcSendParams
                        {
                            TargetClientIds = new ulong[] { client.PlayerObject.NetworkObjectId }
                        }
                    };*/
                    //Para chamar a un en concreto sería así
                    float VorA = Random.Range(0f, 1f);
                    if (VorA < 0.5f)
                    {
                        client.PlayerObject.GetComponent<Player>().AdvantageClientRpc(4);
                    }
                    else
                    {
                        client.PlayerObject.GetComponent<Player>().DisvantageClientRpc(4);
                    }
                        
                }
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
}