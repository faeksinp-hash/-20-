using System.Windows;
using System.Windows.Controls;
using Porjai20.ViewModels;

namespace Porjai20.Views
{
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();

            Loaded += async (s, e) =>
            {
                if (DataContext is DashboardViewModel vm)
                {
                    await vm.LoadData();
                }
            };

            IsVisibleChanged += async (s, e) =>
            {
                if (IsVisible && DataContext is DashboardViewModel vm)
                {
                    await vm.LoadData();
                }
            };
        }
    }
}
