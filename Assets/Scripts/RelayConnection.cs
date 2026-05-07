using UnityEngine;
using TMPro;
using Unity.Netcode;
using Unity.Services.Relay;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay; // ¡Librería requerida en Unity 6!

public class RelayConnection : MonoBehaviour
{
    [SerializeField]
    private TMP_Text code;
    [SerializeField]
    private TMP_Text codeInput;

    async void Start()
    {
        await Unity.Services.Core.UnityServices.InitializeAsync();

        // Es buena práctica comprobar si ya estás logueado antes de intentar hacerlo de nuevo
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    public async void StartRelay()
    {
        string joinCode = await StartHost_(4);
        code.text = joinCode;
    }

    public async void JoinRelay()
    {
        // MUY IMPORTANTE: TextMeshPro suele añadir un espacio en blanco invisible al final 
        // del texto (Zero Width Space). El .Trim() evita que Relay rechace el código.
        string cleanCode = codeInput.text.Trim();
        await StartClient_(cleanCode);
    }

    async Task<string> StartHost_(int maxConnections)
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log("Join code: " + joinCode);

            // ---> NUEVO MÉTODO PARA UNITY 6 <---
            // Convierte el Allocation en RelayServerData usando el protocolo "dtls" (seguro)
            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost();
            return joinCode;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Error al crear el Host del Relay: {e.Message}");
            return null;
        }
    }

    async Task<bool> StartClient_(string joinCode)
    {
        try
        {
            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            // ---> NUEVO MÉTODO PARA UNITY 6 <---
            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            // Iniciar el cliente y devolver el resultado (eliminé la llamada duplicada que tenías)
            return NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Error al unirse al Relay: {e.Message}");
            return false;
        }
    }
}