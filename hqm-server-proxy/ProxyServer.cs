using hqm_server_proxy.Classes;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;

namespace hqm_server_proxy
{
    public class ProxyServer
    {
        const string masterUrl = "https://sam2.github.io/HQMMasterServerEndpoint/";
        private string masterIp = "66.226.72.227";
        private int masterPort = 27590;
        private byte[] header = new byte[] { 0x48, 0x6f, 0x63, 0x6b };
        private UdpClient _socket;
        private Config _config;
        private string _name;
        private List<UdpProxyClient> _knownClients = new List<UdpProxyClient>();
        public ProxyServer(Config config, string name)
        {
            _config = config;
            _socket = new UdpClient(_config.Port);
            _name = name;
        }

        public async Task Run()
        {
            await GetMasterServer();

            //from client
            _socket.BeginReceive(new AsyncCallback(OnUdpData), _socket);

            var _master_timer = new Timer(SendMasterQuery, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));

            while (true) { Thread.Sleep(Int32.MaxValue); }
        }

        private void ProcessRecieve(byte[] data, IPEndPoint source)
        {
            if (header.SequenceEqual(data.Take(4))) {

                var c = _knownClients.FirstOrDefault(x => x.Client.Equals(source));
                if (c == null)
                {
                    var newSocket = new UdpClient(NextFreePort());
                    var client = new UdpProxyClient { Socket = newSocket, Client = source };
                    _knownClients.Add(client);
                    newSocket.BeginReceive(new AsyncCallback(OnUdpFromServer), client);
                    newSocket.Send(data, new IPEndPoint(IPAddress.Parse(_config.TargetIp), _config.TargetPort));
                }
                else
                {
                    c.Socket.BeginReceive(new AsyncCallback(OnUdpFromServer), c);
                    c.Socket.Send(data, new IPEndPoint(IPAddress.Parse(_config.TargetIp), _config.TargetPort));
                }
            }
        }

        private void ProcessRecieveFromServer(byte[] data, IPEndPoint client)
        {
            var command = data[4];
            if (header.SequenceEqual(data.Take(4)))
            {
                if (command == 1)
                {
                    var n = data.Skip(12);
                    var nm = Encoding.UTF8.GetString(n.ToArray());

                    var newName = nm.Replace("\0", "") + " ("+_name+" Proxy)";
                    var newNameBytes = Encoding.UTF8.GetBytes(newName);

                    var p = 12;
                    foreach (var c in newNameBytes)
                    {
                        data[p] = c;
                        p++;
                    }
                    _socket.Send(data, client);
                }
                else
                {
                    _socket.Send(data, client);
                }
            }
        }

        private void OnUdpData(IAsyncResult result)
        {
            IPEndPoint source = new IPEndPoint(0, 0);
            byte[] message = _socket.EndReceive(result, ref source);
            ProcessRecieve(message, source);
            _socket.BeginReceive(new AsyncCallback(OnUdpData), _socket);
        }


        private void OnUdpFromServer(IAsyncResult result)
        {
            var proxyClient = result.AsyncState as UdpProxyClient;
            IPEndPoint source = new IPEndPoint(0, 0);
            byte[] message = proxyClient.Socket.EndReceive(result, ref source);
            ProcessRecieveFromServer(message, proxyClient.Client);
            proxyClient.Socket.BeginReceive(new AsyncCallback(OnUdpFromServer), proxyClient);
        }

        private void SendMasterQuery(object? state)
        {
            var send_buffer = new byte[] { 0x48, 0x6f, 0x63, 0x6b, 0x20 };

            var serverAddr = IPAddress.Parse(masterIp);
            var endPoint = new IPEndPoint(serverAddr, masterPort);
            _socket.Send(send_buffer, endPoint);
        }

        private async Task GetMasterServer()
        {
            var client = new HttpClient();
            using HttpResponseMessage response = client.GetAsync(masterUrl).Result;
            using HttpContent content = response.Content;
            var r = await content.ReadAsStringAsync();
            r = Regex.Replace(r, @"\s+", " ");
            var items = r.Split(" ");
            masterIp = items[1];
            masterPort = Int32.Parse(items[2]);
        }



        private int NextFreePort(int port = 0)
        {
            port = new Random().Next(27580, 28580);
            while (!IsFree(port))
            {
                port += 1;
            }
            return port;
        }

        private static bool IsFree(int port)
        {
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] listeners = properties.GetActiveTcpListeners();
            int[] openPorts = listeners.Select(item => item.Port).ToArray<int>();
            return openPorts.All(openPort => openPort != port);
        }
    }
}
