using System;
using System.IO;
using System.Net;
using System.Text;

namespace Namiono.Network
{
	public class WebServer
	{
		public delegate void WSDataReceivedEventHandler(object sender, WSDataReceivedEventArgs e);
		public delegate void WSDataSendEventHandler(object sender, WSDataSendEventArgs e);
		public delegate void WSErrorEventHandler(object sender, WSErrorEventArgs e);
        public delegate void WSStartedEventHandler(object sender, WSStartedEventArgs e);
		public class WSDataReceivedEventArgs : EventArgs
		{
			public readonly Encoding Encoding;
			public readonly IPEndPoint RemoteEndPoint;
			public HttpListenerContext Context;
            public Needs Needs;
		}

        public class WSStartedEventArgs : EventArgs
        {
        }

        public class WSDataSendEventArgs : EventArgs
		{
		}

		public class WSErrorEventArgs : EventArgs
		{
            public Exception Exception;
		}

		public event WSDataReceivedEventHandler WSDataReceived;
		public event WSDataSendEventHandler WSDataSend;
		public event WSErrorEventHandler WSError;
        public event WSStartedEventHandler WSStarted;

		HTTPSocket socket;
		Namiono provider;
		string docRoot;

		public WebServer(Namiono namiono, string ident, string hostname, int port)
		{
			docRoot = Path.Combine(Environment.CurrentDirectory, ident);
			provider = namiono;
			provider.WebRequestHandled += (sender, e) =>
			{
                var data = Encoding.UTF8.GetBytes(e.Content);
				Send(ref data, ref e.Context);
			};

			socket = new HTTPSocket(hostname, port);
			socket.HTTPDataReceived += (sender, e) =>
            {
                var evtargs = new WSDataReceivedEventArgs();
                evtargs.Context = e.context;
                var path = FileSystem.ResolvePath(e.context.Request.Url.LocalPath, docRoot);

                if (path.EndsWith(".js", StringComparison.InvariantCultureIgnoreCase)
                    || path.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase)
                    || path.EndsWith(".css", StringComparison.InvariantCultureIgnoreCase)
                   || path.EndsWith(".ico", StringComparison.InvariantCultureIgnoreCase))
                {
                    var data = new byte[FileSystem.Size(path)];
                    var bytesRead = 0;

                    FileSystem.Read(path, ref data, out bytesRead);

                    e.context.Response.StatusCode = 200;
                    e.context.Response.StatusDescription = "OK";

                    Send(ref data, ref e.context);
                }
                else
                {
                    if (!string.IsNullOrEmpty(e.context.Request.Headers["Needs"]))
                    {
                        var needsHeader = e.context.Request.Headers["Needs"];
                        if (needsHeader == "shoutcast")
                            evtargs.Needs = Needs.ShoutCast;

                        if (needsHeader == "user")
                            evtargs.Needs = Needs.User;

                        if (needsHeader == "site")
                            evtargs.Needs = Needs.Site;
                    }
                    else
                        evtargs.Needs = Needs.Nothing;   

                    if (WSDataReceived != null)
                        WSDataReceived.Invoke(this, evtargs);
                }};

			socket.HTTPError += (sender, e) =>
			{
				if (WSError == null)
					return;

				var evrgs = new WSErrorEventArgs();
				evrgs.Exception = e.Exception;

				WSError.Invoke(this, evrgs);
			};
		}

		public void Start()
		{
			socket.Start();
            if (WSStarted != null)
                WSStarted.Invoke(this, new WSStartedEventArgs());
		}

		public string DocRoot
		{
			get
			{
				return this.docRoot;
			}
		}

		public void Send(ref byte[] content, ref HttpListenerContext context)
		{
			socket.Send(content, context);

			if (WSDataSend != null)
				WSDataSend.Invoke(this, new WSDataSendEventArgs());
		}

		public void Close()
		{
			socket.Close();
		}
	}
}
