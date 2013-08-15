using System;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;

namespace FormattedLabel
{
    /// <summary>
    /// A control for displaying a string of text where various words and phrases can have different 
    /// formatting options and behavior from surrounding text.
    /// </summary>
    public class UIFormattedLabel : UIView
    {
        /// <summary>
        /// Fired when either the text has changed or this control's Frame has been adjusted in such
        /// a way that the number of lines being displayed changes value.
        /// </summary>
        public event Action LineCountChanged;
    
        private Phrase[] phrases;
        private Word[] words;
        private Line[] lines;
        private WordPosition[] wordPositions;
        private int wordSpacing = -1;
        private int extraWordSpacing;
        private int extraLineSpacing;
        private SizeF lastSize;
        private Rectangle wordsArea;
        private UITapGestureRecognizer tapGesture;
        private int maxLines;
        private int lastLineCount;
        private UIFont font;
        private UIColor textColor;
        private TextAlignment textAlignment;
        private TextDecoration textDecoration;
        private bool invalidated;
    
        public UIFormattedLabel(Phrase[] phrases)
        {
            this.phrases = phrases;
            
            Font = UIFont.SystemFontOfSize(UIFont.SystemFontSize);
            TextColor = UIColor.Black;
            BackgroundColor = UIColor.Clear;
        }
        
        public UIFont Font
        { 
            get { return font; }
            set
            {
                font = value;
                Invalidate();
            }
        }
        
        public float FontSize
        {
            get { return Font.PointSize; }
            set { Font = DeriveFont(Font, value); }
        }
        
        public UIColor TextColor
        { 
            get { return textColor; }
            set
            {
                textColor = value;
                Invalidate();
            }
        }
        
        public TextDecoration TextDecoration 
        { 
            get { return textDecoration; }
            set
            {
                textDecoration = value;
                Invalidate();
            }
        }
        
        public TextAlignment TextAlignment 
        { 
            get { return textAlignment; }
            set 
            {
                textAlignment = value;
                Invalidate();
            }
        }
        
        public int MaxLines 
        {
            get { return maxLines; }
            set
            {
                maxLines = value;
                Invalidate();
            }
        }
        
        public int ExtraWordSpacing
        {
            get { return extraWordSpacing; }
            set
            {
                extraWordSpacing = value;
                Invalidate();
            }
        }
        
        public int ExtraLineSpacing
        {
            get { return extraLineSpacing; }
            set
            {
                extraLineSpacing = value;
                Invalidate();
            }
        }
        
        public string GetLine(int lineIndex)
        {
            return string.Join(" ", lines[lineIndex].Words.Select(x => x.Text));
        }
        
        public int LineCount
        {   
            get { return lines.Length; }
        }
        
        public Phrase[] Phrases
        {
            get { return phrases; }
            set
            {
                phrases = value;
                Invalidate();
            }
        }
        
        public void Invalidate()
        {
            invalidated = true;
            lines = null;
            lastSize = Size.Empty;
            wordsArea = Rectangle.Empty;
            wordSpacing = -1;
        }
        
        public void Refresh()
        {
            Invalidate();
            RebuildWords();
            LayoutSubviews();
            SetNeedsDisplay();
        }
        
        private void OnTap()
        {
            // Find word at tap location
            var tapLocation = tapGesture.LocationOfTouch(0, this);
            
            // Correct for frame
            tapLocation = new PointF(tapLocation.X, Frame.Height - tapLocation.Y);

            var word = FindWordAtLocation(tapLocation);            
            if (word != null && word.Value.Action != null)
                word.Value.Action();
        }
        
        private Word? FindWordAtLocation(PointF tapLocation)
        {
            for (var current = 0; current < words.Length; current++)
            {
                var word = words[current];
                var wordPosition = wordPositions[current];
                var wordRect = new RectangleF(wordPosition.Left, wordPosition.Bottom + word.Descender, word.Width, word.Height);
                if (wordRect.Contains(tapLocation))
                {
                    return word;
                }
            }
            return null;
        }
        
