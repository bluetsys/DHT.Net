// Authors:
//   Alan McGovern <alan.mcgovern@gmail.com>
//
// Copyright (C) 2008 Alan McGovern
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using DHTNet.Enums;
using DHTNet.Utils;

namespace DHTNet.Listeners
{
    public class UdpListener : Listener
    {
        private UdpClient _client;

        public UdpListener(IPEndPoint endpoint)
            : base(endpoint)
        {
        }

        public override event Action<byte[], IPEndPoint> MessageReceived;

        public override void Send(Stream stream, IPEndPoint endpoint)
        {
            try
            {
                byte[] buffer;

                MemoryStream memoryStream = stream as MemoryStream;
                if (memoryStream != null)
                {
                    buffer = memoryStream.ToArray();
                }
                else
                {
                    buffer = new byte[stream.Length];
                    using (MemoryStream ms = new MemoryStream(buffer))
                        stream.CopyTo(ms);
                }

                _client.SendAsync(buffer, buffer.Length, endpoint).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logger.Log("UdpListener could not send message: {0}", ex);
            }
        }

        public override void Start()
        {
            try
            {
                _client = new UdpClient(Endpoint);
                Status = ListenerStatus.Listening;

                StartReceive();
            }
            catch (ObjectDisposedException)
            {
                // Do Nothing
            }
            catch (Exception)
            {
                StartReceive();
            }
        }

        private void StartReceive()
        {
            _client.ReceiveAsync().ContinueWith(task =>
            {
                if (!task.IsFaulted && !task.IsCanceled)
                    MessageReceived?.Invoke(task.Result.Buffer, task.Result.RemoteEndPoint);

                StartReceive();
            });
        }

        public override void Stop()
        {
            _client.Dispose();
        }
    }
}