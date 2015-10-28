using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using UIKit;
using CoreGraphics;

[assembly:ExportRenderer(typeof(Cropper.CropperPage), typeof(Cropper.iOS.CropperPageRenderer))]

namespace Cropper.iOS
{
    public class CropperPageRenderer : PageRenderer
    {
        UIImageView imageView;
        CropperView cropperView;
        UIPanGestureRecognizer pan;
        UIPinchGestureRecognizer pinch;
        UITapGestureRecognizer doubleTap;

        protected override void OnElementChanged (VisualElementChangedEventArgs e)
        {
            base.OnElementChanged (e);
           
            var page = e.NewElement as CropperPage;
            var view = NativeView;

            var label = new UILabel (new CGRect(20, 40, view.Frame.Width-40, 40));
            label.AdjustsFontSizeToFitWidth = true;
            label.TextColor = UIColor.White;

            if (page != null) {
                label.Text = page.Text;
            }

            view.Add (label);
        }

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();

            using (var image = UIImage.FromFile ("monkey.png")) {
                imageView = new UIImageView (new CGRect (0, 0, image.Size.Width, image.Size.Height));
                imageView.Image = image;
            }

            cropperView = new CropperView { Frame = View.Frame };
            View.AddSubviews (imageView, cropperView);

            nfloat dx = 0;
            nfloat dy = 0;

			pan = new UIPanGestureRecognizer (() => {
                if ((pan.State == UIGestureRecognizerState.Began || pan.State == UIGestureRecognizerState.Changed) && (pan.NumberOfTouches == 1)) {

                    var p0 = pan.LocationInView (View);

                    if (dx == 0)
                        dx = p0.X - cropperView.Origin.X;

                    if (dy == 0)
                        dy = p0.Y - cropperView.Origin.Y;

                    var p1 = new CGPoint (p0.X - dx, p0.Y - dy);

                    cropperView.Origin = p1;
                } else if (pan.State == UIGestureRecognizerState.Ended) {
                    dx = 0;
                    dy = 0;
                }
            });

            nfloat s0 = 1;

            pinch = new UIPinchGestureRecognizer (() => {
				nfloat s = pinch.Scale;
				nfloat ds = (nfloat)Math.Abs (s - s0);
                nfloat sf = 0;
                const float rate = 0.5f;

                if (s >= s0) {
                    sf = 1 + ds * rate;
                } else if (s < s0) {
                    sf = 1 - ds * rate;
                }
                s0 = s;

                cropperView.CropSize = new CGSize (cropperView.CropSize.Width * sf, cropperView.CropSize.Height * sf);  

				if (pinch.State == UIGestureRecognizerState.Ended) {
                    s0 = 1;
                }
            });

            doubleTap = new UITapGestureRecognizer ((gesture) => Crop ()) { 
                NumberOfTapsRequired = 2, NumberOfTouchesRequired = 1 
            };

            cropperView.AddGestureRecognizer (pan);
            cropperView.AddGestureRecognizer (pinch);
            cropperView.AddGestureRecognizer (doubleTap);   
        }

        void Crop()
        {
            var inputCGImage = UIImage.FromFile ("monkey.png").CGImage;

            var image = inputCGImage.WithImageInRect (cropperView.CropRect);
            using (var croppedImage = UIImage.FromImage (image)) {

                imageView.Image = croppedImage;
                imageView.Frame = cropperView.CropRect;
                imageView.Center = View.Center;

                cropperView.Origin = new CGPoint (imageView.Frame.Left, imageView.Frame.Top);
                cropperView.Hidden = true;
            }
        }
    }
}