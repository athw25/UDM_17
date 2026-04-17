using Caro.Client.UI.Forms;
using Caro.Shared.Network;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Caro.Client.Network
{
    public class ClientSocket
    {
        private TcpClient client;
        private NetworkStream stream;
        
        public string? ServerIP { get; private set; }
        public int ServerPort { get; private set; }

        private readonly SemaphoreSlim sendLock = new SemaphoreSlim(1, 1);
        private CancellationTokenSource cts;

        public event Action<Packet>? OnReceive;

        public bool IsConnected => client?.Connected ?? false;

        // ================= CONNECT =================
        public async Task ConnectAsync(string ip, int port)
        {
            ServerIP = ip;        
            ServerPort = port;    
            
            client = new TcpClient();
            await client.ConnectAsync(ip, port);

            stream = client.GetStream();
            cts = new CancellationTokenSource();

            _ = Task.Run(() => ReceiveLoop(cts.Token));
        }

        // ================= RECEIVE LOOP =================
        private async Task ReceiveLoop(CancellationToken token)
        {
            try
            {
                var reader = new StreamReader(stream, Encoding.UTF8);

                while (!token.IsCancellationRequested)
                {
                    string line = await reader.ReadLineAsync();

                    if (string.IsNullOrEmpty(line))
                        continue;

                    Packet packet = JsonSerializer.Deserialize<Packet>(line);

                    OnReceive?.Invoke(packet);
                }
            }
            catch (Exception)
            {
                Disconnect();
            }
        }

        // ================= SEND =================
        public async Task SendAsync(Packet packet)
        {
            if (!IsConnected) return;

            string json = JsonSerializer.Serialize(packet) + "\n";

            byte[] data = Encoding.UTF8.GetBytes(json);

            await sendLock.WaitAsync();
            try
            {
                await stream.WriteAsync(data, 0, data.Length);
            }
            finally
            {
                sendLock.Release();
            }
        }

        // ================= SYNC SEND =================
        public void Send(Packet packet)
        {
            _ = SendAsync(packet);
        }

        // ================= GET HISTORY =================
        public async Task<System.Collections.Generic.List<Caro.Shared.Models.MatchHistory>> GetHistory(string username)
        {
            var tcs = new TaskCompletionSource<System.Collections.Generic.List<Caro.Shared.Models.MatchHistory>>();

            void handler(Packet p)
            {
                if (p.Command == CommandType.HistoryResponse)
                {
                    try
                    {
                        var data = JsonSerializer.Deserialize<System.Collections.Generic.List<Caro.Shared.Models.MatchHistory>>(p.Data);
                        tcs.TrySetResult(data);
                    }
                    catch
                    {
                        tcs.TrySetResult(new System.Collections.Generic.List<Caro.Shared.Models.MatchHistory>());
                    }
                }
            }

            OnReceive += handler;

            await SendAsync(new Packet
            {
                Command = CommandType.GetHistory,
                Payload = Caro.Shared.Utils.Serializer.Serialize(username)
            });

            var result = await tcs.Task;

            OnReceive -= handler;

            return result;
        }

        // ================= LOGIN =================
        public void Login(string username)
        {
            Send(new Packet
            {
                Command = CommandType.Login,
                Payload = Caro.Shared.Utils.Serializer.Serialize(username)
            });
        }

        // ================= CHALLENGE =================
        public void Challenge(string target)
        {
            Send(new Packet
            {
                Command = CommandType.Challenge,
                Data = target
            });
        }

        // ================= MOVE =================
        public void SendMove(object moveData)
        {
            Send(new Packet
            {
                Command = CommandType.Move,
                Data = JsonSerializer.Serialize(moveData)
            });
        }

        // ================= DISCONNECT =================
        public void Disconnect()
        {
            try
            {
                cts?.Cancel();
                stream?.Close();
                client?.Close();
            }
            catch { }
        }
    }
}
