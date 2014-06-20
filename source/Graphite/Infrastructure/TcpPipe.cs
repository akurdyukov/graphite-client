using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Common.Logging;

namespace Graphite.Infrastructure
{
    internal class TcpPipe : IPipe, IDisposable
    {
        private static readonly ILog logger = LogManager.GetCurrentClassLogger();

        private readonly IPEndPoint endpoint;

        private TcpClient tcpClient;

        private bool disposed;

        public TcpPipe(IPAddress address, int port)
        {
            this.endpoint = new IPEndPoint(address, port);

            this.RenewClient();
        }

        public bool Send(string message)
        {
            if (message == null)
                return false;

            return this.Send(new[] { message });
        }

        public bool Send(string[] messages)
        {
            if (messages == null)
                return false;

            var data = Encoding.Default.GetBytes(string.Join("\n", messages) + "\n");

            return this.CoreSend(data);
        }

        public void Dispose()
        {
            this.Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !this.disposed)
            {
                try
                {
                    if (this.tcpClient != null)
                    {
                        this.tcpClient.Close();
                    }
                }
                catch
                {
                }

                this.disposed = true;
            }
        }

        private void EnsureConnected()
        {
            try
            {
                if (this.tcpClient.Connected)
                    return;

                this.tcpClient.Connect(this.endpoint);
            }
            catch (ObjectDisposedException)
            {
                this.RenewClient();

                this.tcpClient.Connect(this.endpoint);
            }
        }

        private void RenewClient()
        {
            this.tcpClient = new TcpClient();
            this.tcpClient.ExclusiveAddressUse = false;
        }

        private bool CoreSend(byte[] data)
        {
            this.EnsureConnected();

            try
            {
                this.tcpClient
                    .GetStream()
                    .Write(data, 0, data.Length);

                return true;
            }
            catch (IOException exception)
            {
                logger.Error("IO Exception sending data", exception);
            }
            catch (ObjectDisposedException exception)
            {
                logger.Error("Exception sending data", exception);
            }

            return false;
        }
    }
}