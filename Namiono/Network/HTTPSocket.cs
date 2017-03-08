using System;
using System.Net;

namespace Namiono.Network
{
	public class HTTPSocket
	{
		public delegate void HTTPDataReceivedEventHandler(object sender, HTTPDataReceivedEventArgs e);
		public delegate void HTTPDataSendEventHandler(object sender, HTTPDataSendEventArgs e);
		public delegate void HTTPErrorEventHandler(object sender, HTTPErrorEventArgs e);

		public class HTTPDataReceivedEventArgs : EventArgs
		{
			public HttpListenerContext context;
		}

		public class HTTPDataSendEventArgs : EventArgs
		{
			public string Bytessend;
		}

		public class HTTPErrorEventArgs : EventArgs
		{
			public Exception Exception;
		}

		public event HTTPDataReceivedEventHandler HTTPDataReceived;
		public event HTTPDataSendEventHandler HTTPDataSend;
		public event HTTPErrorEventHandler HTTPError;

		void OnHTTPError(Exception ex)
		{
			var evrgs = new HTTPErrorEventArgs();
			evrgs.Exception = ex;

			if (HTTPError != null)
				HTTPError.Invoke(this, evrgs);
		}

		HttpListener socket;
		public HTTPSocket(string hostname, int port, string protocol = "http")
		{
			socket = new HttpListener();
			socket.Prefixes.Add(string.Format("{2}://{0}:{1}/", hostname, port, protocol));
		}

		public void Start()
		{
			try
			{
				socket.Start();

				if (socket.IsListening)
					socket.BeginGetContext(new AsyncCallback(this.GetContext), null);
			}
			catch (Exception ex)
			{
				OnHTTPError(ex);
			}
		}

		public void Send(byte[] buffer, HttpListenerContext context)
		{
			context.Response.OutputStream.Write(buffer, 0, buffer.Length);
			context.Response.OutputStream.Close();
			context.Response.OutputStream.Dispose();

			if (HTTPDataSend != null)
				HTTPDataSend.Invoke(this, new HTTPDataSendEventArgs());
		}

		public void Close()
		{
			socket.Close();
		}

		void GetContext(IAsyncResult ar)
		{
			var evtargs = new HTTPDataReceivedEventArgs();
			evtargs.context = socket.EndGetContext(ar);

			if (HTTPDataReceived != null)
				HTTPDataReceived.Invoke(this, evtargs);

			try
			{
				if (socket.IsListening)
					socket.BeginGetContext(new AsyncCallback(this.GetContext), null);
			}
			catch (Exception ex)
			{
				OnHTTPError(ex);
			}
		}
	}
}
