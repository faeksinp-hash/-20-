using System.Windows;
using System.Windows.Controls;
using Porjai20.ViewModels;

namespace Porjai20.Views
{
    /// <summary>
    /// Interaction logic for PromotionView.xaml
    /// </summary>
    public partial class PromotionView : UserControl
    {
        public PromotionView()
        {
            InitializeComponent();
            Loaded += PromotionView_Loaded;
            IsVisibleChanged += PromotionView_IsVisibleChanged;
        }

        private void PromotionView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is PromotionViewModel vm)
            {
                _ = vm.LoadPromotionsAsync();
            }
        }

        private void PromotionView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is true && DataContext is PromotionViewModel vm)
            {
                _ = vm.LoadPromotionsAsync();
            }
        }
    }
}
