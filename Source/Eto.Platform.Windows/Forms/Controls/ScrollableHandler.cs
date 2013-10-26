using System;
using sd = System.Drawing;
using swf = System.Windows.Forms;
using Eto.Drawing;
using Eto.Forms;

namespace Eto.Platform.Windows
{
	public class ScrollableHandler : WindowsDockContainer<ScrollableHandler.CustomScrollable, Scrollable>, IScrollable
	{
		readonly swf.Panel content;
		bool expandWidth = true;
		bool expandHeight = true;

		public class CustomScrollable : System.Windows.Forms.Panel
		{
			public ScrollableHandler Handler { get; set; }

			protected override bool ProcessDialogKey(swf.Keys keyData)
			{
				var e = new swf.KeyEventArgs(keyData);
				OnKeyDown(e);
				if (!e.Handled)
				{
					// Prevent firing the keydown event twice for the same key
					Handler.LastKeyDown = e.KeyData.ToEto();
				}
				return e.Handled;
			}

			protected override void OnCreateControl()
			{
				base.OnCreateControl();
				AutoSize = false;
			}

			protected override sd.Point ScrollToControl(swf.Control activeControl)
			{
				/*if (autoScrollToControl) return base.ScrollToControl(activeControl);
				else return this.AutoScrollPosition;*/
				return AutoScrollPosition;
			}

			protected override void OnClientSizeChanged(EventArgs e)
			{
				base.OnClientSizeChanged(e);
				Handler.UpdateExpanded();
			}
		}

		public override swf.Control ContainerContentControl
		{
			get { return content; }
		}

		public override void SetScale(bool xscale, bool yscale)
		{
			base.SetScale(xscale, yscale);
			if (Content != null)
				Content.SetScale(!ExpandContentWidth, !ExpandContentHeight);
		}

		protected override void SetContentScale(bool xscale, bool yscale)
		{
			base.SetContentScale(!ExpandContentWidth, !ExpandContentHeight);
		}

		public override Size DesiredSize
		{
			get
			{
				var baseSize = UserDesiredSize;
				var size = base.DesiredSize;
				// if we have set to a specific size, then try to use that
				if (baseSize.Width >= 0)
					size.Width = baseSize.Width;
				if (baseSize.Height >= 0)
					size.Height = baseSize.Height;
				return size;
			}
		}

		public BorderType Border
		{
			get
			{
				switch (Control.BorderStyle)
				{
					case swf.BorderStyle.FixedSingle:
						return BorderType.Line;
					case swf.BorderStyle.None:
						return BorderType.None;
					case swf.BorderStyle.Fixed3D:
						return BorderType.Bezel;
					default:
						throw new NotSupportedException();
				}
			}
			set
			{
				switch (value)
				{
					case BorderType.Bezel:
						Control.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
						break;
					case BorderType.Line:
						Control.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
						break;
					case BorderType.None:
						Control.BorderStyle = System.Windows.Forms.BorderStyle.None;
						break;
					default:
						throw new NotSupportedException();
				}
			}
		}

		public ScrollableHandler()
		{
			Control = new CustomScrollable
			{
				Handler = this,
				Size = sd.Size.Empty,
				MinimumSize = sd.Size.Empty,
				BorderStyle = swf.BorderStyle.Fixed3D,
				AutoScroll = true,
				AutoSize = true,
				AutoSizeMode = swf.AutoSizeMode.GrowAndShrink
			};
			Control.VerticalScroll.SmallChange = 5;
			Control.VerticalScroll.LargeChange = 10;
			Control.HorizontalScroll.SmallChange = 5;
			Control.HorizontalScroll.LargeChange = 10;

			content = new swf.Panel
			{
				Size = sd.Size.Empty,
				AutoSize = true,
				AutoSizeMode = swf.AutoSizeMode.GrowAndShrink
			};
			Control.Controls.Add(content);
		}

		void UpdateExpanded()
		{
			var contentControl = Content.GetWindowsHandler();
			if (contentControl != null)
			{

				var minSize = Control.ClientSize;
				minSize.Width = !ExpandContentWidth ? 0 : Math.Max(0, minSize.Width);
				minSize.Height = !ExpandContentHeight ? 0 : Math.Max(0, minSize.Height);

				// set the scale of the content based on whether we want it to or not
				contentControl.SetScale(!ExpandContentWidth, !ExpandContentHeight);
				// set minimum size for the content if we want to extend to the size of the scrollable width/height
				contentControl.ParentMinimumSize = minSize.ToEto();
			}
		}

		protected override void SetContent(swf.Control contentControl)
		{
			content.Controls.Clear();
			content.Controls.Add(contentControl);
		}

		public override void AttachEvent(string handler)
		{
			switch (handler)
			{
				case Scrollable.ScrollEvent:
					Control.Scroll += delegate {
						Widget.OnScroll(new ScrollEventArgs(ScrollPosition));
					};
					break;
				default:
					base.AttachEvent(handler);
					break;
			}
		}

		public void UpdateScrollSizes()
		{
			Control.PerformLayout();
		}

		public Point ScrollPosition
		{
			get { return new Point(-Control.AutoScrollPosition.X, -Control.AutoScrollPosition.Y); }
			set
			{
				Control.AutoScrollPosition = value.ToSD();
			}
		}

		public Size ScrollSize
		{
			get { return Control.DisplayRectangle.Size.ToEto(); }
			set { Control.AutoScrollMinSize = value.ToSD(); }
		}

		public Rectangle VisibleRect
		{
			get { return new Rectangle(ScrollPosition, Size.Min(ScrollSize, ClientSize)); }
		}

		public override Size ClientSize
		{
			get { return Control.ClientSize.ToEto(); }
			set
			{
				Control.AutoSize = value.Width == -1 || value.Height == -1;
				Control.ClientSize = value.ToSD();
			}
		}

		public bool ExpandContentWidth
		{
			get { return expandWidth; }
			set
			{
				if (expandWidth != value)
				{
					expandWidth = value;
					SetScale();
					UpdateExpanded();
				}
			}
		}

		public bool ExpandContentHeight
		{
			get { return expandHeight; }
			set
			{
				if (expandHeight != value)
				{
					expandHeight = value;
					SetScale();
					UpdateExpanded();
				}
			}
		}
	}
}
