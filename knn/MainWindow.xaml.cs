using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace knn
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public static class Dane
        {
            public static List<Probka> probki = new List<Probka>();
            public static Probka test = null;
            public static void normalizuj()
            {
                double[] max = probki[1].atrybuty.ToArray();
                double[] min = probki[1].atrybuty.ToArray();

                for (int i = 0; i < max.Length; i++)
                {
                    foreach (Probka probka in probki)
                    {
                        if (probka.atrybuty[i]>max[i])
                        {
                            max[i] = probka.atrybuty[i];
                        }
                        if (probka.atrybuty[i]<min[i])
                        {
                            min[i] = probka.atrybuty[i];
                        }
                    }
                }

                foreach (Probka probka in probki)
                {
                    for (int i = 0; i < max.Length; i++)
                    {
                        probka.atrybuty[i] = (probka.atrybuty[i] - min[i]) / (max[i] - min[i]);
                    }
                }


            }
        }

        public class Probka
        {
            public List<double> atrybuty = new List<double>();
            public string klasa;
            public int liczbaAtr;

            public Probka(List<double> atr, string kl, int n)
            {
                atrybuty = atr;
                klasa = kl;
                liczbaAtr = n;
            }

            public string getText()
            {
                string result = "";
                //result = atrybuty.Count.ToString();
                foreach (var atr in atrybuty)
                {
                    result = String.Concat(result, atr.ToString("0.00", CultureInfo.InvariantCulture), "\t");
                }

                result = String.Concat(result, "klasa=", klasa);
                return result;
            }

            private double manhattan(Probka compare)
            {
                double result = 0;
                for (int i = 0; i < liczbaAtr; i++)
                {
                    result += Math.Abs(atrybuty[i] - compare.atrybuty[i]);
                }

                return result;
            }

            private double euklides(Probka compare)
            {
                double result = 0;
                for (int i = 0; i < liczbaAtr; i++)
                {
                    result += Math.Pow((atrybuty[i] - compare.atrybuty[i]),2);
                }

                result = Math.Sqrt(result);
                return result;
            }

            private double czebyszew(Probka compare)
            {
                double result = 0;
                for (int i = 0; i < liczbaAtr; i++)
                {
                    double temp = Math.Abs(atrybuty[i] - compare.atrybuty[i]);
                    if (temp > result)
                    {
                        result = temp;
                    }
                }

                return result;
            }

            private double minkowski(Probka compare, double m)
            {
                double result = 0;
                for (int i = 0; i < liczbaAtr; i++)
                {
                    result += Math.Pow(Math.Abs(atrybuty[i] - compare.atrybuty[i]), m);
                }

                result = Math.Pow(result, (1 / m));
                return result;
            }

            private double logarytm(Probka compare)
            {
                double result = 0;
                for (int i = 0; i < liczbaAtr; i++)
                {
                    result += Math.Abs(Math.Log10(atrybuty[i]) - Math.Log10(compare.atrybuty[i]));
                }

                return result;
            }

            public string knn(List<Probka> probki, int n = 5, string metryka = "euklides", double m = 1)
            {
                string result = "";
                switch (metryka)
                {
                    case "manhattan":
                        probki.Sort(delegate (Probka x, Probka y) {
                            return x.manhattan(this).CompareTo(y.manhattan(this));
                        });
                        break;
                    case "euklides":
                        probki.Sort(delegate (Probka x, Probka y) {
                            return x.euklides(this).CompareTo(y.euklides(this));
                        });
                        break;
                    case "czebyszew":
                        probki.Sort(delegate (Probka x, Probka y) {
                            return x.czebyszew(this).CompareTo(y.czebyszew(this));
                        });
                        break;
                    case "minkowski":
                        probki.Sort(delegate (Probka x, Probka y) {
                            return x.minkowski(this,m).CompareTo(y.minkowski(this,m));
                        });
                        break;
                    case "logarytm":
                        probki.Sort(delegate (Probka x, Probka y) {
                            return x.logarytm(this).CompareTo(y.logarytm(this));
                        });
                        break;
                    default:
                        return "bledna nazwa metryki";
                }
                
                var top = new Dictionary<string, int>();
                for (int i = 0; i < n; i++)
                {
                    string temp = probki[i].klasa;
                    if (top.ContainsKey(temp))
                    {
                        top[probki[i].klasa] += 1;
                    }
                    else
                    {
                        top.Add(temp, 1);
                    }
                }
                int max = 0;
                foreach (var elem in top)
                {
                    for (int i = 0; i < top.Count; i++)
                    {
                        if (elem.Value == max)
                        {
                            result = "blad-knn-rowne-odleglosci-do-klas";
                        }
                        else
                        {
                            if (elem.Value > max)
                            {
                                result = elem.Key;
                                max = elem.Value;
                            }
                        }
                    }
                }

                return result;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();



            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".txt";
            dlg.Filter = "TXT Files (*.txt)|*.txt";


            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();


            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;
                FileText.Text = filename;
                var sr = new StreamReader(dlg.FileName);
                String text = sr.ReadToEnd();
                String[] sep = {"\n"};
                int maxCount = 9999;
                String[] wiersze = text.Split(sep, maxCount, StringSplitOptions.RemoveEmptyEntries);
                String[] sep2 = {" ", "\t"};
                foreach (String wiersz in wiersze)
                {
                    String[] atrybuty = wiersz.Split(sep2, maxCount, StringSplitOptions.RemoveEmptyEntries);
                    List<double> atrybutynum = new List<double>();
                    for (int i = 0; i < atrybuty.Length - 1; i++)
                    {
                        atrybutynum.Add(Convert.ToDouble(atrybuty[i], CultureInfo.InvariantCulture));
                    }

                    string klasa = atrybuty[atrybuty.Length - 1];
                    int n = atrybutynum.Count;
                    Probka probka = new Probka(atrybutynum, klasa, n);
                    Dane.probki.Add(probka);
                }

                TestText.Text = "";
                foreach (Probka probka in Dane.probki)
                {
                    TestText.Text = String.Concat(TestText.Text, probka.getText());
                }

            }

            for (int i = 0; i < Dane.probki[0].liczbaAtr; i++)
            {
                TextBox txtb = new TextBox();
                string name = String.Concat("atr",(i+1).ToString());
                txtb.Name = name;
                txtb.Height = 20;
                txtb.Width = 50;
                txtb.Text = txtb.Name;
                atrybutytxt.Children.Add(txtb);
            }
            
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Normal_Click(object sender, RoutedEventArgs e)
        {
            Dane.normalizuj();
            TestText.Text = "";
            foreach (Probka probka in Dane.probki)
            {
                TestText.Text = String.Concat(TestText.Text, probka.getText(), "\n");
            }
        }

        private void Porownaj_Click(object sender, RoutedEventArgs e)
        {
            string wyniki = "ilosc poprawnych ";
            int n;
            double m;
            try
            {
                m = Convert.ToDouble(LiczbaM.Text, CultureInfo.InvariantCulture);
            }
            catch (Exception exception)
            {
                m = 1;
            }

            try
            {
                n = Convert.ToInt32(LiczbaM.Text, CultureInfo.InvariantCulture);
            }
            catch (Exception exception)
            {
                n = 5;
            }

            wyniki = String.Concat(wyniki, " dla n=", n.ToString(), ", m=", m.ToString(), ", liczba elementow=", Dane.probki.Count.ToString(), ":\n");


            string[] metryki = new string[] {"manhattan", "euklides", "czebyszew", "minkowski", "logarytm"};
            foreach (var metryka in metryki)
            {
                int poprawne = 0;
                foreach (var probka in Dane.probki)
                {
                    if (probka.klasa == probka.knn(Dane.probki, n, metryka, m))
                    {
                        poprawne++;
                    }
                }
                wyniki = String.Concat(wyniki, metryka, " = ", poprawne.ToString(), "\n");
            }

            Porownanie.Text = wyniki;
        }

        private void LiczbaM_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void test_Click(object sender, RoutedEventArgs e)
        {
            int n = Dane.probki[0].liczbaAtr;
            int i = 0;
            List<double> atrybuty = new List<double>();
            foreach(var elem in atrybutytxt.Children)
            {
                if (elem.GetType() == typeof(TextBox))
                {
                    TextBox txt = (TextBox) elem;
                    double atr;
                    String name = String.Concat("atr", (i + 1).ToString());
                    try
                    {
                        atr = Convert.ToDouble(txt.Text, CultureInfo.InvariantCulture);
                    }
                    catch (Exception exception)
                    {
                        test_obiekt.Text = "";
                        test_obiekt.Text = "zle atrybuty";
                        return;
                    }

                    atrybuty.Add(Convert.ToDouble(txt.Text, CultureInfo.InvariantCulture));
                }

                i++;
            }
            Probka testProbka = new Probka(atrybuty, "brak", n);
            Dane.test = testProbka;
            test_obiekt.Text = "";
            test_obiekt.Text = testProbka.getText();
        }

        private void manhattanTest_Click(object sender, RoutedEventArgs e)
        {
            if (Dane.test == null)
            {
                return;
            }
            double m;
            int n;
            try
            {
                m = Convert.ToDouble(LiczbaM.Text, CultureInfo.InvariantCulture);
            }
            catch (Exception exception)
            {
                m = 1;
            }

            try
            {
                n = Convert.ToInt32(LiczbaM.Text, CultureInfo.InvariantCulture);
            }
            catch (Exception exception)
            {
                n = 5;
            }

            Dane.test.klasa = Dane.test.knn(Dane.probki, n, "manhattan", m);
            test_obiekt.Text = "";
            test_obiekt.Text = Dane.test.getText();
        }

        private void euklidesTest_Click(object sender, RoutedEventArgs e)
        {
            if (Dane.test == null)
            {
                return;
            }
            double m;
            int n;
            try
            {
                m = Convert.ToDouble(LiczbaM.Text, CultureInfo.InvariantCulture);
            }
            catch (Exception exception)
            {
                m = 1;
            }

            try
            {
                n = Convert.ToInt32(LiczbaM.Text, CultureInfo.InvariantCulture);
            }
            catch (Exception exception)
            {
                n = 5;
            }

            Dane.test.klasa = Dane.test.knn(Dane.probki, n, "euklides", m);
            test_obiekt.Text = "";
            test_obiekt.Text = Dane.test.getText();
        }

        private void czebyszewTest_Click(object sender, RoutedEventArgs e)
        {
            if (Dane.test == null)
            {
                return;
            }
            double m;
            int n;
            try
            {
                m = Convert.ToDouble(LiczbaM.Text, CultureInfo.InvariantCulture);
            }
            catch (Exception exception)
            {
                m = 1;
            }

            try
            {
                n = Convert.ToInt32(LiczbaM.Text, CultureInfo.InvariantCulture);
            }
            catch (Exception exception)
            {
                n = 5;
            }

            Dane.test.klasa = Dane.test.knn(Dane.probki, n, "czebyszew", m);
            test_obiekt.Text = "";
            test_obiekt.Text = Dane.test.getText();
        }

        private void minkowskiTest_Click(object sender, RoutedEventArgs e)
        {
            if (Dane.test == null)
            {
                return;
            }
            double m;
            int n;
            try
            {
                m = Convert.ToDouble(LiczbaM.Text, CultureInfo.InvariantCulture);
            }
            catch (Exception exception)
            {
                m = 1;
            }

            try
            {
                n = Convert.ToInt32(LiczbaM.Text, CultureInfo.InvariantCulture);
            }
            catch (Exception exception)
            {
                n = 5;
            }

            Dane.test.klasa = Dane.test.knn(Dane.probki, n, "minkowski", m);
            test_obiekt.Text = "";
            test_obiekt.Text = Dane.test.getText();
        }

        private void logarytmTest_Click(object sender, RoutedEventArgs e)
        {
            if (Dane.test == null)
            {
                return;
            }
            double m;
            int n;
            try
            {
                m = Convert.ToDouble(LiczbaM.Text, CultureInfo.InvariantCulture);
            }
            catch (Exception exception)
            {
                m = 1;
            }

            try
            {
                n = Convert.ToInt32(LiczbaM.Text, CultureInfo.InvariantCulture);
            }
            catch (Exception exception)
            {
                n = 5;
            }

            Dane.test.klasa = Dane.test.knn(Dane.probki, n, "logarytm", m);
            test_obiekt.Text = "";
            test_obiekt.Text = Dane.test.getText();
        }

        private void dodaj_Click(object sender, RoutedEventArgs e)
        {
            if (Dane.test != null)
            {
                Dane.probki.Add(Dane.test);
                Dane.test = null;
            }
            TestText.Text = "";
            foreach (Probka probka in Dane.probki)
            {
                TestText.Text = String.Concat(TestText.Text, probka.getText());
            }
        }
    }
}