        private void RebuildWords()
        {
            using (var s = new NSString(" "))
            {
                var size = s.StringSize(Font);
                this.wordSpacing = (int)size.Width;
            }
            
            if (phrases.Any(x => x.Action != null))
            {
                UserInteractionEnabled = true;
                if (tapGesture == null)
                {
                    tapGesture = new UITapGestureRecognizer(OnTap);
                    AddGestureRecognizer(tapGesture);
                }
            }
            else
            {
                UserInteractionEnabled = false;
                if (tapGesture != null)
                {
                    RemoveGestureRecognizer(tapGesture);
                    tapGesture = null;
                }
            }
        
            words = phrases
                .SelectMany(x => SelectPosition(x.Text.Split(' '))
                    .Select(y => new { Phrase = x, Word = y.Item, IsFirst = y.IsFirst, IsLast = y.IsLast }))
                .Select((x, i) => new Word(
                    i, 
                    x.Word.Replace("\t", " "),      // The tab character is essentially just a non-breaking space
                    x.Phrase.FontSize != null ? DeriveFont(x.Phrase.Font ?? Font, x.Phrase.FontSize.Value) : (x.Phrase.Font ?? Font), 
                    x.Phrase.TextColor ?? TextColor,
                    x.Phrase.TextDecoration ?? TextDecoration,
                    x.IsFirst && x.Phrase.Style == WordStyle.Prefix ? x.Phrase.Style : 
                        x.IsLast && x.Phrase.Style == WordStyle.Suffix ? x.Phrase.Style :
                        WordStyle.Word,
                    x.Phrase.Action))
                .ToArray();
            wordPositions = new WordPosition[words.Length];
        }
        
        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            
            if (Frame != Rectangle.Empty)
                LayoutWords(Frame.Size);
        }
        
        private void LayoutWords(SizeF size)
        {
            if (!invalidated && lastSize == Frame.Size && lastSize != new SizeF())
                return;
            invalidated = false;
            lastSize = Frame.Size;
        
            if (words == null)
                RebuildWords();
            
            lines = SplitWordsByLines(size);
            if (lines.Length != lastLineCount)
                OnLineCountChanged();
            lastLineCount = lines.Length;
            
            var p = new Point();
            wordsArea = new Rectangle();
            
            bool firstLine = true;
            foreach (var line in lines.Reverse())
            {
                var descender = -line.Words.Max(x => x.Descender);
                switch (TextAlignment)
                {
                    case TextAlignment.Left:
                        p.X = 0;
                        break;
                    case TextAlignment.Right:
                        p.X = (int)(size.Width - line.Width);
                        break;
                    case TextAlignment.Center:
                        p.X = (int)((size.Width - line.Width) / 2);
                        break;
                }
                p.Y += firstLine ? descender : line.Words.Max(x => x.Height) + extraLineSpacing;
                
                for (var i = 0; i < line.Words.Length; i++)
                {
                    var word = line.Words[i];
                    wordPositions[word.Index] = new WordPosition(p.Y, p.X);
                        
                    p.X += word.Width;
                    
                    var nextWord = i < line.Words.Length - 1 ? (Word?)line.Words[i + 1] : null;
                    var isAdjacentToNextWord = IsAdjacentToNextWord(word, nextWord);
                    if (!isAdjacentToNextWord)
                    {
                        p.X += wordSpacing;
                    }
                    wordsArea = Rectangle.Union(wordsArea, new Rectangle(new Point(p.X, p.Y - descender), word.Size));
                }
                firstLine = false;
            }
        }
        
        protected virtual void OnLineCountChanged()
        {
            var lineCountChanged = LineCountChanged;
            if (lineCountChanged != null)
                lineCountChanged();
        }
        
        private bool IsAdjacentToNextWord(Word word, Word? nextWord)
        {
            return word.WordStyle == WordStyle.Prefix || (nextWord != null && nextWord.Value.WordStyle == WordStyle.Suffix);
        }
        
