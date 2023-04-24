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
using Car_service_Whell.View.Windows; //подключение папки с окнами приложения


namespace Car_service_Whell.View.Pages
{
    /// <summary>
    /// Логика взаимодействия для AllOrders.xaml
    /// </summary>
    public partial class AllOrders : Page
    {
        private Model.Entities bd = Helper.getContex();

        Model.User user = new Model.User(); //создаем пустой объект пользователя

        public AllOrders(Model.User currentUser)
        {
            InitializeComponent();

            var orders = bd.Order.ToList(); //обращаемся к таблице "Товары"
            lvOrder.ItemsSource = orders; //передаем таблицу в ListView

            DataContext = this; //привязываем контекст данных к коду, чтобы обратиться к массивам

            tblTotalNumber.Text = orders.Count().ToString(); //вывод количества всех записей из таблицы (общее количество)

            user = currentUser;

            User();
        }
        //вывод полного имени пользователя
        private void User()
        {
            if (this.user != null)
                tblFullnameClient.Text = user.UserSurname.ToString().Trim() + " " + user.UserName.ToString().Trim() + " " + user.UserPatronymic.ToString().Trim();
            else
                tblFullnameClient.Text = "Гость";
        }
        //Переход на предыдущую страницу
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
        //Переход на страницу изменения товара при двойном клике
        private void lvOrder_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            NavigationService.Navigate(new AddEditOrderPage(lvOrder.SelectedItem as Model.Order)); //для редактирования передаем данные о заказе на страницу AddEditOrderPage 
        }

        //Переход на странциу добавления товара
        private void btnAddNewOrder_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddEditOrderPage(null)); //для добавления передаем null на страницу AddEditOrderPage 
        }

        //Обновление списка продуктов при возврате на страницу Admin
        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                bd.ChangeTracker.Entries().ToList().ForEach(o => o.Reload());
                var orders = bd.Order.ToList();
                lvOrder.ItemsSource = orders;
                tblTotalNumber.Text = orders.Count().ToString();
            }
        }
    }
}
