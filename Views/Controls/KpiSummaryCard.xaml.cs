using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Porjai20.Views.Controls
{
    /// <summary>
    /// Interaction logic for KpiSummaryCard.xaml
    /// Reusable modern KPI card component.
    /// </summary>
    public partial class KpiSummaryCard : UserControl
    {
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                nameof(Title),
                typeof(string),
                typeof(KpiSummaryCard),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                nameof(Value),
                typeof(object),
                typeof(KpiSummaryCard),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty SubtitleProperty =
            DependencyProperty.Register(
                nameof(Subtitle),
                typeof(string),
                typeof(KpiSummaryCard),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty IconGlyphProperty =
            DependencyProperty.Register(
                nameof(IconGlyph),
                typeof(string),
                typeof(KpiSummaryCard),
                new PropertyMetadata("📊"));

        public static readonly DependencyProperty IconBackgroundProperty =
            DependencyProperty.Register(
                nameof(IconBackground),
                typeof(Brush),
                typeof(KpiSummaryCard),
                new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0F2FE"))));

        public static readonly DependencyProperty ValueBrushProperty =
            DependencyProperty.Register(
                nameof(ValueBrush),
                typeof(Brush),
                typeof(KpiSummaryCard),
                new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0B2545"))));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public object Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public string Subtitle
        {
            get => (string)GetValue(SubtitleProperty);
            set => SetValue(SubtitleProperty, value);
        }

        public string IconGlyph
        {
            get => (string)GetValue(IconGlyphProperty);
            set => SetValue(IconGlyphProperty, value);
        }

        public Brush IconBackground
        {
            get => (Brush)GetValue(IconBackgroundProperty);
            set => SetValue(IconBackgroundProperty, value);
        }

        public Brush ValueBrush
        {
            get => (Brush)GetValue(ValueBrushProperty);
            set => SetValue(ValueBrushProperty, value);
        }

        public KpiSummaryCard()
        {
            InitializeComponent();
        }
    }
}
