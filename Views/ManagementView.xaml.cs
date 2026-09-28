using System.Windows.Controls;
using System.Windows.Input;
using Porjai20.Models;
using Porjai20.ViewModels;

namespace Porjai20.Views
{
    public partial class ManagementView : UserControl
    {
        public ManagementView()
        {
            InitializeComponent();
        }

        private void ProductRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row && row.Item is Product product)
            {
                if (DataContext is ProductViewModel vm)
                {
                    vm.SelectedProduct = product;
                    vm.ExecuteOpenEditProductModal(product);
                }
            }
        }

        private void CategoryRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row && row.Item is ProductCategory category)
            {
                if (DataContext is ProductViewModel vm)
                {
                    vm.SelectedCategory = category;
                    vm.ExecuteOpenManageCategoryModal(category);
                }
            }
        }
    }
}