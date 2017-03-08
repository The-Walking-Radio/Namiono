using System;
using System.Net;
using System.Xml;
using System.ComponentModel;
using System.Collections.Generic;
using Namiono.Interfaces;

namespace Namiono.Providers
{
	public class ShoutcastProvider : IDisposable, IProvider
	{
		public delegate void RequestHandledEventHandler(object sender, RequestHandledEventArgs e);
		public event RequestHandledEventHandler RequestHandled;

		public class RequestHandledEventArgs : EventArgs
		{
			public Actions Action;
			public Types Type;
		}

		Uri sc_url;
		int port;
		string hostname;
        string tpl;

		Dictionary<string, ShoutcastStream> streams;

        public ShoutcastProvider(string hostname, int port, string tpl)
		{
            this.tpl = tpl;

			streams = new Dictionary<string, ShoutcastStream>();
			this.hostname = hostname;
			this.port = port;

			sc_url = new Uri(string.Format("http://{0}:{1}/statistics", this.hostname, this.port));
		}

		public void Start()
		{
			using (var wc = new WebClient())
			{
				wc.DownloadStringCompleted += (wc_sender, wc_e) =>
				{
					using (var bgw = new BackgroundWorker())
					{
						bgw.WorkerReportsProgress = true;
						bgw.WorkerSupportsCancellation = true;

						bgw.DoWork += (bgw_doWorkSender, bgw_e) =>
						{
							var response = new XmlDocument();
							response.LoadXml((string)bgw_e.Argument);

							var totalStreams = int.Parse(response.DocumentElement.SelectSingleNode("STREAMSTATS/TOTALSTREAMS").InnerText);
							if (totalStreams != 0 && totalStreams < byte.MaxValue)
							{
								var streamXML = new XmlDocument();
								using (var scStream = new ShoutcastStream())
									for (var i = 0; i < totalStreams; i++)
									{
										streamXML.LoadXml(response.DocumentElement.SelectNodes("STREAMSTATS/STREAM")[i].OuterXml
										.Replace(string.Format("<STREAM id=\"{0}\">", (i + 1)), "<SHOUTCASTSERVER>")
										.Replace("</STREAM>", "</SHOUTCASTSERVER>"));

										streamXML.CreateXmlDeclaration("1.0", "UTF-8", "yes");

										scStream.Listeners = int.Parse(streamXML.DocumentElement.SelectSingleNode("CURRENTLISTENERS").InnerText);
										scStream.Active = (int.Parse(streamXML.DocumentElement.SelectSingleNode("STREAMSTATUS").InnerText) != 0);
										scStream.Title = streamXML.DocumentElement.SelectSingleNode("SONGTITLE").InnerText;
										scStream.Name = streamXML.DocumentElement.SelectSingleNode("SERVERTITLE").InnerText;
										scStream.ID = Convert.ToString((i + 1));
										scStream.XML = streamXML;

										bgw.ReportProgress((i + 1), scStream);
									}
							}
						};

						bgw.ProgressChanged += (bgw_progChanged_sender, progress) =>
						{
							var stream = (ShoutcastStream)progress.UserState;
							if (string.IsNullOrEmpty(stream.ID))
							{
								lock (streams)
								{
									if (streams.ContainsKey(stream.ID))
										streams.Add(stream.ID, stream);
									else
									{
										streams[stream.ID].Listeners = stream.Listeners;
										streams[stream.ID].Name = stream.Name;
										streams[stream.ID].Title = stream.Title;
										streams[stream.ID].XML = stream.XML;
									}
								}
							}
						};

						bgw.RunWorkerCompleted += (bgw_done_sender, done_e) =>
						{
							if (RequestHandled == null)
								return;

							var evargs = new RequestHandledEventArgs();
							evargs.Action = Actions.Add;
							evargs.Type = Types.Content;

							RequestHandled.Invoke(this, evargs);
						};
					}
				};

				wc.DownloadStringAsync(sc_url);
			}
		}

		public void Close()
		{
			streams.Clear();
		}

        public string HTMLTemplate
        {
            set { tpl = value; }
        }

		public void Dispose()
		{
		}

		public Dictionary<string, ShoutcastStream> Streams
		{
			get
			{
				return streams;
			}
		}

		public string HTMLOutput
		{
			get
			{
				var output = string.Empty;
				foreach (var item in Streams.Values)
				{
					if (item == null)
						continue;

					output += string.Format("<div class=\"stream-name\">{0}</div>\n", item.Name);
					output += string.Format("<div class=\"stream-title\">{0}</div>\n", item.Title);
				}

				return this.tpl.Replace("[[box-content]]", output).Replace("[[box-title]]", "Live Stream");
			}
		}
	}

	public class ShoutcastStream : IDisposable
	{
		string id;
		int listener;

		string title;
		string server_title;
		XmlDocument xmldoc;
		bool active;

		public string Title
		{
			get
			{
				return title;
			}

			set
			{
				title = value;
			}
		}

		public bool Active
		{
			get
			{
				return active;
			}

			set
			{
				active = value;
			}
		}

		public int Listeners
		{
			get
			{
				return listener;
			}

			set
			{
				listener = value;
			}
		}

		public string ID
		{
			get
			{
				return id;
			}

			set
			{
				id = value;
			}
		}

		public string Name
		{
			get
			{
				return server_title;
			}

			set
			{
				server_title = value;
			}
		}

		public XmlDocument XML
		{
			get
			{
				return xmldoc;
			}

			set
			{
				xmldoc = value;
			}
		}

		public void Dispose()
		{
		}
	}
}
