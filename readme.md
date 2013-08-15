UIFormattedLabel
==============================

<img src="ScreenShot.png" width="320" height="480" align="right" />

A control for displaying a string of text where various words and phrases can have different 
formatting options and behavior from surrounding text.  The following attributes apply to 
each individual word and can thus be adjusted independently:

* Text color
* Font (and thus font size, bold, italics, etc.)
* Text decoration (underline, strikethrough)
* Tap events (underline a word and respond to when the user taps on it)
* Text alignment (standard "paragraph" alignment options of left, right, and center)
* Maximum lines (when surpassed, an ellipsis will be rendered at the end of the max line)

Any of these settings can be updated after the fact, followed by a call to `Refresh()`.

The following is the code used to produce the screen shot to the right:

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
