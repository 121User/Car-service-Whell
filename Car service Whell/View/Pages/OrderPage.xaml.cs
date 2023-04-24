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
	/// Логика взаимодействия для OrderPage.xaml
	/// </summary>
	public partial class OrderPage : Page
	{
		private Model.Entities bd = Helper.getContex();

		List<Model.Product> productList = new List<Model.Product>(); //пустой список продуктов

		Client clientPage; //Страница клиента
		Manager managerPage; //Страница менеджера
		Admin adminPage; //Страница администратора

		public OrderPage(Client client, Manager manager, Admin admin, List<Model.Product> products, Model.User user)
		{
			InitializeComponent();

			clientPage = client;
			managerPage = manager;
			adminPage = admin;

			DataContext = this; //привязываем контекст данных к коду
			productList = products;
			lvOrder.ItemsSource = productList; //вывод списка выбранных товаров в ListView

			cmbPickupPiint.ItemsSource = bd.PickupPoint.ToList(); //вывод в ComboBox списка пунктов выдачи
			if (user != null) //если пользователь авторизован
			{
				//вывод ФИО пользователя
				tbUser.Text = user.UserSurname.ToString().Trim() + " " + user.UserName.ToString().Trim() + " " + user.UserPatronymic.ToString().Trim();
			}

			//блокировка кнопки сохранения заказа, если заказ пуст
			if (productList.Count == 0)
				btnOrderSave.IsEnabled = false;
		}

		//получение общей стоимости заказа
		public string Total
		{
			get
			{
				var total = productList.Sum(product => (Convert.ToDouble(product.ProductCost) - Convert.ToDouble(product.ProductCost) * Convert.ToDouble(product.ProductDiscountAmount / 100.00)) * Convert.ToDouble(product.ProductCountInOrder));
				return total.ToString();
			}
		}

		//Увеличение (на 1) количества выбранных товаров в заказе
		private void btnUpCountProduct_Click(object sender, RoutedEventArgs e)
		{
			//Получение массива выбранных товаров
			var selectedProducts = lvOrder.SelectedItems.Cast<Model.Product>().ToArray();
			foreach (var item in selectedProducts)
			{
				int index = productList.IndexOf(item); //Поиск индекаса выделенного элемента 
				item.ProductCountInOrder += 1;
				productList[index] = item; //Обновление в списке
			}
			lvOrder.ItemsSource = null;
			lvOrder.ItemsSource = productList;
			tblTotal.Text = Total + " рублей";
		}

		//Уменьшение (на 1) количества выбранных товаров в заказе
		private void btnDownCountProduct_Click(object sender, RoutedEventArgs e)
		{
			//Получение массива выбранных товаров
			var selectedProducts = lvOrder.SelectedItems.Cast<Model.Product>().ToArray();
			foreach (var item in selectedProducts)
			{
				int index = productList.IndexOf(item); //Поиск индекаса выделенного элемента 
				item.ProductCountInOrder -= 1;
				
				if(item.ProductCountInOrder == 0)
                {
					productList.Remove(item); //Удаление, если количество равно 0
				}
                else
                {
					productList[index] = item; //Обновление в списке
				}
			}
			lvOrder.ItemsSource = null;
			lvOrder.ItemsSource = productList;
			tblTotal.Text = Total + " рублей";

			//блокировка кнопки сохранения заказа, если заказ пуст
			if (productList.Count == 0)
			{
				btnOrderSave.IsEnabled = false;
			}
		}

		//удаление товаров из заказа
		private void btnDeleteProduct_Click(object sender, RoutedEventArgs e)
		{
			var selectedProducts = lvOrder.SelectedItems.Cast<Model.Product>().ToArray();//Получение массива выбранных товаров
			string sign = "";
			//Проверка количества выбранных элементов																			 
			if (selectedProducts.Length == 0) return;
			else if (selectedProducts.Length == 1) sign = "этот элемент";
			else sign = "эти элементы";

			if (MessageBox.Show("Вы уверены, что хотите удалить " + sign + "?", "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
			{
				//Удаление выбранных продуктов из списка 
				foreach (var item in selectedProducts)
                {
					productList.Remove(item);
                }
				lvOrder.ItemsSource = null;
				lvOrder.ItemsSource = productList;
				tblTotal.Text = Total + " рублей";
            }
			//блокировка кнопки сохранения заказа, если заказ пуст
			if (productList.Count == 0)
            {
				btnOrderSave.IsEnabled = false;
			}
		}

		private void btnOrderSave_Click(object sender, RoutedEventArgs e)
		{
			string[] productArticle = productList.Select(product => product.ProductArticleNumber).ToArray(); //получение массива артиклей товаров
			Random random = new Random(); //рандом для получения "Код получения"
			DateTime deliveryDate = DateTime.Now; //дата доставки
			//рассчет даты доставки
			if (productList.Any(product => product.ProductQuantityInStock < 3))
				deliveryDate = deliveryDate.AddDays(6);
			else
				deliveryDate = deliveryDate.AddDays(3);
			//проверка выбор апункта выдачи
			if(cmbPickupPiint.SelectedItem == null)
			{
				MessageBox.Show("Выберите пункт выдачи!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
				return;
			}

			try
			{
				//в пустой объект Order добавляем данные
				Model.Order newOrder = new Model.Order()
				{
					OrderID = Helper.getOrderID(),
					OrderStatus = bd.OrderStatus.Where(status => status.Status == "Новый").First().ID,
					OrderDate = DateTime.Now,
					OrderPickupPoint = cmbPickupPiint.SelectedIndex + 1,
					OrderDeliveryDate = deliveryDate,
					ReceiptCode =random.Next(100, 1000),
					ClientFullName = tbUser.Text,
				};
				bd.Order.Add(newOrder); //добавляем новый заказ в таблицу Order
				
				//Добавление всех записей в таблицу OrderProduct
				foreach(var product in productList)
                {
					//в пустой объект OrderProduct добавляем данные
					Model.OrderProduct newOrderProduct = new Model.OrderProduct
					{
						OrderID = newOrder.OrderID,
						ProductArticleNumber = product.ProductArticleNumber,
						Quantity = product.ProductCountInOrder
					};
					bd.OrderProduct.Add(newOrderProduct); //добавляем новую запись в таблицу OrderProduct
				}
				bd.SaveChanges(); //сохраняем добавленные в БД
				MessageBox.Show("Заказ оформлен!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
				NavigationService.Navigate(new OrderTicketPage(newOrder, productList)); //переходим на страницу талона заказа
				
				//Очистка спсика продуктов в заказе
				productList.Clear();
				if(clientPage != null)
                {
					clientPage.clearOrderProducts();
				}
				else if (managerPage != null)
                {
					managerPage.clearOrderProducts();
                }
				else
				{
					adminPage.clearOrderProducts();
				}
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.Message.ToString(), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning); //если есть какие-то ошибки, выводим их
			}
		}
    }
}
