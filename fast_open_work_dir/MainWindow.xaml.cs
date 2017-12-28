using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Data;
using System.Windows.Automation;

namespace fast_open_work_dir {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {

            ResizeMode = System.Windows.ResizeMode.NoResize;

            InitializeComponent();
            InitButtons();


                var root = AutomationElement.RootElement;
                AutomationElement aelement = AutomationElement.RootElement
                          .FindFirst( TreeScope.Descendants, new PropertyCondition( AutomationElement.ClassNameProperty, "Shell_TrayWnd" ) )
                          .FindFirst( TreeScope.Descendants, new PropertyCondition( AutomationElement.ClassNameProperty, "ReBarWindow32" ) )
                          //.FindFirst( TreeScope.Descendants, new PropertyCondition( AutomationElement.ClassNameProperty, "MSTaskSwWClass" ) )
                          .FindFirst( TreeScope.Descendants, new PropertyCondition( AutomationElement.ClassNameProperty, "MSTaskListWClass" ) )
                          .FindFirst( TreeScope.Descendants, new PropertyCondition( AutomationElement.NameProperty, "目录打开快捷工具" ) );
                if( aelement != null ) {
                    System.Windows.Rect rect = (System.Windows.Rect)aelement.GetCurrentPropertyValue( AutomationElement.BoundingRectangleProperty );
                    this.Left = rect.Left;
                    this.Top = rect.Top - this.Height;
                }


        }

        public void InitButtons() {
            var dataTable = DataSource.GetPathList();
            ButtonList.Children.Clear();
            dataTable.Rows.Cast<DataRow>().ToList().ForEach( item => {
                var name = item[ "Name" ].ToString();
                var path = item[ "Path_Text" ].ToString();
                var button = new Button();

                var contextMenu = new ContextMenu();
                var menuItem = new MenuItem();
                menuItem.Header = "删除";
                menuItem.Click += delegate ( object sender, RoutedEventArgs e ) {
                    var success = DataSource.Delete( path );
                    if( success ) {
                        InitButtons();
                    } else {
                        MessageBox.Show( "删除失败" );
                    }
                };
                contextMenu.Items.Add( menuItem );
                button.ContextMenu = contextMenu;

                var text = new TextBlock();
                text.Text = name;
                button.Content = text;
                button.Style = Resources[ "ButtonNormal" ] as Style;
                button.Click += ( s, e ) => {
                    try {
                        System.Diagnostics.Process.Start( path );
                    } catch( Exception ex ) {
                        MessageBox.Show( ex.Message );
                    }
                };
                ButtonList.Children.Add( button );
            } );
        }



        public void ForceShow() {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Visibility = System.Windows.Visibility.Visible;
            this.Activate();
        }

        protected override void OnClosing( System.ComponentModel.CancelEventArgs e ) {
            e.Cancel = true;
            this.WindowState = WindowState.Minimized;
        }

        private void WrapPanel_MouseEnter( object sender, MouseEventArgs e ) {
            Close.Visibility = Visibility.Visible;
            Add.Visibility = Visibility.Visible;
        }

        private void WrapPanel_MouseLeave( object sender, MouseEventArgs e ) {
            Close.Visibility = Visibility.Collapsed;
            Add.Visibility = Visibility.Collapsed;
        }

        private void Close_Click( object sender, RoutedEventArgs e ) {
            Application.Current.Shutdown();
        }

        private void Add_Click( object sender, RoutedEventArgs e ) {
            var ediWid = new Edit();
            ediWid.Owner = this;
            ediWid.ShowDialog();
        }
    }
}
