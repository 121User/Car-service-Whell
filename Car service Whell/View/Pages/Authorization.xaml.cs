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
using System.Windows.Threading;

namespace Car_service_Whell.View.Pages
{
    /// <summary>
    /// Логика взаимодействия для Authorization.xaml
    /// </summary>
    public partial class Authorization : Page
    {
        public Authorization()
        {
            InitializeComponent();
        }

		Model.Entities bd = Helper.getContex();

        private int numberUnsuccess = 0; //количество неудачных попыток входа
        private DateTime endLock; //конечное время блокировки входа
        private string captcha = ""; //текст captcha

        private void btnEnterGuest_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Client(null));
        }

        private void btnEnter_Click(object sender, RoutedEventArgs e)
        {
			string login = tbLogin.Text.Trim(); //логин пользователя
			string password = tbPassword.Text.Trim(); //пароль пользователя
            
            //проверка логина и пароля пользователя
            if (bd.User.Any(us => us.UserLogin == login && us.UserPassword == password))
			{
                //проверка captcha
                if (numberUnsuccess == 0 || numberUnsuccess > 0 && tbCaptcha.Text == captcha)
                {
                    Model.User user = bd.User.Where(us => us.UserLogin == login && us.UserPassword == password).FirstOrDefault();
                    string role = bd.Role.Where(r => r.RoleID == user.UserRole).FirstOrDefault().RoleName;
                    MessageBox.Show("Вы вошли под ролью: " + role, "Authorization", MessageBoxButton.OK);
                    LoadForm(role, user);
                }
                else
                {
                    numberUnsuccess += 1;
                    MessageBox.Show("Captcha неверна!", "Authorization", MessageBoxButton.OK, MessageBoxImage.Warning);
                    tbLogin.Text = "";
                    tbPassword.Text = "";
                    tbCaptcha.Text = "";
                    GenerateCaptcha();
                }
            }
            else
			{
                numberUnsuccess += 1;
                MessageBox.Show("Данные неверны!", "Authorization", MessageBoxButton.OK, MessageBoxImage.Warning);
                tbLogin.Text = "";
                tbPassword.Text = "";
                tbCaptcha.Text = "";

                if (numberUnsuccess == 1)
                {
                    GenerateCaptcha();
                    tbCaptcha.Visibility = Visibility.Visible;
                    tblCaptcha.Visibility = Visibility.Visible;
                }
			}

            if (numberUnsuccess > 1)
            {
                //блокировка входа
                tblLock.Visibility = Visibility.Visible;
                tbLogin.IsEnabled = false;
                tbPassword.IsEnabled = false;
                tbCaptcha.IsEnabled = false;
                btnEnterGues.IsEnabled = false;
                btnEnter.IsEnabled = false;
                //включение таймера блокировки входа
                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromMilliseconds(1);
                timer.Tick += TimerTick;
                timer.Start();

                endLock = DateTime.Now.AddSeconds(11);
            }
        }

        //вывод оставшегося времени блокировки
        private void TimerTick(object sender, EventArgs e)
        {
            tblLock.Text = "Вход заблокирован: " + (endLock - DateTime.Now).Seconds;

            if ((endLock - DateTime.Now).Seconds is 0) //если время закончилось, то разблокировка входа
            {
                DispatcherTimer timer = (DispatcherTimer)sender;
                timer.Stop();

                tbLogin.IsEnabled = true;
                tbPassword.IsEnabled = true;
                tbCaptcha.IsEnabled = true;
                btnEnterGues.IsEnabled = true;
                btnEnter.IsEnabled = true;
                tblLock.Visibility = Visibility.Hidden;
                GenerateCaptcha();
            }
        }

        //генерация captcha
        private void GenerateCaptcha()
        {
            String allowchar = "A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z,";
            allowchar += "a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p,q,r,s,t,u,v,w,y,z,";
            allowchar += "1,2,3,4,5,6,7,8,9,0";

            String[] arrayChar = allowchar.Split(',');
            captcha = "";

            Random rnd = new Random();
            for (int i = 0; i < 4; i++)
            {
                captcha += arrayChar[rnd.Next(0, arrayChar.Length)];
            }
            int view = rnd.Next(2); //вид captcha
            switch (view)
            {
                case 0:
                    tblCaptcha.Text = captcha[0] + "\t" + captcha[2] + "\n" + "\t" + captcha[1] + "\t" + captcha[3];
                    break;
                case 1:
                    tblCaptcha.Text = "\t" + captcha[1] + "\t" + captcha[3] + "\n" + captcha[0] + "\t" + captcha[2];
                    break;
            }
            Color[] arrColors = new Color[] {Colors.DarkRed, Colors.DarkBlue, Colors.DarkGreen, Colors.Black};
            Color color = arrColors[rnd.Next(0, arrColors.Length)];
            tblCaptcha.Foreground = new SolidColorBrush(color);
        }

        //проверка роли пользователя
        private void LoadForm(string role, Model.User user)
        {
            switch (role)
            {
                case ("Клиент"): //если роль "Клиент", переходим на страницу Client
                    NavigationService.Navigate(new Client(user)); 
                    break;
                case ("Менеджер"): //если роль "Менеджер", переходим на страницу Client
                    NavigationService.Navigate(new Manager(user));
                    break;
                case ("Администратор"): //если роль "Администратор", переходим на страницу Admin
                    NavigationService.Navigate(new Admin(user));
                    break;
            }
        }
    }
}
