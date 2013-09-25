using System;
using System.Drawing;

using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.QuartzComposer;
using MonoMac.CoreGraphics;

using Leap;

namespace XamMacLeapQC
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
		[Export("initWithCoder:")]
		public CustomView (NSCoder coder) : base(coder) { }
		public CustomView (IntPtr handle) : base(handle) { }

		QCCompositionLayer layer;

		public void Move(float normx, float t)
		{
			var x = Math.Max (Math.Min (normx * 2 - 1, 1), -1f);

			layer.SetValueForKeyPath (NSNumber.FromFloat (x),
			                          (NSString)"patch.XPos.value");

			layer.SetValueForKeyPath (NSNumber.FromFloat (t),
			                          (NSString)"patch.Turn.value");
		}

		public override void AwakeFromNib ()
		{
			var path = NSBundle.MainBundle.PathForResource ("Space", "qtz");

			layer = new QCCompositionLayer (path) { Frame = Frame };
			layer.BackgroundColor = new CGColor (0, 0, 0);

			Layer.AddSublayer (layer);
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

				var turn = point.Direction.x > 0 ? -10f : 10f;

				var ibox = frame.InteractionBox;
				var normPos = ibox.NormalizePoint(position);

				InvokeOnMainThread(() => MainView.Move(normPos.x, turn));
			};
		}

		public new MainWindow Window { get { return (MainWindow)base.Window; } }
	}
}

