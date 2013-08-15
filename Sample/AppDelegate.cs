using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Drawing;
using FormattedLabel;

namespace Sample
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register ("AppDelegate")]
    public partial class AppDelegate : UIApplicationDelegate
    {
        // class-level declarations
        UIWindow window;
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            // create a new window instance based on the screen size
            window = new UIWindow(UIScreen.MainScreen.Bounds);

            var label1 = new UIFormattedLabel(new[] 
            { 
                new Phrase("Hello", textColor: UIColor.Green), new Phrase("World", textColor: UIColor.Red) 
            });            
            var label2 = new UIFormattedLabel(new[]
            {
                new Phrase("Please click"),
                new Phrase("here", textColor: UIColor.Red, textDecoration: TextDecoration.Underline, action: () => new UIAlertView("Click!", "You clicked here!", null, null, "OK").Show())
            });
            var label3 = new UIFormattedLabel(new[]
            {
                new Phrase("You", fontSize: 8),
                new Phrase("can", fontSize: 10),
                new Phrase("have", fontSize: 12),
                new Phrase("words", fontSize: 14),
                new Phrase("of", fontSize: 16),
                new Phrase("different", fontSize: 18),
                new Phrase("sizes", fontSize: 20),
            });
            var label4 = new UIFormattedLabel(new[] { new Phrase("Text can be aligned to the left") });
            var label5 = new UIFormattedLabel(new[] { new Phrase("Text can be aligned to the center") }) { TextAlignment = TextAlignment.Center };
            var label6 = new UIFormattedLabel(new[] { new Phrase("Text can be aligned to the right") }) { TextAlignment = TextAlignment.Right };
            var label7 = new UIFormattedLabel(new[] 
            {
                new Phrase("You", font: UIFont.FromName("AmericanTypewriter", 14)),
                new Phrase("can", font: UIFont.FromName("AvenirNextCondensed", 14)),
                new Phrase("have", font: UIFont.FromName("Baskerville", 14)),
                new Phrase("words", font: UIFont.FromName("Chalkduster", 14)),
                new Phrase("with", font: UIFont.FromName("Courier", 14)),
                new Phrase("different", font: UIFont.FromName("Futura", 14)),
                new Phrase("fonts", font: UIFont.FromName("Noteworthy", 14)),
            });
            var label8 = new UIFormattedLabel(new[] { new Phrase("When you have a really long sentence, you can set a maximum number of lines.  If the text goes any longer the last line will end with an ellipsis.") }) { MaxLines = 3 };
            
            var view = new UIView();
            view.BackgroundColor = UIColor.White;
            view.AddSubview(label1);
            view.AddSubview(label2);
            view.AddSubview(label3);
            view.AddSubview(label4);
            view.AddSubview(label5);
            view.AddSubview(label6);
            view.AddSubview(label7);
            view.AddSubview(label8);
            
            label1.Frame = new RectangleF(0, 0, 80, 20);
            label2.Frame = new RectangleF(0, 20, 120, 20);
            label3.Frame = new RectangleF(0, 40, 320, 30);
            label4.Frame = new RectangleF(0, 70, 80, 60);
            label5.Frame = new RectangleF(100, 70, 80, 60);
            label6.Frame = new RectangleF(200, 70, 80, 60);
            label7.Frame = new RectangleF(0, 130, 320, 30);
            label8.Frame = new RectangleF(0, 160, 300, 60);
            
            var controller = new UIViewController();
            controller.View = view;
            window.RootViewController = controller;
            
            
            // make the window visible
            window.MakeKeyAndVisible();
            
            return true;
        }
    }
}

