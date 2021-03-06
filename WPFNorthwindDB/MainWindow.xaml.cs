using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
                var suppliers = listBox.SelectedItem as Supplier;
                var employee = lstEmployees.SelectedItem as Employee;

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

        private void BtnShowCustomer_OnClick(object sender, RoutedEventArgs e)
        {
            if (lstEmployees.SelectedIndex != -1)
            {
                var employee = lstEmployees.SelectedItem as Employee;
                
                lblSelectedEmployeeFirstName.Content = employee.FirstName;
                lblSelectedEmployeeLastName.Content = employee.LastName;

                lstCustomers.ItemsSource = db.Orders
                    .Include(x => x.Employee)
                    .Include(x => x.Customer)
                    .Where(x => x.Employee == employee)
                    .Select(x => x.Customer)
                    .Distinct()
                    .ToList();
            }
        }

        private void GrdProducts_OnSelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (grdProducts.SelectedIndex != -1)
            {
                var grdProductsSelectedItem = grdProducts.SelectedItem as Product;
                txtProductName.Text = grdProductsSelectedItem.ProductName;
                txtProductName.IsEnabled = false;
                txtNewPrice.Text = grdProductsSelectedItem.UnitPrice.ToString();
            }
        }

        private void btnUpdate_OnClick(object sender, RoutedEventArgs e)
        {
            if (grdProducts.SelectedIndex != -1)
            {
                var grdProductsSelectedItem = grdProducts.SelectedItem as Product;

                grdProductsSelectedItem.UnitPrice =  Decimal.Parse(txtNewPrice.Text);

                db.SaveChanges();
            }
        }

        private void btnAdd_OnClick(object sender, RoutedEventArgs e)
        {
            var productName = txtNewProductName.Text;
            var supplier = db.Suppliers.Single(x => x.CompanyName == txtSupplier.Text);
            var catagory = db.Categories.Single(x => x.CategoryName == txtCatagory.Text);

            if (catagory != null && supplier != null)
            {
                var product = new Product()
                {
                    ProductName = productName,
                    SupplierId = supplier.SupplierId,
                    Supplier = supplier,
                    CategoryId = catagory.CategoryId,
                    Category = catagory
                };

                db.Products.Add(product);

                db.SaveChanges();
            }
        }
}
