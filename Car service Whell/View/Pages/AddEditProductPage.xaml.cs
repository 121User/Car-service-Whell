using Microsoft.Win32;
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
	/// Логика взаимодействия для AddEditProductPage.xaml
	/// </summary>
	public partial class AddEditProductPage : Page
	{
		private Model.Entities bd = Helper.getContex();

		Model.Product product = new Model.Product();

		public AddEditProductPage(Model.Product currentProduct)
		{
			InitializeComponent();

			if(currentProduct != null) //проверка: пустой ли объект с прошлой страницы
			{
				product = currentProduct;

				tbArticle.IsEnabled = false; //запрет редактирования артикула
			}
			else btnDeleteProduct.Content = "Отмена";

			DataContext = product;

			//вывод в ComboBox списка категорий товаров
			cmbCategory.ItemsSource = bd.ProductCategory.Select(productCategory => productCategory.Category).ToList();
			cmbCategory.SelectedIndex = product.ProductCategory - 1;
			//вывод в ComboBox списка наименований товаров
			cmbName.ItemsSource = bd.ProductName.Select(productName => productName.Name).ToList();
			cmbName.SelectedIndex = product.ProductName - 1;
			//вывод в ComboBox списка единиц измерения товаров
			cmbUnit.ItemsSource = bd.ProductUnit.Select(productUnit => productUnit.Unit).ToList();
			cmbUnit.SelectedIndex = product.Unit - 1;
			//вывод в ComboBox списка поставщиков товаров
			cmbSupplier.ItemsSource = bd.ProductSupplier.Select(productSupplier => productSupplier.Supplier).ToList();
			cmbSupplier.SelectedIndex = product.Supplier - 1;
			//вывод в ComboBox списка производителей товаров
			cmbManufacturer.ItemsSource = bd.ProductManufacturer.Select(productManufacturer => productManufacturer.Manufacturer).ToList();
			cmbManufacturer.SelectedIndex = product.ProductManufacturer - 1;
		}


		private void btnEnterImage_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog GetImageDialog = new OpenFileDialog(); //открытие диалогового окна

			GetImageDialog.Filter = "Файлы изображений: (*.png, *.jpg, *.jpeg,)| *.png; *.jpg; *.jpeg"; // фильтр видимости файлов
			GetImageDialog.InitialDirectory = "D:\\Колледж 1\\Автосервис - Руль\\Car service Whell\\Car service Whell\\Resources"; //путь к папке ресурсов проекта
			if(GetImageDialog.ShowDialog() == true)
			{
				if(GetImageDialog.FileName.StartsWith("D:\\Колледж 1\\Автосервис - Руль\\Car service Whell\\Car service Whell\\Resources"))
                {
					product.ProductImage = GetImageDialog.SafeFileName; //добавление имени выбранного фото в БД

					//Обновление изображения
					imgProduct.Source = new BitmapImage(new Uri(product.ImgPath));
				}
                else
                {
					MessageBox.Show("Выберите изображение из ресурсов приложения", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
				}
			}
		}

		private void btnDeleteProduct_Click(object sender, RoutedEventArgs e)
		{
			if (btnDeleteProduct.Content.ToString() == "Отмена")
			{
				NavigationService.GoBack();
			}
			else if(MessageBox.Show($"Вы действительно хотите удалить {product.ProductNameStr}?", "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
			{
				try
				{
					bd.Product.Remove(product);
					bd.SaveChanges();
					MessageBox.Show("Запись удалена!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
					NavigationService.GoBack();
				}
				catch(Exception ex)
				{
					MessageBox.Show(ex.Message.ToString(), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}

		private void btnSaveProduct_Click(object sender, RoutedEventArgs e)
		{
			StringBuilder errors = new StringBuilder();

			//Проверка введенных данных и обновление данных товара
			if (tbArticle.Text.Length != 6)
				errors.AppendLine("Длина артикля товара должна состоять из 6 символов!");

			if (cmbName.SelectedIndex + 1 != 0)
				product.ProductName = cmbName.SelectedIndex + 1;
			else errors.AppendLine("Не выбрано наименование!");

			if (cmbCategory.SelectedIndex + 1 != 0)
				product.ProductCategory = cmbCategory.SelectedIndex + 1;
			else errors.AppendLine("Не выбрана категория!");

			if (!int.TryParse(tbCountInStock.Text, out int countInStock))
				errors.AppendLine("Количество на складе указано некорректно!");

			if (cmbUnit.SelectedIndex + 1 != 0)
				product.Unit = cmbUnit.SelectedIndex + 1;
			else errors.AppendLine("Не выбрана единица измерения!");

			if (tbCountInPack.Text.Length > 0)
            {
				if (!int.TryParse(tbCountInPack.Text, out int countInPack))
					errors.AppendLine("Количество в упаковке указано некорректно!");
			}

			if (tbMinCount.Text.Length > 0)
			{
				if (!int.TryParse(tbMinCount.Text, out int minCount))
					errors.AppendLine("Минимальное количество указано некорректно!");
			}

			if (cmbSupplier.SelectedIndex + 1 != 0)
				product.Supplier = cmbSupplier.SelectedIndex + 1;
			else errors.AppendLine("Не выбран поставщик!");

			if (cmbManufacturer.SelectedIndex + 1 != 0)
				product.ProductManufacturer = cmbManufacturer.SelectedIndex + 1;
			else errors.AppendLine("Не выбран производитель!");

			if (!int.TryParse(tbMaxDiscount.Text, out int maxDiscount))
				errors.AppendLine("Максимальная скидка указана некорректно!");

			if (tbDiscount.Text.Length > 0)
			{
				if (!int.TryParse(tbDiscount.Text, out int discount))
					errors.AppendLine("Размер действующей скидки указан некорректно!");
			}

			if (!decimal.TryParse(tbCost.Text, out decimal cost))
				errors.AppendLine("Стоимость за единицу указана некорректно!");

			if(tbDescription.Text.Length == 0)
				errors.AppendLine("Описание не должно быть пустым!");


			if (product.ProductCost < 0)
				errors.AppendLine("Стоимость не может быть отрицательной!");
			if (product.ProductQuantityInStock < 0)
				errors.AppendLine("Количество на складе не может быть отрицательным!");
			if (product.CountInPack < 0)
				errors.AppendLine("Количество в упаковке не может быть отрицательным!");
			if (product.MinCount < 0)
				errors.AppendLine("Минимальное количество не может быть отрицательным!");
			if (product.MaxDiscountAmount < 0)
				errors.AppendLine("Размер максимальной скидки не может быть отрицательным!");
			if (product.ProductDiscountAmount < 0)
				errors.AppendLine("Размер действующей скидки не может быть отрицательным!");

			if (product.ProductDiscountAmount > product.MaxDiscountAmount)
				errors.AppendLine("Действующая скидка на товар не может быть больше максимальной скидки!");

			//Вывод ошибок
			if (errors.Length > 0)
			{
				MessageBox.Show(errors.ToString()); //выводим ошибки
				return;
			}

			//Проверка уникальности акртикля при добавлении товара
			List<string> listProductArticle = bd.Product.Select(p => p.ProductArticleNumber).ToList();
			bool uniqueArticle = true;
			foreach (string article in listProductArticle)
            {
				if (article == product.ProductArticleNumber)
                {
					uniqueArticle = false; 
					break;
                }
            }
            if (uniqueArticle) 
				bd.Product.Add(product);
			else if (btnDeleteProduct.Content.ToString() == "Отмена")
            {
				MessageBox.Show("Товар с таким артиклем уже существует!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			try
			{
				bd.SaveChanges();
				MessageBox.Show("Информация сохранена!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information); //сохраняем данные в БД
				NavigationService.GoBack();
			}
			catch(Exception ex)
            {
				MessageBox.Show(ex.Message.ToString(), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); //выводим ошибки
            }
		}
	}
}
