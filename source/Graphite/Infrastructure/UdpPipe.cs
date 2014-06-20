﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Common.Logging;

namespace Graphite.Infrastructure
{
    internal class UdpPipe : IPipe, IDisposable
    {
        private static readonly ILog logger = LogManager.GetCurrentClassLogger();

        private readonly UdpClient udpClient;

        private bool disposed;

        public UdpPipe(IPAddress address, int port)
        {
            this.udpClient = new UdpClient();

            this.udpClient.Connect(new IPEndPoint(address, port));
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

            var data = Encoding.Default.GetBytes(string.Join("\n", messages));

            return this.CoreSend(data);
        }
  
        private bool CoreSend(byte[] data)
        {
            try
            {
                this.udpClient.Send(data, data.Length);

                return true;
            }
            catch (SocketException exception)
            {
                logger.Error("Exception sending data", exception);
            }

            return false;
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
                    if (this.udpClient != null)
                    {
                        this.udpClient.Close();
                    }
                }
                catch
                {
                }

                this.disposed = true;
            }
        }
    }
}
