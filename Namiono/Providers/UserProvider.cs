using System;
using Namiono.Interfaces;

namespace Namiono.Providers
{
	public class UserProvider : IActions
	{
		public delegate void RequestHandledEventHandler(object sender, RequestHandledEventArgs e);
		public event RequestHandledEventHandler RequestHandled;

		public class RequestHandledEventArgs : EventArgs
		{
			public Actions Action;
			public Types Type;
		}

		Namiono provider;

		public UserProvider(Namiono namiono)
		{
			provider = namiono;
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
			if (RequestHandled == null)
				return;

			var evargs = new RequestHandledEventArgs();
			evargs.Action = Actions.Remove;

			RequestHandled(this, evargs);
		}
	}
}
