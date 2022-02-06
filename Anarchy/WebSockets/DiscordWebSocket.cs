using Leaf.xNet;
using Newtonsoft.Json;
using System;
using DiskoAIO;
using WebSocketSharp;
using DiskoAIO.Properties;
using System.IO;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using System.Text;
using System.Linq;
using DiskoAIO.Anarchy.WebSockets.Gateway;

namespace Discord.WebSockets
{
    public class DiscordWebSocket<TOpcode> : IDisposable where TOpcode : Enum
    {
        private object _socketLock;
        private WebSocket _socket;
        public string ZLIB_SUFFIX = "\x00\x00\xff\xff";
        private byte[] ZLIB_HEADER = null;
        private ZlibStreamContext libStreamContext = new ZlibStreamContext();

        public delegate void MessageHandler(object sender, DiscordWebSocketMessage<TOpcode> message);
        public event MessageHandler OnMessageReceived;

        public delegate void CloseHandler(object sender, CloseEventArgs args);
        public event CloseHandler OnClosed;

        public DiscordWebSocket(string url)
        {
            _socketLock = new object();
            _socket = new WebSocket(url)
            {
                Origin = "https://discordapp.com"
            };
            _socket.OnMessage += OnMessage;
            _socket.OnClose += OnClose;
            //_socket.Compression = CompressionMethod.Deflate;
        }

        public void SetProxy(ProxyClient client)
        {
            if (client != null && client.Type == ProxyType.HTTP)
                _socket.SetProxy($"http://{client.Host}:{client.Port}", client.Username, client.Password);
        }

        public void Connect()
        {
            lock (_socketLock)
            {
                _socket.Connect();
            }
        }

        public void Close(ushort error, string reason)
        {
            lock (_socketLock)
            {
                _socket.Close(error, reason);
            }
        }

        public void Send<T>(TOpcode op, T data)
        {
            lock (_socketLock)
            {
                if (_socket != null) _socket.Send(JsonConvert.SerializeObject(new DiscordWebSocketRequest<T, TOpcode>(op, data)));
                else throw new InvalidOperationException("Socket is disposed of");
            }
        }

        private void OnClose(object sender, CloseEventArgs e)
        {
            OnClosed?.Invoke(this, e);
        }
        private void OnMessage(object sender, MessageEventArgs e)
        {
            if (ZLIB_HEADER == null)
                ZLIB_HEADER = e.RawData.SubArray(0, 2);
            byte[] output = libStreamContext.InflateByteArray(e.RawData.SubArray(0, 2).SequenceEqual(ZLIB_HEADER) ? e.RawData.SubArray(2, e.RawData.Length - 2) : e.RawData);
            OnMessageReceived?.Invoke(this, JsonConvert.DeserializeObject<DiscordWebSocketMessage<TOpcode>>(Encoding.UTF8.GetString(output, 0, output.Length)));
        }
        static byte[] Concat(byte[] a, byte[] b)
        {
            byte[] output = new byte[a.Length + b.Length];
            for (int i = 0; i < a.Length; i++)
                output[i] = a[i];
            for (int j = 0; j < b.Length; j++)
                output[a.Length + j] = b[j];
            return output;
        }
        public void Dispose()
        {
            lock (_socketLock)
            {
                _socket = null;
            }
        }
    }
}
