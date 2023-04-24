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
using System.Windows.Shapes;
using Car_service_Whell.View.Pages; //подключение папки со страницами приложения


namespace Car_service_Whell.View.Windows
{
    /// <summary>
    /// Логика взаимодействия для OrderWindow.xaml
    /// </summary>
    public partial class OrderWindow : Window
    {
        List<Model.Product> Listproduct = new List<Model.Product>();

        public OrderWindow(Client client, Manager manager, Admin admin, List<Model.Product> products, Model.User user)
        {
            InitializeComponent();

            FrmOrder.Navigate(new OrderPage(client, manager, admin, products, user));
        }
    }
}
