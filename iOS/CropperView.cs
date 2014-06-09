using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.CoreGraphics;

namespace Cropper.iOS
{
    public class CropperView : UIView
    {
        PointF origin;
        SizeF cropSize;

        public CropperView ()
        {
            origin = new PointF (100, 100);
            cropSize = new SizeF (200, 200);

            BackgroundColor = UIColor.Clear;
            Opaque = false;

            Alpha = 0.4f;
        }

        public PointF Origin {
            get {
                return origin;
            }

            set {
                origin = value;
                SetNeedsDisplay ();
            }
        }

        public SizeF CropSize {
            get {
                return cropSize;
            }
            set {
                cropSize = value;
                SetNeedsDisplay ();
            }
        }

        public RectangleF CropRect {
            get {
                return new RectangleF (Origin, CropSize);
            }
        }

        public override void Draw (RectangleF rect)
        {
            base.Draw (rect);

            using (var g = UIGraphics.GetCurrentContext ()) {

                g.SetFillColor (UIColor.Gray.CGColor);
                g.FillRect (rect);

                g.SetBlendMode (CGBlendMode.Clear);
                UIColor.Clear.SetColor ();

                var path = new CGPath (); 
                path.AddRect (new RectangleF (origin, cropSize));

                g.AddPath (path);     
                g.DrawPath (CGPathDrawingMode.Fill);  
            }
        }
    }
}

