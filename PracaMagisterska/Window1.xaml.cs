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

namespace PracaMagisterska
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    /// 
    public partial class Window1 : Window
    {
        Point startPoint;
        string dragPanel;
        bool drag = false;

        public Window1()
        {
            InitializeComponent();
            
        }

        private void panel_DragOver(object sender, DragEventArgs e)
        {
            Console.WriteLine("In panel dragOver");
            if (!drag)
            {
                dragPanel = ((Panel)sender).Name;
                drag = true;
            }
            Console.WriteLine(dragPanel);
            if (e.Data.GetDataPresent("Object"))
            {
                Console.WriteLine("I have object");
                // These Effects values are used in the drag source's
                // GiveFeedback event handler to determine which cursor to display.
                if (e.KeyStates == DragDropKeyStates.ControlKey)
                {
                    Console.WriteLine("Copy");
                    e.Effects = DragDropEffects.Copy;
                }
                else
                {
                    Console.WriteLine("Move");
                    e.Effects = DragDropEffects.Move;
                }
            }
        }

        private void panelCanvas_Drop(object sender, DragEventArgs e)
        {
            // If an element in the panel has already handled the drop,
            // the panel should not also handle it.
            if (e.Handled == false)
            {
                Console.WriteLine("Upisciem cos");
                Panel _panel = (Panel)sender;
                Console.WriteLine(sender.ToString());
                UIElement _element = (UIElement)e.Data.GetData("Object");
                //Viewbox viewbox = new Viewbox();
                //viewbox.Stretch = Stretch.Uniform;
                //viewbox.Width = 170;
                //viewbox.Height = 30;
                //viewbox.Child = _element;
                _panel.Children.Remove(_element);
                _panel.Children.Add(_element);

                //if (_panel != null && _element != null)
                //{
                //    // Get the panel that the element currently belongs to,
                //    // then remove it from that panel and add it the Children of
                //    // the panel that its been dropped on.
                //    Panel _parent = (Panel)VisualTreeHelper.GetParent(_element);

                //    if (_parent != null)
                //    {
                //        Console.WriteLine("Parent not null");
                //        if (e.KeyStates == DragDropKeyStates.ControlKey &&
                //            e.AllowedEffects.HasFlag(DragDropEffects.Copy))
                //        {
                //            Neuron1 _neuron = new Neuron1();
                //            _panel.Children.Add(_neuron);
                //            // set the value to return to the DoDragDrop call
                //            e.Effects = DragDropEffects.Copy;
                //        }
                //        else if (e.AllowedEffects.HasFlag(DragDropEffects.Move))
                //        {
                //            _parent.Children.Remove(_element);
                //            _panel.Children.Add(_element);
                //            // set the value to return to the DoDragDrop call
                //            e.Effects = DragDropEffects.Move;
                //        }
                //    }
                
            }
        }

        private void panel_Drop(object sender, DragEventArgs e)
        {
            // If an element in the panel has already handled the drop,
            // the panel should not also handle it.
            
            if (e.Handled == false)
            {
                Console.WriteLine("Upisciem cos");
                Panel _panel = (Panel)sender;
                Console.WriteLine(sender.ToString());
                UIElement _element = (UIElement)e.Data.GetData("Object");
                string model = (string)e.Data.GetData("Model");
                Console.WriteLine(model);
                if (_panel != null && _element != null)
                {
                    // Get the panel that the element currently belongs to,
                    // then remove it from that panel and add it the Children of
                    // the panel that its been dropped on.

                    if (this.dragPanel == "dropCanvas")
                    {
                        _panel.Children.Remove(_element);
                        _panel.Children.Add(_element);
                    }
                    else
                    {
                        if (model == "Model0")
                        {
                            Neuron0 neuron = new Neuron0();
                            _panel.Children.Add(neuron);
                        }
                        else if (model == "Model1")
                        {
                            Neuron1 neuron = new Neuron1();
                            _panel.Children.Add(neuron);
                        }
                        else
                        {
                            Neuron2 neuron = new Neuron2();
                            _panel.Children.Add(neuron);
                        }
                        
                    }
                    drag = false;
                        //Panel _parent = (Panel)VisualTreeHelper.GetParent(_element);

                        //if (_parent != null)
                        //{
                        //    if (_parent == _panel)
                        //    {
                        //        _parent.Children.Remove(_element);
                        //        _panel.Children.Add(_element);
                        //        // set the value to return to the DoDragDrop call
                        //        e.Effects = DragDropEffects.Move;
                        //    }
                        //    else
                        //    {
                        //        if (model == "Model0")
                        //        {
                        //            Neuron0 neuron = new Neuron0();
                        //            _panel.Children.Add(neuron);
                        //        }
                        //        else if (model == "Model1")
                        //        {
                        //            Neuron1 neuron = new Neuron1();
                        //            _panel.Children.Add(neuron);
                        //        }
                        //        else
                        //        {
                        //            Neuron2 neuron = new Neuron2();
                        //            _panel.Children.Add(neuron);
                        //        }
                        //    }
                        //}
                    }
                //Neuron1 neuron = new Neuron1();

                //_panel.Children.Add(viewbox);
            }
        }

                //private void DropCanvas_DragEnter(object sender, DragEventArgs e)
                //{
                //    if (!e.Data.GetDataPresent("Model") ||
                //        sender == e.Source)
                //    {
                //        e.Effects = DragDropEffects.None;
                //        Console.WriteLine("Here2");
                //    }
                //    else
                //    {
                //        Console.WriteLine("Here");
                //        e.Effects = DragDropEffects.Copy;
                //    }
                //}

                //private void DropCanvas_Drop(object sender, DragEventArgs e)
                //{
                //    if (e.Data.GetDataPresent("Model"))
                //    {
                //        Console.WriteLine("In drop");
                //        Neuron1 neuron1 = e.Data.GetData("Model") as Neuron1;
                //        StackPanel stakView = sender as StackPanel;
                //        stakView.Children.Add(neuron1);
                //    }
                //}

         private void neuron1_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //    //startPoint = e.GetPosition(null);
            //    // Console.WriteLine("Dupka");
        }

        private void neuon1_MouseMove(object sender, MouseEventArgs e)
        {
            //    // Console.WriteLine("Pupka");
            //    //Point mousePos = e.GetPosition(null);
            //    //Vector diff = startPoint - mousePos;
            //    ////Console.WriteLine(diff);

            //    //if (e.LeftButton == MouseButtonState.Pressed)
            //    ////&& Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
            //    ////Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
            //    //{
            //    //    Console.WriteLine(e.GetPosition(null));

            //    //    DataObject data = new DataObject();
            //    //    data.SetData("Model", "Model2");
            //    //    data.SetData("Object", this);

            //    //    // Initialize the drag & drop operation
            //    //    DragDrop.DoDragDrop(this, data, DragDropEffects.Move);
            //    //}
        }
    }
}
