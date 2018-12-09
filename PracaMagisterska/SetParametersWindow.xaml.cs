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
using System.Xml;
using System.Xml.Linq;
using System.IO;

namespace PracaMagisterska
{
    /// <summary>
    /// Interaction logic for SetParametersWindow.xaml
    /// </summary>
    public partial class SetParametersWindow : Window
    {
        private string projectPath;
        private Action<string, double, double, double> callback;
        private object parentWindow; 

        public SetParametersWindow(Action<string, double, double, double> action, object window, string conf = "")
        {
            InitializeComponent();
            if (conf != "")
            {
                this.load(conf);

            }
            this.projectPath = string.Join("\\", Directory.GetCurrentDirectory().Split('\\').Take(4).ToArray());
            this.callback = action;
            this.parentWindow = window;

            //string[] parameters = { "Neuron Length", "Denrite Diameter", "Axon Diameter", "Flow ", "Flow time", "Max speed" };
            //string[] parametersM1BoxNames = {"neuronLenBoxM1", "denDiamBoxM1", "axonDiamBox1", "flowBoxM1", "timeBoxM1", "maxSpeedM1" };
            //for (int i = 0; i < parametersM1BoxNames.Length; i++)
            //{
            //    info = new TextBlock() { Height=16};
            //}

        }


        private void changeCurrentConfInParentWindow(string path)
        {
            this.callback(path, double.Parse(timeBox.Text), double.Parse(flowBox.Text), double.Parse(drainingBox.Text));
            //((MainWindow)Application.Current.MainWindow).currentConf = path;
        }

        private void writeParametersToXML(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "paramsConf"; // Default file name
            dlg.DefaultExt = ".xml"; // Default file extension
            dlg.Filter = "XML documents (.xml)|*.xml"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                string filename = dlg.FileName;
                this.saveXML(filename);

            }
        }

        private void saveXML(string filename)
        {
            XElement xmlTree = new XElement("Configuration");
            XElement xmlGeneral = new XElement("General");
            XElement xmlModel1 = new XElement("Model1");
            XElement xmlModel2 = new XElement("Model2");
            XElement xmlModel3 = new XElement("Model3");

            xmlGeneral = addElementsToXElement(xmlGeneral, generalParams);
            xmlModel1 = addElementsToXElement(xmlModel1, Model1_params);
            xmlModel2 = addElementsToXElement(xmlModel2, Model2_params);
            xmlModel3 = addElementsToXElement(xmlModel3, Model3_params);

            xmlTree.Add(xmlGeneral, xmlModel1, xmlModel2, xmlModel3);
            xmlTree.Save(filename);
        }

        private XElement addElementsToXElement(XElement xmlElement, StackPanel panel)
        {
            foreach (object obj in panel.Children)
            {
                if (obj.GetType().Name == "ComboBox")
                {
                    ComboBox combobox = (ComboBox)obj;
                    xmlElement.Add(new XElement(combobox.Name, combobox.Text));
                }
                else if (obj.GetType().Name == "TextBox")
                {
                    TextBox textbox = (TextBox)obj;
                    xmlElement.Add(new XElement(textbox.Name, textbox.Text));
                }
            }
            return xmlElement;

        }

