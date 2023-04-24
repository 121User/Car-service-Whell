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

namespace Car_service_Whell.View.Pages
{
    /// <summary>
    /// Логика взаимодействия для OrderTicketPage.xaml
    /// </summary>
    public partial class OrderTicketPage : Page
    {
        List<Model.Product> productList = new List<Model.Product>(); //пустой список продуктов

        public OrderTicketPage(Model.Order currentOrder, List<Model.Product> products)
        {
            InitializeComponent();

            productList = products; //передаем в пустой список данные с прошлой страницы
            DataContext = currentOrder; //привязываем контекст данных к оформленному заказу

            tblPickupPoint.Text = currentOrder.PickupPoint.Address; //вывод адреса пункта выдачи

            string result = "";
            foreach (var product in productList)
            {
                result += (result == "" ? "" : "\n") + product.ProductNameStr + " x " + product.ProductCountInOrder; //вывод названий товаров, оформленных в заказе (если result пуст добавляем "", иначе добавляем ", ")

            }
            tblProductList.Text = result.ToString();

            double total = productList.Sum(product => Convert.ToDouble(product.ProductCost) - Convert.ToDouble(product.ProductCost) * Convert.ToDouble(product.ProductDiscountAmount / 100.00));
            tblCost.Text = total.ToString() + " руб."; //выводим итоговую суммму заказа

            var discountAmount = productList.Sum(product => product.ProductDiscountAmount);
            tblDiscountAmount.Text = discountAmount.ToString() + " %"; //вывод суммы скидки заказа
        }
        //сохранение PDF документа
        private void btnSaveDocument_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog printDialod = new PrintDialog();
            if(printDialod.ShowDialog() == true)
            {
                IDocumentPaginatorSource idp = fdOrderTicket;
                printDialod.PrintDocument(idp.DocumentPaginator, Title);
            }
        }
    }
}
