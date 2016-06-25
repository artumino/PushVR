using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Runtime.CompilerServices;

using PushbulletVR.Extensions;

namespace PushbulletVR.PushbulletAPI.Networking
{
    public class WebSocketManager
    {
        public class StateObject
        {
            public const int BufferSize = 1024;             // Dimensione del buffer di ricezione
            public ArraySegment<byte> BufferSegment;
            public string TextValue = "";

            public StateObject()
            {
                BufferSegment = WebSocket.CreateClientBuffer(BufferSize, BufferSize);
            }
        }
        
        public delegate void MessageReceived(StateObject state, string value);
        public event MessageReceived OnMessageReceived;

        public CancellationTokenSource connectCSRC;
        public ClientWebSocket ClientSocket;
        
        public async Task<Exception> Connect(Uri WebService)
        {
            try
            {
                connectCSRC = new CancellationTokenSource();
                ClientSocket = new ClientWebSocket();
                await ClientSocket.ConnectAsync(WebService, connectCSRC.Token);
                Connected(connectCSRC.Token);
            }
            catch(Exception ex)
            {
                return ex;
            }
            return null;
        }
        
        private async void Connected(CancellationToken token)
        {
            StateObject state = new StateObject();
            while (!token.IsCancellationRequested)
            {
                WebSocketReceiveResult response = await ClientSocket.ReceiveAsync(state.BufferSegment, token);

                if (response.EndOfMessage && response.MessageType == WebSocketMessageType.Text)
                {
                    string Result = state.TextValue + System.Text.Encoding.UTF8.GetString(state.BufferSegment.Array, 0, response.Count);
                    Array.Clear(state.BufferSegment.Array, 0, response.Count);
                    
                    state.TextValue = "";
                    if (OnMessageReceived != null)
                        OnMessageReceived(state, Result);
                }
                else if(!response.EndOfMessage && response.MessageType == WebSocketMessageType.Text)
                {
                    string Result = System.Text.Encoding.UTF8.GetString(state.BufferSegment.Array, 0, response.Count);
                    Array.Clear(state.BufferSegment.Array, 0 , response.Count);
                    state.TextValue += Result;
                }
            }
        }
        
    }
}