        private Line[] SplitWordsByLines(SizeF size)
        {
            List<Line> lines = new List<Line>();
            var currentLine = new List<Word>();
            
            var remainingLineWidth = size.Width;
            var ellipsisWidth = 0;
            if (maxLines > 0)
            {
                using (var s = new NSString("..."))
                {
                    var ellipsisSize = s.StringSize(Font);
                    ellipsisWidth = (int)ellipsisSize.Width;
                }
            }
            for (var i = 0; i < words.Length; i++)
            {
                var word = words[i];
                var nextWord = i < words.Length - 1 ? (Word?)words[i + 1] : null;
                var isAdjacentToNextWord = IsAdjacentToNextWord(word, nextWord);
                var wordSpacing = isAdjacentToNextWord ? 0 : this.wordSpacing;
                Action createLine = () => lines.Add(new Line((int)(size.Width - remainingLineWidth), currentLine.ToArray()));
                
                if (maxLines > 0 && currentLine.Any() && lines.Count + 1 >= maxLines && remainingLineWidth - (word.Width + ellipsisWidth) < 0)
                {
                    // We've reached the end.  Go ahead and add the ellipsis word
                    remainingLineWidth -= ellipsisWidth;
                    currentLine.Add(new Word(word.Index, "...", Font, TextColor, TextDecoration, WordStyle.Suffix, null));
                    createLine();
                    break;
                }
                if (word.Width < remainingLineWidth || !currentLine.Any())
                {
                    currentLine.Add(word);
                    remainingLineWidth -= word.Width + wordSpacing;
                    
                    if (i == words.Length - 1)
                        createLine();
                }
                else
                {
                    createLine();
                    currentLine = new List<Word>();
                    remainingLineWidth = size.Width;
                    
                    currentLine.Add(word);
                    remainingLineWidth -= word.Width + wordSpacing;
                    
                    if (i == words.Length - 1)
                        createLine();
                }
            }
            return lines.ToArray();
        }

        public override SizeF SizeThatFits(SizeF size)
        {
            LayoutWords(size);
        
            return wordsArea.Size;
        }
        
        public override void Draw(RectangleF rect)
        {
            base.Draw(rect);

            var canvas = UIGraphics.GetCurrentContext();
            canvas.ClearRect(rect);
            canvas.TranslateCTM(0, rect.Height);
            canvas.ScaleCTM(1, -1);
            canvas.SetTextDrawingMode(CGTextDrawingMode.Fill);
            
            var extraHeight = rect.Height - wordsArea.Height;
            
            foreach (var line in lines)
            {
                foreach (var item in SelectPosition(line.Words))
                {
                    var word = item.Item;
                    var p = wordPositions[word.Index].Offset(rect.Location);
                    word.TextColor.SetFill();
                    canvas.SelectFont(word.Font.Name, word.Font.PointSize, CGTextEncoding.MacRoman);
                    canvas.ShowTextAtPoint(p.X, p.Y + extraHeight, word.Text);
                    
                    DrawTextDecoration(canvas, line, item, p);
                }
            }
        } 
        
        private void DrawTextDecoration(CGContext canvas, Line line, Position<Word> item, Point p)
        {
            var word = item.Item;
            switch (word.TextDecoration)
            {
                case TextDecoration.Underline:
                case TextDecoration.Strikethrough:
                    // Draw a line with a 1 pixel space between the baseline and and the underline
                    word.TextColor.SetStroke();
                    canvas.SetLineWidth(1);
                    
                    float right = p.X + word.Width;
                    var hasMoreWords = item.Index < words.Length - 1;
                    if (hasMoreWords)
                    {
                        var nextWord = !item.IsLast ? (Word?)line.Words[item.Index + 1] : null;
                        if (nextWord != null)
                        {
                            var nextWordContinuesDecoration = word.TextDecoration == nextWord.Value.TextDecoration && word.TextColor == nextWord.Value.TextColor;
                            if (nextWordContinuesDecoration)
                            {
                                right = wordPositions[word.Index + 1].Left;
                            }
                        }
                    }
                    
                    var yOffset = word.TextDecoration == TextDecoration.Underline ? -2.5f : word.Font.xHeight / 2;
                    
                    canvas.StrokeLineSegments(new[] { new PointF(p.X, p.Y + yOffset), new PointF(right, p.Y + yOffset) });
                    break;
            }
        }

        /// <summary>
        /// Immutable type that represents a discrete word in a string.  Includes all relevant info for 
        /// layout and styling of the text.
        /// </summary>
        private struct Word
        {
            private readonly int index;
            private readonly string text;
            private readonly int width;
            private readonly int height;
            private readonly int ascender;
            private readonly int descender;
            private readonly UIFont font;
            private readonly UIColor textColor;
            private readonly TextDecoration decoration;
            private readonly Action action;
            private readonly WordStyle wordStyle;
            
            public Word(int index, string text, UIFont font, UIColor textColor, TextDecoration decoration, WordStyle wordStyle, Action action)
            {
                this.index = index;
                this.text = text;
                this.font = font;
                this.textColor = textColor;
                this.decoration = decoration;
                this.wordStyle = wordStyle;
                this.action = action;
                
                // We need to use an NSString in order to measure text width
                using (var s = new NSString(text))
                {
                    var size = s.StringSize(font);
                    
                    width = (int)size.Width;
                    height = (int)font.LineHeight;
                    ascender = (int)Math.Round(font.Ascender, MidpointRounding.AwayFromZero);
                    descender = (int)Math.Round(font.Descender, MidpointRounding.AwayFromZero);
                }
            }
            
