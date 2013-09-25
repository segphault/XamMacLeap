using System;
using System.Drawing;

using MonoMac.Foundation;
using MonoMac.AppKit;

using Leap;

namespace XamMacLeap
{
	public class MainListener : Listener
	{
		public event EventHandler FrameAvailable;

		public override void OnFrame (Controller controller)
		{
			if (FrameAvailable != null)
				FrameAvailable (this, new EventArgs());
		}
	}

	[Register("CustomView")]
	public class CustomView : NSView
	{
		public CustomView (IntPtr handle) : base(handle) {}

		[Export("initWithCoder:")]
		public CustomView (NSCoder coder) : base(coder) {}

		float xpos = 1f;
		float ypos = 1f;
		float size = 15f;

		public override bool IsFlipped { get { return true; } }

		public void Move(float normx, float normy)
		{
			var x = normx * Frame.Width;
			var y = Frame.Height - normy * Frame.Height;

			xpos = Math.Max (Math.Min (x, Frame.Width - size), 0f);
			ypos = Math.Max (Math.Min (y, Frame.Height - size), 0f);

			NeedsDisplay = true;
		}

		public override void DrawRect (RectangleF dirtyRect)
		{
			var shape = new RectangleF (xpos, ypos, size, size);
			var path = NSBezierPath.FromOvalInRect (shape);
			var color = NSColor.FromCalibratedRgba (0, 0, 0, 1);

			color.SetFill ();
			path.Fill ();
		}
	}

	public partial class MainWindowController : MonoMac.AppKit.NSWindowController
	{
		public MainWindowController (IntPtr handle) : base (handle) {}
	
		[Export ("initWithCoder:")]
		public MainWindowController (NSCoder coder) : base (coder) {}

		public MainWindowController () : base ("MainWindow") {}

		public override void AwakeFromNib ()
		{
			var listener = new MainListener ();
			var controller = new Controller (listener);

			listener.FrameAvailable += (sender, e) => {
				var frame = controller.Frame();
				var point = frame.Pointables.Frontmost;
				var position = point.StabilizedTipPosition;

				var ibox = frame.InteractionBox;
				var normPos = ibox.NormalizePoint(position);

				InvokeOnMainThread(() => MainView.Move(normPos.x, normPos.y));
			};
		}

		public new MainWindow Window { get { return (MainWindow)base.Window; } }
	}
}

