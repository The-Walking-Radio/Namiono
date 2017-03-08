using System;
using System.IO;
using System.Net;
using System.Text;
using Namiono.Network;
using Namiono.Providers;

namespace Namiono
{
	public class Namiono
	{
		WebServer ws_public;
		WebPage site_public;
		UserProvider UserProvider;
		ShoutcastProvider scProvider;
		SQLDatabase sqlDB;

		public delegate void WebPageRequestedEventHandler(object sender, WebPageRequestedEventArgs e);
		public delegate void WebRequestHandledEventHandler(object sender, WebRequestHandledEventArgs e);

		public class WebPageRequestedEventArgs : EventArgs
		{
			public HttpListenerContext Context;
			public string Path;
			public Actions Action;
			public Types Type;
		}

		public class WebRequestHandledEventArgs : EventArgs
		{
			public string Content;
			public HttpListenerContext Context;
		}

		public event WebRequestHandledEventHandler WebRequestHandled;
		public event WebPageRequestedEventHandler WebPageRequested;

		public Namiono()
		{
			sqlDB = new SQLDatabase();
			UserProvider = new UserProvider(this);
			site_public = new WebPage(this, "html");
			ws_public = new WebServer(this, "public_website", "*", 81);
			scProvider = new ShoutcastProvider("the-walking-radio.de", 8000
				, ws_public.DocRoot);

			UserProvider.RequestHandled += (sender, e) =>
			{
				var evtargs = new WebPageRequestedEventArgs();
				evtargs.Action = Actions.Update;
				evtargs.Type = e.Type;

				WebPageRequested.Invoke(this, evtargs);
			};

			ws_public.WSStarted += (sender, e) =>
			{
				var tplPath = Path.Combine(ws_public.DocRoot, "templates/content_box.tpl");
				var data = new byte[FileSystem.Size(tplPath)];

				scProvider.HTMLTemplate = Encoding.UTF8.GetString(data);
				scProvider.Start();
			};

			ws_public.WSDataReceived += (sender, e) =>
			{
				switch (e.Needs)
				{
					case Needs.ShoutCast:
						break;
				}

				if (WebPageRequested == null)
					return;

				var evargs = new WebPageRequestedEventArgs();
				evargs.Context = e.Context;
				evargs.Path = ws_public.DocRoot;

				WebPageRequested.Invoke(this, evargs);
			};

			site_public.WebPageRendered += (sender, e) =>
			{
				if (WebRequestHandled == null)
					return;

				var evargs = new WebRequestHandledEventArgs();
				evargs.Content = e.Content;
				evargs.Context = e.Context;

				WebRequestHandled.Invoke(this, evargs);
			};

			ws_public.WSError += (sender, e) =>
				Console.WriteLine("Webserver Exception: {0}", e.Exception);
		}

		public void Start()
		{
			ws_public.Start();
		}

		public SQLDatabase DB
		{
			get
			{
				return sqlDB;
			}
		}

		public void Close()
		{
			ws_public.Close();
		}
	}
}
