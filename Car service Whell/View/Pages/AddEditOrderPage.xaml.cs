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
    /// Логика взаимодействия для AddEditOrderPage.xaml
    /// </summary>
    public partial class AddEditOrderPage : Page
    {
        private Model.Entities bd = Helper.getContex();

        Model.Order order = new Model.Order();

		List<Model.Product> listAllProducts = new List<Model.Product>(); //Список всех товаров
		List<Model.Product> listProductInOrder = new List<Model.Product>(); //Список товаров заказа

		public AddEditOrderPage(Model.Order currentOrder)
        {
            InitializeComponent();

            if (currentOrder != null) //проверка: пустой ли объект с прошлой страницы
            {
                order = currentOrder;

				//Поиск товаров текущего заказа
				List<Model.OrderProduct> listOrderProduct = bd.OrderProduct.ToList();
				foreach(var orderProduct in listOrderProduct)
                {
					if(order.OrderID == orderProduct.OrderID)
                    {
						var product = bd.Product.Where(p => p.ProductArticleNumber == orderProduct.ProductArticleNumber).First();
						product.ProductCountInOrder = orderProduct.Quantity;
						listProductInOrder.Add(product);
					}
                }
				lvProductsInOrder.ItemsSource = listProductInOrder;

				dpOrderDate.SelectedDate = order.OrderDate;
				dpOrderDeliveryDate.SelectedDate = order.OrderDeliveryDate;

                tbID.IsEnabled = false; //запрет редактирования артикула
            }
            else
            {
				dpOrderDate.SelectedDate = DateTime.Today;
				dpOrderDeliveryDate.SelectedDate = DateTime.Today;

				btnDeleteOrder.Content = "Отмена";
			}

			listAllProducts = bd.Product.ToList();
			lvAllProducts.ItemsSource = listAllProducts;

			DataContext = order;

            //вывод в ComboBox списка статусов
            cmbStatus.ItemsSource = bd.OrderStatus.Select(orderStatus => orderStatus.Status).ToList();
            cmbStatus.SelectedIndex = order.OrderStatus - 1;
            //вывод в ComboBox списка пунктов выдачи
            cmbPickupPoint.ItemsSource = bd.PickupPoint.Select(pickupPoint => pickupPoint.Address).ToList();
            cmbPickupPoint.SelectedIndex = order.OrderPickupPoint - 1;
        }

        private void btnDeleteOrder_Click(object sender, RoutedEventArgs e)
        {
			if(btnDeleteOrder.Content.ToString() == "Отмена")
            {
				NavigationService.GoBack();
			}
			else if (MessageBox.Show($"Вы действительно хотите удалить заказ?", "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    bd.Order.Remove(order);
                    bd.SaveChanges();
                    MessageBox.Show("Запись удалена!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    NavigationService.GoBack();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString(), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


		private void btnSaveOrder_Click(object sender, RoutedEventArgs e)
		{
			StringBuilder errors = new StringBuilder();

			//Проверка введенных данных и обновление данных заказа
			if (!int.TryParse(tbID.Text, out int id) || id == 0)
				errors.AppendLine("ID указано некорректно!");

			if (cmbStatus.SelectedIndex + 1 != 0)
				order.OrderStatus = cmbStatus.SelectedIndex + 1;
			else errors.AppendLine("Не выбран статус!");

			if (cmbPickupPoint.SelectedIndex + 1 != 0)
				order.OrderPickupPoint = cmbPickupPoint.SelectedIndex + 1;
			else errors.AppendLine("Не выбран пункт выдачи!");

			if (dpOrderDate.SelectedDate.Value == null)
				errors.AppendLine("Дата заказа указана некорректно!");
			else order.OrderDate = dpOrderDate.SelectedDate.Value;

			if (dpOrderDeliveryDate.SelectedDate.Value == null)
				errors.AppendLine("Дата доставки заказа указана некорректно!");
			else order.OrderDeliveryDate = dpOrderDeliveryDate.SelectedDate.Value;
		
			order.ClientFullName = tbClient.Text;

			if (!int.TryParse(tbReceiptCode.Text, out int receiptCode) || tbReceiptCode.Text.Length !=3)
				errors.AppendLine("Код подтверждения указан некорректно!");


			if (order.OrderID < 0)
				errors.AppendLine("ID не может быть отрицательным!");
			if (order.ReceiptCode < 0)
				errors.AppendLine("Код подтверждения не может быть отрицательным!");

			if (listProductInOrder.Count == 0)
				errors.AppendLine("Заказ должен содержать товары!");

			//Вывод ошибок
			if (errors.Length > 0)
			{
				MessageBox.Show(errors.ToString()); //выводим ошибки
				return;
			}

			//Проверка уникальности акртикля при добавлении товара
			List<int> listOrderID = bd.Order.Select(o => o.OrderID).ToList();
			bool uniqueID = true;
			foreach (int oldProductID in listOrderID)
			{
				if (oldProductID == order.OrderID)
				{
					uniqueID = false;
					break;
				}
			}
			if (uniqueID)
				bd.Order.Add(order);
			else if (btnDeleteOrder.Content.ToString() == "Отмена")
			{
				MessageBox.Show("Заказ с таким ID уже существует!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}



			List<Model.OrderProduct> listOrderProduct = bd.OrderProduct.ToList();
			//Удаление из БД всех товаров, связанных с текущим заказом
			foreach (var orderProduct in listOrderProduct)
			{
				if (order.OrderID == orderProduct.OrderID)
				{
					bd.OrderProduct.Remove(orderProduct);
				}
			}
			
			//Добавление всех товаров заказа в БД
			foreach (var product in listProductInOrder)
            {
				Model.OrderProduct orderProduct = new Model.OrderProduct();
				orderProduct.OrderID = order.OrderID;
				orderProduct.ProductArticleNumber = product.ProductArticleNumber;
				orderProduct.Quantity = product.ProductCountInOrder;

				bd.OrderProduct.Add(orderProduct);
            }

			try
			{
				bd.SaveChanges();
				MessageBox.Show("Информация сохранена!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information); //сохраняем данные в БД
				NavigationService.GoBack();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message.ToString(), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); //выводим ошибки
			}
		}


		//Увеличение (на 1) количества выбранных товаров в заказе
		private void btnUpCountProduct_Click(object sender, RoutedEventArgs e)
		{
			//Получение массива выбранных товаров
			var selectedProducts = lvProductsInOrder.SelectedItems.Cast<Model.Product>().ToArray();
			foreach (var item in selectedProducts)
			{
				int index = listProductInOrder.IndexOf(item); //Поиск индекаса выделенного элемента 
				item.ProductCountInOrder += 1;
				listProductInOrder[index] = item; //Обновление в списке
			}
			lvProductsInOrder.ItemsSource = null;
			lvProductsInOrder.ItemsSource = listProductInOrder;
		}

		//Уменьшение (на 1) количества выбранных товаров в заказе
		private void btnDownCountProduct_Click(object sender, RoutedEventArgs e)
		{
			//Получение массива выбранных товаров
			var selectedProducts = lvProductsInOrder.SelectedItems.Cast<Model.Product>().ToArray();
			foreach (var item in selectedProducts)
			{
				int index = listProductInOrder.IndexOf(item); //Поиск индекаса выделенного элемента 
				item.ProductCountInOrder -= 1;

				if (item.ProductCountInOrder == 0)
				{
					listProductInOrder.Remove(item); //Удаление, если количество равно 0
				}
				else
				{
					listProductInOrder[index] = item; //Обновление в списке
				}
			}
			lvProductsInOrder.ItemsSource = null;
			lvProductsInOrder.ItemsSource = listProductInOrder;

			//блокировка кнопки сохранения заказа, если заказ пуст
			if (listProductInOrder.Count == 0)
			{
				btnSaveOrder.IsEnabled = false;
			}
		}

		//удаление товаров из заказа
		private void btnDeleteProduct_Click(object sender, RoutedEventArgs e)
		{
			var selectedProducts = lvProductsInOrder.SelectedItems.Cast<Model.Product>().ToArray();//Получение массива выбранных товаров
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
					listProductInOrder.Remove(item);
				}
				lvProductsInOrder.ItemsSource = null;
				lvProductsInOrder.ItemsSource = listProductInOrder;
			}
		}

		
		//Добавление товаров в заказ
        private void btnAddInOrder_Click(object sender, RoutedEventArgs e)
        {
			StringBuilder errors = new StringBuilder();

			//Получение массива выбранных товаров в списке всех товаров
			var selectedProducts = lvAllProducts.SelectedItems.Cast<Model.Product>().ToArray();
			foreach (var item in selectedProducts)
			{
				//Добавление в заказ, если еще не добавлен
				if (!listProductInOrder.Contains(item))
				{
					item.ProductCountInOrder = 1;
					listProductInOrder.Add(item);
				}
				else errors.AppendLine(string.Format("Товар {0} уже добавлен в заказ", item.ProductArticleNumber));
			}
			//Вывод ошибок
			if (errors.Length > 0)
			{
				MessageBox.Show(errors.ToString()); //выводим ошибки
			}

			lvProductsInOrder.ItemsSource = null;
			lvProductsInOrder.ItemsSource = listProductInOrder;
		}
    }
}
