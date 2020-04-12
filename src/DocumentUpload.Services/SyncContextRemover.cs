using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace DocumentUpload.Services
{
	internal struct SyncContextRemover : INotifyCompletion
	{
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DebuggerHidden]
		public bool IsCompleted => SynchronizationContext.Current == null;

		[EditorBrowsable(EditorBrowsableState.Never)]
		[DebuggerHidden, DebuggerStepThrough]
		public override bool Equals(object obj) =>
			obj is SyncContextRemover sync && sync.IsCompleted == IsCompleted;

		[EditorBrowsable(EditorBrowsableState.Never)]
		[DebuggerHidden, DebuggerStepThrough]
		public override int GetHashCode() => 0;

		[EditorBrowsable(EditorBrowsableState.Never)]
		void INotifyCompletion.OnCompleted(Action continuation)
		{
			var prevContext = SynchronizationContext.Current;
			try
			{
				SynchronizationContext.SetSynchronizationContext(null);
				continuation();
			}
			finally
			{
				SynchronizationContext.SetSynchronizationContext(prevContext);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[DebuggerHidden, DebuggerStepThrough]
		public SyncContextRemover GetAwaiter() => this;

		[EditorBrowsable(EditorBrowsableState.Never)]
		[DebuggerHidden, DebuggerStepThrough]
		public void GetResult() { }
	}
}