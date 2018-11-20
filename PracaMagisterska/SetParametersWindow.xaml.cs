﻿using System;
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

namespace PracaMagisterska
{
    /// <summary>
    /// Interaction logic for SetParametersWindow.xaml
    /// </summary>
    public partial class SetParametersWindow : Window
    {
        private TextBlock info;
        private TextBox parBox;

        public SetParametersWindow()
        {
            InitializeComponent();

            //string[] parameters = { "Neuron Length", "Denrite Diameter", "Axon Diameter", "Flow ", "Flow time", "Max speed" };
            //string[] parametersM1BoxNames = {"neuronLenBoxM1", "denDiamBoxM1", "axonDiamBox1", "flowBoxM1", "timeBoxM1", "maxSpeedM1" };
            //for (int i = 0; i < parametersM1BoxNames.Length; i++)
            //{
            //    info = new TextBlock() { Height=16};
            //}

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
                Console.WriteLine(filename);

                XmlWriter xmlWriter = XmlWriter.Create(filename);
                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("Conf");
                xmlWriter.WriteStartElement("Model2");

                foreach (object obj in Model2_params.Children)
                {
                    if (obj.GetType().Name == "ComboBox")
                    {
                        ComboBox combobox = (ComboBox)obj;
                        xmlWriter.WriteStartElement(combobox.Name);
                        xmlWriter.WriteString(combobox.SelectedItem.ToString().Split(':')[1]);
                        //xmlWriter.WriteString(combobox.Items[combobox.SelectedIndex].ToString().Split(':')[1]);
                        xmlWriter.WriteEndElement();
                    }
                    else
                    {
                        TextBox textbox = (TextBox)obj;
                        xmlWriter.WriteStartElement(textbox.Name);
                        xmlWriter.WriteString(textbox.Text);
                        xmlWriter.WriteEndElement();

                    }
                }

                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("Model3");

                foreach (object obj in Model2_params.Children)
                {
                    if (obj.GetType().Name == "ComboBox")
                    {
                        ComboBox combobox = (ComboBox)obj;
                        xmlWriter.WriteStartElement(combobox.Name);
                        xmlWriter.WriteString(combobox.SelectedItem.ToString().Split(':')[1]);
                        //xmlWriter.WriteString(combobox.Items[combobox.SelectedIndex].ToString().Split(':')[1]);
                        xmlWriter.WriteEndElement();
                    }
                    else if (obj.GetType().Name == "TextBox")
                    {
                        TextBox textbox = (TextBox)obj;
                        xmlWriter.WriteStartElement(textbox.Name);
                        xmlWriter.WriteString(textbox.Text);
                        xmlWriter.WriteEndElement();
                    }
                }

                xmlWriter.WriteEndElement();

                xmlWriter.WriteEndDocument();
                xmlWriter.Close();
                Console.WriteLine("Done");

            }


        }

    }


}