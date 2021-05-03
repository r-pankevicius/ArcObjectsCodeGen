using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ArcObjectsCodeGen.Runtime
{
	/// <summary>
	/// Manages lifetime of multiple COM objects and IDisposables.
	/// </summary>
	public class Releaser : IDisposable
	{
		private readonly List<object> m_ManagedObjects = new();

		public T ManageLifetime<T>(object obj) where T : class
			=> ManageLifetime((T)obj); // Fail fast with InvalidCastException

		public T ManageLifetime<T>(T obj) where T : class
		{
			if (obj is not null)
				m_ManagedObjects.Add(obj);

			return obj;
		}

		public void Dispose()
		{
			foreach (object obj in m_ManagedObjects)
			{
				if (obj is IDisposable disposable)
					disposable.Dispose();
				else if (Marshal.IsComObject(obj))
					Marshal.ReleaseComObject(obj);
			}

			m_ManagedObjects.Clear();
		}
	}
}
