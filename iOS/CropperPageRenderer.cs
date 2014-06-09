﻿using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using MonoTouch.UIKit;
using System.Drawing;

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

        public CropperPageRenderer ()
        {
        }

        protected override void OnModelSet (VisualElement model)
        {
            base.OnModelSet (model);

            var page = model as CropperPage;
            var view = NativeView;

            var label = new UILabel (new RectangleF(20, 40, view.Frame.Width-40, 40));
            label.AdjustsFontSizeToFitWidth = true;
            label.TextColor = UIColor.White;
            label.Text = page.Text;

            view.Add (label);
        }

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();

            using (var image = UIImage.FromFile ("monkey.png")) {
                imageView = new UIImageView (new RectangleF (0, 0, image.Size.Width, image.Size.Height));
                imageView.Image = image;
            }

            cropperView = new CropperView { Frame = View.Frame };
            View.AddSubviews (imageView, cropperView);

            float dx = 0;
            float dy = 0;

            pan = new UIPanGestureRecognizer ((gesture) => {
                if ((gesture.State == UIGestureRecognizerState.Began || gesture.State == UIGestureRecognizerState.Changed) && (gesture.NumberOfTouches == 1)) {

                    var p0 = gesture.LocationInView (View);

                    if (dx == 0)
                        dx = p0.X - cropperView.Origin.X;

                    if (dy == 0)
                        dy = p0.Y - cropperView.Origin.Y;

                    var p1 = new PointF (p0.X - dx, p0.Y - dy);

                    cropperView.Origin = p1;
                } else if (gesture.State == UIGestureRecognizerState.Ended) {
                    dx = 0;
                    dy = 0;
                }
            });

            float s0 = 1;

            pinch = new UIPinchGestureRecognizer ((gesture) => {
                float s = gesture.Scale;
                float ds = Math.Abs (s - s0);
                float sf = 0;
                const float rate = 0.5f;

                if (s >= s0) {
                    sf = 1 + ds * rate;
                } else if (s < s0) {
                    sf = 1 - ds * rate;
                }
                s0 = s;

                cropperView.CropSize = new SizeF (cropperView.CropSize.Width * sf, cropperView.CropSize.Height * sf);  

                if (gesture.State == UIGestureRecognizerState.Ended) {
                    s0 = 1;
                }
            });

            doubleTap = new UITapGestureRecognizer ((gesture) => {
                Crop();

            }) { 
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

                cropperView.Origin = new PointF (imageView.Frame.Left, imageView.Frame.Top);
                cropperView.Hidden = true;
            }
        }
    }
}