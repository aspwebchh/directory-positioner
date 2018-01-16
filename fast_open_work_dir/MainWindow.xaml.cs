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
using System.IO;
using System.Drawing;

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

        private SolidColorBrush Color2SCB( string color ) {
            System.Drawing.SolidBrush sb = new System.Drawing.SolidBrush( ColorTranslator.FromHtml( color ) );
            SolidColorBrush solidColorBrush = new SolidColorBrush( System.Windows.Media.Color.FromArgb( sb.Color.A, sb.Color.R, sb.Color.G, sb.Color.B ) );
            return solidColorBrush;
        }

        public void InitButtons() {
            var dataTable = DataSource.GetPathList();
            ButtonList.Children.Clear();
            dataTable.Rows.Cast<DataRow>().ToList().ForEach( item => {
                var name = item[ "Name" ].ToString();
                var path = item[ "Path_Text" ].ToString();
                var bgColor = "";
                var txtColor = "";
                if( dataTable.Columns.Contains( "BgColor" ) ) {
                    bgColor = item[ "BgColor" ].ToString();
                }
                if( dataTable.Columns.Contains( "TextColor" ) ) {
                    txtColor = item[ "TextColor" ].ToString();
                }

                var button = new Button();
               // button.Background = Color2SCB( "#FFDDDDDD" );

                button.MouseEnter += delegate ( object sender, MouseEventArgs e ) {
                    if( string.IsNullOrEmpty( bgColor ) ) {
                        button.Background = Color2SCB( "#FFBEE6FD" );
                    }
                };
                button.MouseLeave += delegate ( object sender, MouseEventArgs e ) {
                    if( string.IsNullOrEmpty( bgColor ) ) {
                        button.Background = Color2SCB( "#FFDDDDDD" );
                    }
                };

                if( !string.IsNullOrEmpty( bgColor ) ) {
                    button.Background = Color2SCB( bgColor );
                    button.BorderThickness = new Thickness( 0 );
                }

                if( !string.IsNullOrEmpty( txtColor ) ) {
                    button.Foreground = Color2SCB( txtColor );
                }

                var contextMenu = new ContextMenu();
                //删除
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

                //设置按钮背景颜色
                var bgColorMenuItem = new MenuItem();
                bgColorMenuItem.Header = "背景颜色";
                bgColorMenuItem.Click += delegate ( object sender, RoutedEventArgs e ){
                    System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();
                    if( colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK ) {
                        var success = DataSource.SetBgColor( path, ColorTranslator.ToHtml( colorDialog.Color ) );
                        if( !success ) {
                            MessageBox.Show( "设置背景颜色失败" );
                        } else {
                            InitButtons();
                        }
                    }
                };
                contextMenu.Items.Add( bgColorMenuItem );

                //设置按钮文本颜色
                var txtColorMenuItem = new MenuItem();
                txtColorMenuItem.Header = "文本颜色";
                txtColorMenuItem.Click += delegate ( object sender, RoutedEventArgs e ) {
                    System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();
                    if( colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK ) {
                        var success = DataSource.SetTextColor( path, ColorTranslator.ToHtml( colorDialog.Color ) );
                        if( !success ) {
                            MessageBox.Show( "设置文本颜色失败" );
                        } else {
                            InitButtons();
                        }
                    }
                };
                contextMenu.Items.Add( txtColorMenuItem );

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
