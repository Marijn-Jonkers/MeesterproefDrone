using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using System.Net.WebSockets;

public class WebSocketClient : MonoBehaviour
{
    private ClientWebSocket webSocket;
    private Uri serverUri = new Uri("ws://localhost:8080");
    private CancellationTokenSource cts;
    private CommandListener commandListener;

    void Start()
    {
        commandListener = GetComponent<CommandListener>();
        ConnectToWebSocketServer();
    }

    // Connect to the WebSocket server and begin receiving messages
    async void ConnectToWebSocketServer()
    {
        webSocket = new ClientWebSocket();
        cts = new CancellationTokenSource();

        try
        {
            await webSocket.ConnectAsync(serverUri, cts.Token);
            Debug.Log("Connected to WebSocket server.");

            // Start listening for messages in the background
            ReceiveMessagesAsync();
        }
        catch (Exception e)
        {
            Debug.LogError($"WebSocket connection failed: {e.Message}");
        }
    }

    // Asynchronously receive messages from the WebSocket server
    async void ReceiveMessagesAsync()
    {
        byte[] buffer = new byte[1024];

        while (!cts.Token.IsCancellationRequested)
        {
            try
            {
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Debug.Log($"Received message: {message}");
                    commandListener.ProcessCommand(message);  // Pass command to CommandListener
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error receiving message: {e.Message}");
                break;
            }
        }
    }

    // Cleanup WebSocket connection
    void OnDestroy()
    {
        if (webSocket != null && webSocket.State == WebSocketState.Open)
        {
            cts.Cancel();
            webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client", CancellationToken.None);
            webSocket.Dispose();
        }
    }
}
