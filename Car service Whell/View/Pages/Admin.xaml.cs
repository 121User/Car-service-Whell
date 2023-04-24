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
    /// Логика взаимодействия для Admin.xaml
    /// </summary>
    public partial class Admin : Page
    {
        private Model.Entities bd = Helper.getContex();

        Model.User user = new Model.User(); //создаем пустой объект пользователя

        public Admin(Model.User currentUser)
        {
            InitializeComponent();

            var product = bd.Product.ToList(); //обращаемся к таблице "Товары"
            lvProduct.ItemsSource = product; //передаем таблицу в ListView

            DataContext = this; //привязываем контекст данных к коду, чтобы обратиться к массивам

            tblResultAmount.Text = product.Count().ToString(); //вывод количества записей, подходящих под условия фильтрации и поиска
			tblTotalNumber.Text = product.Count().ToString(); //вывод количества всех записей из таблицы

			user = currentUser;

            UpdateData();
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

		//список сортировок
		public string[] SortingList { get; set; } =
		{
			"Без сортировки",
			"Стоимость по возрастанию",
			"Стоимость по убыванию"
		};
		//список фильтров
		public string[] FilterList { get; set; } =
		{
			"Все диапазоны",
			"0% - 9,99%",
			"10% - 14,99%",
			"15% и более"
		};

		public void UpdateData()
		{
			var result = bd.Product.ToList(); //обращаемся к таблице "Товары"
											  //реализация сортировки по цене
			switch (cbSorting.SelectedIndex)
			{
				case (1):
					result = result.OrderBy(product => product.ProductCost).ToList(); //сортировка по возрастанию
					break;
				case (2):
					result = result.OrderByDescending(product => product.ProductCost).ToList(); //сортировка по убыванию
					break;
			}
			//реализация фильтра по скидке
			switch (cbFilter.SelectedIndex)
			{
				case (1):
					result = result.Where(product => product.ProductDiscountAmount >= 0 && product.ProductDiscountAmount < 10).ToList(); //фильтрация 0% - 9,99%
					break;
				case (2):
					result = result.Where(product => product.ProductDiscountAmount >= 10 && product.ProductDiscountAmount < 15).ToList(); //фильтрация 10% - 14,99%
					break;
				case (3):
					result = result.Where(product => product.ProductDiscountAmount >= 15).ToList(); //фильтрация 15% и более
					break;
			}
			//реализация поиска
			result = result.Where(product => product.ProductNameStr.ToLower().Contains(tbSearch.Text.ToLower())).ToList();

			//вывод результата
			lvProduct.ItemsSource = result;

			tblResultAmount.Text = result.Count().ToString(); //вывод количества всех записей из таблицы после применения поиска, сортировки, фильтрации

		}

		private void tbSearch_SelectionChanged(object sender, RoutedEventArgs e)
		{
			UpdateData();
		}

		private void cbSorting_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			UpdateData();
		}

		private void cbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			UpdateData();
		}


		List<Model.Product> orderProducts = new List<Model.Product>(); //Список продуктов в заказе
		//Добавление продуктов в заказ
		private void btnAddProduct_Click(object sender, RoutedEventArgs e)
		{
			//Получение массива выбранных товаров
			var selectedProducts = lvProduct.SelectedItems.Cast<Model.Product>().ToArray();
			//Добавление выбранных продуктов в заказ
			foreach (var item in selectedProducts)
			{
				if (!orderProducts.Contains(item))
				{
					item.ProductCountInOrder = 1;
					orderProducts.Add(item);
				}
			}
			//Включение видимости кнопки Заказ, если список продуктов заказа не пустой
			if (orderProducts.Count > 0)
			{
				btnOrder.Visibility = Visibility.Visible;
			}
		}

		private void btnOrder_Click(object sender, RoutedEventArgs e)
		{
			OrderWindow order = new OrderWindow(null, null, this, orderProducts, user);
			order.ShowDialog();
		}
		//Очистка списка продуктов в заказе и выключение видимости для кнопки Заказ
		public void clearOrderProducts()
		{
			orderProducts.Clear();
			btnOrder.Visibility = Visibility.Hidden;
		}



        private void btnAllOrders_Click(object sender, RoutedEventArgs e)
        {
			NavigationService.Navigate(new AllOrders(user));
		}



		//Переход на странциу добавления товара при нажатии на кнопку btnAddNewProduct
		private void btnAddNewProduct_Click(object sender, RoutedEventArgs e)
        {
			NavigationService.Navigate(new AddEditProductPage(null)); //для добавления передаем null на страницу AddEditProductPage 
		}
		//Переход на страницу изменения товара при двойном клике
		private void lvProduct_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
			NavigationService.Navigate(new AddEditProductPage(lvProduct.SelectedItem as Model.Product)); //для редактирования передаем данные о товаре на страницу AddEditProductPage 
		}

		//Обновление списка продуктов при возврате на страницу Admin
        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
			if(Visibility == Visibility.Visible)
            {
				bd.ChangeTracker.Entries().ToList().ForEach(p => p.Reload());
				lvProduct.ItemsSource = bd.Product.ToList();

				var product = bd.Product.ToList(); //обращаемся к таблице "Товары"
				tblTotalNumber.Text = product.Count().ToString(); //вывод количества всех записей из таблицы
				UpdateData();
			}
        }
    }
}