        private void loadButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".xml";
            dlg.Filter = "XML documents (.xml)|*.xml"; // Filter files by extension
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string filename = dlg.FileName;
                this.load(filename);
                changeCurrentConfInParentWindow(filename);
            }
        }

        private void load(string filename)
        {
            XElement xmlTree = XElement.Load(filename, LoadOptions.None);
            //Console.WriteLine(xmlTree);
            foreach (XElement element in xmlTree.Elements())
            {
                foreach (XElement childElement in element.Elements())
                {
                    string name = childElement.Name.ToString();
                    object window_element = this.FindName(name);
                    if (window_element.GetType() == typeof(ComboBox))
                    {
                        ComboBox comboBox = (ComboBox)window_element;
                        Console.WriteLine("Selected value combo box! " + childElement.Value);
                        comboBox.SelectedItem = childElement.Value == "True" ? trueItem : falseItem;
                    }
                    else if (window_element.GetType() == typeof(TextBox))
                    {
                        TextBox textbox = (TextBox)window_element;
                        textbox.Text = childElement.Value;
                    }
                }
            }
        }

        private void setParamsValueToNeurons()
        {
            if (this.parentWindow.GetType() == typeof(MainWindow))
            {
                Tuple< double, double> denTuple;
                List<Tuple<double, double>> denList;
                bool blocked = false;
                string selectedItem = blockFlow.Text;
                Console.WriteLine("Block flow set params"  + blockFlow.Text);
                if (selectedItem == "True")
                {
                    Console.WriteLine("Block");
                    blocked = true;
                }
                MainWindow mainWindow = (MainWindow)this.parentWindow;
                if (validateField(neuDiamBoxM1) && validateField(neuLenBoxM1))
                    mainWindow.neuron0.SetParameters(new List<Tuple<double, double>>(), 0, double.Parse(neuDiamBoxM1.Text), double.Parse(neuLenBoxM1.Text), blocked);

                if (validateField(denDiamBoxM2) && validateField(denLenBoxM2) && validateField(somaDiamBoxM2) && validateField(axonDiamM2) && validateField(axonLenM2))
                {
                    denTuple = new Tuple<double, double>(double.Parse(denDiamBoxM2.Text), double.Parse(denLenBoxM2.Text));
                    denList = new List<Tuple<double, double>> { denTuple };
                    mainWindow.neuron1.SetParameters(denList, double.Parse(somaDiamBoxM2.Text), double.Parse(axonDiamM2.Text), double.Parse(axonLenM2.Text), blocked);
                }

                if (validateField(den1DiamBoxM3) && validateField(den1LenBoxM3) && validateField(den2DiamBoxM3) && validateField(den2LenBoxM3) && validateField(somaDiamBoxM3)
                    && validateField(axonDiamM3) && validateField(axonLenM3))
                {
                    denList = new List<Tuple<double, double>> { new Tuple<double, double>(double.Parse(den1DiamBoxM3.Text), double.Parse(den1LenBoxM3.Text)),
                        new Tuple<double, double>(double.Parse(den2DiamBoxM3.Text), double.Parse(den2LenBoxM3.Text)) };
                    mainWindow.neuron2.SetParameters(denList, double.Parse(somaDiamBoxM3.Text), double.Parse(axonDiamM3.Text), double.Parse(axonLenM3.Text), blocked);
                }
            }
        }

        private bool validateField(TextBox textbox)
        {
            double i = 0;
            if (String.IsNullOrEmpty(textbox.Text) || textbox.Text.Contains('.') || !double.TryParse(textbox.Text, out i))
                return false;
            return true;
        }

        private void updateNeuronValues(Neuron neuron)
        {

        }

        private bool validateXmlFields(string file)
        {
            XElement xmlTree = XElement.Load(this.projectPath + file, LoadOptions.None);
            bool allFieldsFull = true;
            foreach (XElement element in xmlTree.Elements())
            {
                foreach (XElement childElement in element.Elements())
                {
                    string name = childElement.Name.ToString();
                    object window_element = this.FindName(name);
                    if (window_element.GetType() == typeof(TextBox))
                    {
                        TextBox textbox = (TextBox)window_element;
                        if (String.IsNullOrEmpty(textbox.Text) || textbox.Text.Contains('.'))
                        {
                            Console.WriteLine("Checking");
                            textbox.BorderBrush = System.Windows.Media.Brushes.Red;
                            textbox.Foreground = Brushes.Red;
                            allFieldsFull = false;
                        }
                        else
                        {
                            textbox.BorderBrush = System.Windows.Media.Brushes.Gray;
                            textbox.Foreground = Brushes.Black;
                        }
                    }
                }
            }
            return allFieldsFull;
        }

        private void defaultConf_Click(object sender, RoutedEventArgs e)
        {
            string path = this.projectPath + "\\defaultConf.xml";
            load(path);
            changeCurrentConfInParentWindow(path);
        }

        private void updateDefault_Click(object sender, RoutedEventArgs e)
        {
            if (validateXmlFields("\\defaultConf.xml"))
            {
                saveXML(this.projectPath + "\\defaultConf.xml");
                Console.WriteLine("DefaultConf updated");
            }
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string path = this.projectPath + "\\currentConf.xml";
            saveXML(path);
            setParamsValueToNeurons();
            if (validateField(timeBox) && validateField(flowBox))
                changeCurrentConfInParentWindow(path);
            Console.WriteLine("Current path: " + path);
        }
    }


}