            public int Index
            {
                get { return index; }
            }
            
            public string Text
            {
                get { return text; }
            }
            
            public Action Action
            {
                get { return action; }
            }
            
            public int Width 
            {
                get { return width; }
            }
            
            public int Height
            {
                get { return height; }
            }
            
            public int Ascender 
            {
                get { return ascender; }
            }
            
            public int Descender 
            {
                get { return descender; }
            }
            
            public UIFont Font 
            {
                get { return font; }
            }
            
            public UIColor TextColor
            {
                get { return textColor; }
            }
            
            public TextDecoration TextDecoration
            {
                get { return decoration; }
            }
            
            public Size Size
            {
                get { return new Size(Width, Height); }
            }
            
            public WordStyle WordStyle
            {
                get { return wordStyle; }
            }
            
            public override string ToString()
            {
                return text;
            }
        }
        
        private struct Line
        {
            private int width;
            private readonly Word[] words;
            
            public Line(int width, Word[] words)
            {
                this.width = width;
                this.words = words;
            }
            
            public int Width 
            {
                get { return width; }
            }
            
            public Word[] Words
            {
                get { return words; }
            }
        }
        
        private struct WordPosition
        {
            private readonly int bottom;
            private readonly int left;
            
            public WordPosition(int bottom, int left)
            {
                this.bottom = bottom;
                this.left = left;
            }
            
            public int Bottom 
            {
                get { return bottom; }
            }
            
            public int Left
            {
                get { return left; }
            }
            
            public Point Offset(PointF p)
            {
                return new Point(Left + (int)p.X, Bottom - (int)p.Y);
            }
            
            public override string ToString()
            {
                return "(bottom: " + bottom + ", left: " + left + ")";
            }
        }
        
        private static UIFont DeriveFont(UIFont font, float size) 
        {
            return UIFont.FromName(font.Name, size);
        }
        
        private class Position<T>
        {
            public T Item { get; set; }
            public int Index { get; set; }
            public bool IsFirst { get; set; }
            public bool IsLast { get; set; }
        }

        private static IEnumerable<Position<T>> SelectPosition<T>(IEnumerable<T> source)
        {
            IEnumerator<T> enumerator = source.GetEnumerator();

            if (enumerator.MoveNext())
            {
                Func<T, int, bool, bool, Position<T>> makePosition = (item, index, isFirst, isLast) => new Position<T>
                {
                    Item = item, Index = index, IsFirst = isFirst, IsLast = isLast
                };

                T current = enumerator.Current;
                bool hasNext = enumerator.MoveNext();

                int i = 0;

                yield return makePosition(current, i++, true, !hasNext);

                while (hasNext)
                {
                    current = enumerator.Current;
                    hasNext = enumerator.MoveNext();
                    yield return makePosition(current, i++, false, !hasNext);
                }                
            }
        }                
    }
    
    public enum TextAlignment
    {
        Left, Right, Center
    }
    
    public enum TextDecoration
    {
        None, Underline, Strikethrough
    }    
    
    public enum WordStyle
    {
        /// <summary>
        /// A space will surround both the left and right side of this phrase.
        /// </summary>
        Word, 
        
        /// <summary>
        /// A space will only appear on the left side, and no space will appear between this phrase and the next
        /// phrase.
        /// </summary>
        Prefix, 
        
        /// <summary>
        /// A space will only appear on the right side, and no space will appear between this phrase and the prior
        /// phrase.
        /// </summary>
        Suffix
    }

    public class Phrase
    {
        public string Text { get; set; }
        public float? FontSize { get; set; }
        public UIColor TextColor { get; set; }
        public UIFont Font { get; set; }
        public Action Action { get; set; }
        public TextDecoration? TextDecoration { get; set; }
        public WordStyle Style { get; set; }
    
        public Phrase(string text, float? fontSize = null, UIColor textColor = null, UIFont font = null, TextDecoration? textDecoration = null, WordStyle style = WordStyle.Word, Action action = null)
        {
            if (text == null)
                throw new ArgumentNullException("text");
            Text = text;
            FontSize = fontSize;
            TextColor = textColor;
            TextDecoration = textDecoration;
            Font = font;
            Style = style;
            Action = action;
        }
    }
}

