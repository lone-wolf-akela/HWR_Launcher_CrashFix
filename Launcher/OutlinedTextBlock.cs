using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;

namespace Launcher
{
	[ContentProperty("Text")]
	public class OutlinedTextBlock : FrameworkElement
	{
		public static readonly DependencyProperty FillProperty = DependencyProperty.Register("Fill", typeof(Brush), typeof(OutlinedTextBlock), new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));

		public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register("Stroke", typeof(Brush), typeof(OutlinedTextBlock), new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));

		public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register("StrokeThickness", typeof(double), typeof(OutlinedTextBlock), new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));

		public static readonly DependencyProperty FontFamilyProperty = TextElement.FontFamilyProperty.AddOwner(typeof(OutlinedTextBlock), new FrameworkPropertyMetadata(OnFormattedTextUpdated));

		public static readonly DependencyProperty FontSizeProperty = TextElement.FontSizeProperty.AddOwner(typeof(OutlinedTextBlock), new FrameworkPropertyMetadata(OnFormattedTextUpdated));

		public static readonly DependencyProperty FontStretchProperty = TextElement.FontStretchProperty.AddOwner(typeof(OutlinedTextBlock), new FrameworkPropertyMetadata(OnFormattedTextUpdated));

		public static readonly DependencyProperty FontStyleProperty = TextElement.FontStyleProperty.AddOwner(typeof(OutlinedTextBlock), new FrameworkPropertyMetadata(OnFormattedTextUpdated));

		public static readonly DependencyProperty FontWeightProperty = TextElement.FontWeightProperty.AddOwner(typeof(OutlinedTextBlock), new FrameworkPropertyMetadata(OnFormattedTextUpdated));

		public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(OutlinedTextBlock), new FrameworkPropertyMetadata(OnFormattedTextInvalidated));

		public static readonly DependencyProperty TextAlignmentProperty = DependencyProperty.Register("TextAlignment", typeof(TextAlignment), typeof(OutlinedTextBlock), new FrameworkPropertyMetadata(OnFormattedTextUpdated));

		public static readonly DependencyProperty TextDecorationsProperty = DependencyProperty.Register("TextDecorations", typeof(TextDecorationCollection), typeof(OutlinedTextBlock), new FrameworkPropertyMetadata(OnFormattedTextUpdated));

		public static readonly DependencyProperty TextTrimmingProperty = DependencyProperty.Register("TextTrimming", typeof(TextTrimming), typeof(OutlinedTextBlock), new FrameworkPropertyMetadata(OnFormattedTextUpdated));

		public static readonly DependencyProperty TextWrappingProperty = DependencyProperty.Register("TextWrapping", typeof(TextWrapping), typeof(OutlinedTextBlock), new FrameworkPropertyMetadata(TextWrapping.NoWrap, OnFormattedTextUpdated));

		private FormattedText formattedText;

		private Geometry textGeometry;

		private Pen pen;

		public Brush Fill
		{
			get
			{
				return (Brush)GetValue(FillProperty);
			}
			set
			{
				SetValue(FillProperty, value);
			}
		}

		public FontFamily FontFamily
		{
			get
			{
				return (FontFamily)GetValue(FontFamilyProperty);
			}
			set
			{
				SetValue(FontFamilyProperty, value);
			}
		}

		[TypeConverter(typeof(FontSizeConverter))]
		public double FontSize
		{
			get
			{
				return (double)GetValue(FontSizeProperty);
			}
			set
			{
				SetValue(FontSizeProperty, value);
			}
		}

		public FontStretch FontStretch
		{
			get
			{
				return (FontStretch)GetValue(FontStretchProperty);
			}
			set
			{
				SetValue(FontStretchProperty, value);
			}
		}

		public FontStyle FontStyle
		{
			get
			{
				return (FontStyle)GetValue(FontStyleProperty);
			}
			set
			{
				SetValue(FontStyleProperty, value);
			}
		}

		public FontWeight FontWeight
		{
			get
			{
				return (FontWeight)GetValue(FontWeightProperty);
			}
			set
			{
				SetValue(FontWeightProperty, value);
			}
		}

		public Brush Stroke
		{
			get
			{
				return (Brush)GetValue(StrokeProperty);
			}
			set
			{
				SetValue(StrokeProperty, value);
			}
		}

		public double StrokeThickness
		{
			get
			{
				return (double)GetValue(StrokeThicknessProperty);
			}
			set
			{
				SetValue(StrokeThicknessProperty, value);
			}
		}

		public string Text
		{
			get
			{
				return (string)GetValue(TextProperty);
			}
			set
			{
				SetValue(TextProperty, value);
			}
		}

		public TextAlignment TextAlignment
		{
			get
			{
				return (TextAlignment)GetValue(TextAlignmentProperty);
			}
			set
			{
				SetValue(TextAlignmentProperty, value);
			}
		}

		public TextDecorationCollection TextDecorations
		{
			get
			{
				return (TextDecorationCollection)GetValue(TextDecorationsProperty);
			}
			set
			{
				SetValue(TextDecorationsProperty, value);
			}
		}

		public TextTrimming TextTrimming
		{
			get
			{
				return (TextTrimming)GetValue(TextTrimmingProperty);
			}
			set
			{
				SetValue(TextTrimmingProperty, value);
			}
		}

		public TextWrapping TextWrapping
		{
			get
			{
				return (TextWrapping)GetValue(TextWrappingProperty);
			}
			set
			{
				SetValue(TextWrappingProperty, value);
			}
		}

		public OutlinedTextBlock()
		{
			TextDecorations = new TextDecorationCollection();
		}

		protected override void OnRender(DrawingContext drawingContext)
		{
			EnsureGeometry();
			drawingContext.DrawGeometry(null, pen, textGeometry);
		}

		protected override Size MeasureOverride(Size availableSize)
		{
			EnsureFormattedText();
			if (formattedText == null)
			{
				return new Size(10.0, 10.0);
			}
			formattedText.MaxTextWidth = Math.Min(3579139.0, availableSize.Width);
			formattedText.MaxTextHeight = availableSize.Height;
			return new Size(formattedText.Width, formattedText.Height);
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			EnsureFormattedText();
			if (formattedText == null)
			{
				return finalSize;
			}
			formattedText.MaxTextWidth = finalSize.Width;
			formattedText.MaxTextHeight = Math.Max(1.0, finalSize.Height);
			textGeometry = null;
			return finalSize;
		}

		private static void OnFormattedTextInvalidated(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
		{
			OutlinedTextBlock outlinedTextBlock = (OutlinedTextBlock)dependencyObject;
			outlinedTextBlock.formattedText = null;
			outlinedTextBlock.textGeometry = null;
			outlinedTextBlock.InvalidateMeasure();
			outlinedTextBlock.InvalidateVisual();
		}

		private static void OnFormattedTextUpdated(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
		{
			OutlinedTextBlock outlinedTextBlock = (OutlinedTextBlock)dependencyObject;
			outlinedTextBlock.UpdateFormattedText();
			outlinedTextBlock.textGeometry = null;
			outlinedTextBlock.InvalidateMeasure();
			outlinedTextBlock.InvalidateVisual();
		}

		private void EnsureFormattedText()
		{
			if (formattedText == null && Text != null)
			{
				formattedText = new FormattedText(Text, CultureInfo.CurrentUICulture, base.FlowDirection, new Typeface(FontFamily, FontStyle, FontWeight, FontStretches.Normal), FontSize, Brushes.Black);
				UpdateFormattedText();
			}
		}

		private void UpdateFormattedText()
		{
			if (formattedText != null)
			{
				formattedText.MaxLineCount = ((TextWrapping == TextWrapping.NoWrap) ? 1 : int.MaxValue);
				formattedText.TextAlignment = TextAlignment;
				formattedText.Trimming = TextTrimming;
				formattedText.SetFontSize(FontSize);
				formattedText.SetFontStyle(FontStyle);
				formattedText.SetFontWeight(FontWeight);
				formattedText.SetFontFamily(FontFamily);
				formattedText.SetFontStretch(FontStretch);
				formattedText.SetTextDecorations(TextDecorations);
			}
		}

		private void EnsureGeometry()
		{
			if (textGeometry == null && formattedText != null)
			{
				EnsureFormattedText();
				textGeometry = formattedText.BuildGeometry(new Point(0.0, 0.0));
				pen = new Pen(Stroke, StrokeThickness);
				pen.LineJoin = PenLineJoin.Round;
				pen.MiterLimit = 10.0;
			}
		}
	}
}
