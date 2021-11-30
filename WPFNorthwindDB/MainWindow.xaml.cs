using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.EntityFrameworkCore;
using NorthwindDB;

namespace WPFNorthwindDB;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
    public partial class MainWindow : Window
{
    private NorthwindContext db;
    
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            db = new NorthwindContext();
            
            listBox.ItemsSource = db.Suppliers.OrderBy(x => x.CompanyName).ToList();

            lstEmployees.ItemsSource = db.Employees.OrderBy(x => x.LastName).ToList();
        }

        private void ListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Filter();
        }

        private void Filter()
        {
            if (listBox.SelectedIndex != -1 && lstEmployees.SelectedIndex != -1)
            {
                var suppliers = listBox.SelectedItems.OfType<Supplier>().First();
                var employee = lstEmployees.SelectedItems.OfType<Employee>().First();

                grdProducts.ItemsSource = db.OrderDetails
                    .Include(x => x.Product)
                    .Include(x => x.Order)
                    .Include(x => x.Product.Category)
                    .Include(x => x.Product.Supplier)
                    .Include(x => x.Order.Employee)
                    .Where(x => x.Order.Employee.LastName == employee.LastName)
                    .Select(x => x.Product)
                    .Where(x => x.Supplier.CompanyName == suppliers.CompanyName)
                    .Where(x => x.UnitPrice <= (int) sldMaxPrice.Value)
                    .Where(x => x.UnitsInStock >= (int) sldMinStock.Value)
                    .Distinct()
                    .ToList();
            }
        }

        private void Slides_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            lblMaxPrice.Content = sldMaxPrice.Value;
            lblMinStock.Content = sldMinStock.Value;
            
            Filter();
        }

        private void TxtFilter_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            listBox.ItemsSource = db.Suppliers
                .Where(x => x.CompanyName.StartsWith(txtFilter.Text))
                .OrderBy(x => x.CompanyName)
                .ToList();
        }
}
