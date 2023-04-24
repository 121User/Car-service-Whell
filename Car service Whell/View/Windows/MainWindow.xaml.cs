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
using Car_service_Whell.View.Pages; //подключение папки со страницами приложения

namespace Car_service_Whell
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            FrmMain.Navigate(new Authorization()); //открыть страницу авторизации
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            FrmMain.Navigate(new Authorization()); //переход обратно на страницу авторизации
        }

        //Удаление с окна кнопки выхода, если клиент находится на странице авторизации
        private void FrmMain_ContentRendered(object sender, EventArgs e)
        {
            if (FrmMain.Content is Authorization)
            {
                btnExit.Visibility = Visibility.Hidden;
            }
            else
            {
                btnExit.Visibility = Visibility.Visible;
            }
        }
    }
}
