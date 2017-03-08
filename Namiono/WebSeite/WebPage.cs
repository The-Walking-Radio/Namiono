using System;
using System.Net;
using System.Text;
using Namiono.Interfaces;

namespace Namiono
{
	public class WebPage : IActions
	{
		string doctype;
		string title;

		Namiono namiono;

		public delegate void WebPageRenderedEventHandler(object sender, WebPageRenderedEventArgs e);
		public event WebPageRenderedEventHandler WebPageRendered;

		public class WebPageRenderedEventArgs : EventArgs
		{
			public string Content;
			public HttpListenerContext Context;
		}

		public WebPage(Namiono provider, string doctype = "html", string title = "")
		{
			namiono = provider;
			namiono.WebPageRequested += (sender, e) =>
			{
				Render(e.Context, e.Path);
			};

			this.doctype = doctype;
			this.title = title;
		}

		string applyTeamplate(string path)
		{
			var data = new byte[FileSystem.Size(path)];
			var bytesRead = 0;
			var output = string.Empty;

			FileSystem.Read(path, ref data, out bytesRead);
			output = Encoding.UTF8.GetString(data);

			Array.Clear(data, 0, data.Length);

			return output;
		}

		void Render(HttpListenerContext context, string path)
		{
			if (WebPageRendered == null)
				return;

			var content = Functions.Skeleton(doctype, title, "default");
			content = content.Replace("[[site-content]]", applyTeamplate(FileSystem.ResolvePath("templates/site_content.tpl", path)));
			content = content.Replace("[[content-header]]", applyTeamplate(FileSystem.ResolvePath("templates/content_header.tpl", path)));
			content = content.Replace("[[content-box]]", applyTeamplate(FileSystem.ResolvePath("templates/content_box.tpl", path)));
			content = content.Replace("[[content-footer]]", applyTeamplate(FileSystem.ResolvePath("templates/content_footer.tpl", path)));

			var navigation = namiono.DB.SQLQuery("SELECT name, link, needs FROM navigation");
			var navbox = "<ul>\n";
			for (var i = uint.MinValue; i < navigation.Count; i++)
				navbox += "<li><a href=\"#\" onclick=\"LoadDocument('{0}', 'main', '{1}', '{2}')\">{1}</a></li>\n"
					.F(navigation[i]["links"], navigation[i]["name"], navigation[i]["needs"]);
			navbox += "</ul>\n";

			content = content.Replace("[[content-nav]]", navbox);
			content = content.Replace("[#APPNAME#]", "Namiono");
			content = content.Replace("[#YEAR#]", "{0}".F(DateTime.Now.Year));
			var evtargs = new WebPageRenderedEventArgs();

			evtargs.Content = content;
			evtargs.Context = context;

			WebPageRendered.Invoke(this, evtargs);
		}

		public void Add<T>(Types type, T value)
		{
			Notify(Actions.Add, type);
		}

		public void Update<T>(Types type, T value)
		{
			Notify(Actions.Update, type);
		}

		public void Remove<T>(Types type, T value)
		{
			Notify(Actions.Remove, type);
		}

		public void Notify(Actions action, Types type)
		{
		}
	}
}
