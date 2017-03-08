using System;

namespace Namiono
{
	public static class Functions
	{
		public static string Skeleton(string doctype, string title, string style)
		{
			return "<!Doctype {0}>\n<html>\n\t<head>\n\t\t<meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\" />\n\t\t<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0, user-scalable=no\">\n\t\t<title>{1}</title>\n\t\t<link href=\"styles/{2}/style.css\" rel=\"stylesheet\" type=\"text/css\" media=\"screen\" />\n\t</head>\n\t<body>[[site-content]]\n\t</body>\n</html>"
				.F(doctype, title, style);
		}

		public static string F(this string fmt, params object[] objs)
		{
			return string.Format(fmt, objs);
		}
	}
}
