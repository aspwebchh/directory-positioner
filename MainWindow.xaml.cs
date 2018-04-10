using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
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
using Pinyin4net;
using Pinyin4net.Format;
using System.Windows.Threading;

namespace DirectoryPositioner {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            this.Topmost = true;
            ResizeMode = ResizeMode.NoResize;
            InitializeComponent();
            InitPage();

            this.Activated += delegate {
                Keyboard.Focus( SearchText );
            };

            var jumpListView = false;

            this.KeyUp += delegate ( object sender, KeyEventArgs e ) {
                var pageMode = DataSource.GetPageMode();
                if( pageMode == PageMode.Btn ) {
                    return;
                }
                if( e.Key == Key.Down ) {
                    if( SearchText.IsFocused ) {
                        DataList.SelectedIndex = 0;
                        DataList.Focus();
                    }
                }
                if( e.Key == Key.Up && DataList.SelectedIndex == 0 ) {
                    if( jumpListView ) {
                        SearchText.Focus();
                        jumpListView = false;
                    }
                    if( !jumpListView ) {
                        jumpListView = true;
                    }
                } else {
                    jumpListView = false;
                }
                if( e.Key == Key.Enter ) {
                    var selectItem = DataList.SelectedItem as ConfigItem;
                    if( selectItem != null ) {
                        OpenPath( selectItem.Path );
                    }
                }
                if( e.Key == Key.Left ) {
                    SearchText.Focus();
                }

                if( e.Key == Key.Escape ) {
                    WindowState = WindowState.Minimized;
                }
            };

            this.Drop += delegate ( object sender, DragEventArgs e ) {
                if( e.Data.GetDataPresent( DataFormats.FileDrop ) ) {
                    var path = ( (System.Array)e.Data.GetData( DataFormats.FileDrop ) ).GetValue( 0 ).ToString();
                    var window = new Edit( path );
                    window.EditCompleted += delegate {
                        InitPage();
                    };
                    window.Owner = this;
                    window.ShowDialog();
                }
            };
        }

        private void InitPage() {
            SearchText.Text = "";
            Keyboard.Focus( SearchText );

            DataList.Visibility = Visibility.Collapsed;
            ButtonList.Visibility = Visibility.Collapsed;
            var pageMode = DataSource.GetPageMode();
            if( pageMode == PageMode.Btn ) {
                this.Width = 525;
                this.Height = 350;
                ButtonList.Visibility = Visibility.Visible;
                InitButtons();
            } else {
                this.Width = 300;
                this.Height = 300;
                DataList.Visibility = Visibility.Visible;
                InitLists();
            }

            var root = AutomationElement.RootElement;
            AutomationElement aelement = AutomationElement.RootElement
                        .FindFirst( TreeScope.Descendants, new PropertyCondition( AutomationElement.ClassNameProperty, "Shell_TrayWnd" ) )
                        .FindFirst( TreeScope.Descendants, new PropertyCondition( AutomationElement.ClassNameProperty, "ReBarWindow32" ) )
                        //.FindFirst( TreeScope.Descendants, new PropertyCondition( AutomationElement.ClassNameProperty, "MSTaskSwWClass" ) )
                        .FindFirst( TreeScope.Descendants, new PropertyCondition( AutomationElement.ClassNameProperty, "MSTaskListWClass" ) )
                        .FindFirst( TreeScope.Descendants, new PropertyCondition( AutomationElement.NameProperty, "DirectoryPositioner" ) );
            ;
            if( aelement != null ) {
                System.Windows.Rect rect = (System.Windows.Rect)aelement.GetCurrentPropertyValue( AutomationElement.BoundingRectangleProperty );
                this.Left = rect.Left;
                this.Top = rect.Top - this.Height;
            }
        }


        private void InitLists() {
            var data = DataSource.GetDataList();
            DataList.ItemsSource = data;
            SetDataCount( data.Count );
        }

        private SolidColorBrush Color2SCB( string color ) {
            System.Drawing.SolidBrush sb = new System.Drawing.SolidBrush( ColorTranslator.FromHtml( color ) );
            SolidColorBrush solidColorBrush = new SolidColorBrush( System.Windows.Media.Color.FromArgb( sb.Color.A, sb.Color.R, sb.Color.G, sb.Color.B ) );
            return solidColorBrush;
        }

        private void InitButtons( List<ConfigItem> dataList ) {
            var btnList = ( ButtonList.Content as WrapPanel );
            btnList.Children.Clear();
            dataList.ForEach( item => {
                var name = item.Name;
                var path = item.Path;
                var bgColor = item.BgColor;
                var txtColor = item.TextColor;

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
                //编辑
                var editMenuItem = new MenuItem();
                editMenuItem.Header = "编辑";
                editMenuItem.Click += delegate ( object sender, RoutedEventArgs e ) {
                    var window = new Edit( name, path );
                    window.EditCompleted += delegate {
                        InitPage();
                    };
                    window.Owner = this;
                    window.ShowDialog();
                };
                contextMenu.Items.Add( editMenuItem );



                //删除
                var menuItem = new MenuItem();
                menuItem.Header = "删除";
                menuItem.Click += delegate ( object sender, RoutedEventArgs e ) {
                    var success = DataSource.Delete( path );
                    if( success ) {
                        InitPage();
                    } else {
                        MessageBox.Show( "删除失败" );
                    }
                };
                contextMenu.Items.Add( menuItem );

                //设置按钮背景颜色
                var bgColorMenuItem = new MenuItem();
                bgColorMenuItem.Header = "背景颜色";
                bgColorMenuItem.Click += delegate ( object sender, RoutedEventArgs e ) {
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
                    OpenPath( path );
                };
                btnList.Children.Add( button );
            } );
        }

        private void InitButtons() {
            var dataList = DataSource.GetDataList();
            InitButtons( dataList );
            SetDataCount( dataList.Count );
        }

        private void OpenPath( string path ) {
            try {
                System.Diagnostics.Process.Start( path );
                WindowState = WindowState.Minimized;
            } catch( Exception e ) {
                MessageBox.Show( e.Message );
            }
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
            List.Visibility = Visibility.Visible;
            Btn.Visibility = Visibility.Visible;
            Config.Visibility = Visibility.Visible;
        }

        private void WrapPanel_MouseLeave( object sender, MouseEventArgs e ) {
            Close.Visibility = Visibility.Collapsed;
            Add.Visibility = Visibility.Collapsed;
            List.Visibility = Visibility.Collapsed;
            Btn.Visibility = Visibility.Collapsed;
            Config.Visibility = Visibility.Collapsed;
        }

        private void Close_Click( object sender, RoutedEventArgs e ) {
            Application.Current.Shutdown();
        }

        private void Add_Click( object sender, RoutedEventArgs e ) {
            var ediWid = new Edit();
            ediWid.Owner = this;
            ediWid.EditCompleted += delegate {
                InitPage();
            };
            ediWid.ShowDialog();
        }

        #region   
        //列表模式
        private void MenuItem_Click_Edit( object sender, RoutedEventArgs e ) {
            var selectItem = DataList.SelectedItem as ConfigItem;
            var window = new Edit( selectItem.Name, selectItem.Path );
            window.EditCompleted += delegate {
                InitPage();
            };
            window.Owner = this;
            window.ShowDialog();
        }

        private void MenuItem_Click_Del( object sender, RoutedEventArgs e ) {
            var selectItem = DataList.SelectedItem as ConfigItem;
            var success = DataSource.Delete( selectItem.Path );
            if( success ) {
                InitPage();
            } else {
                MessageBox.Show( "删除失败" );
            }
        }
        #endregion



        private void Btn_Click( object sender, RoutedEventArgs e ) {
            DataSource.SetPageMode( PageMode.Btn );
            InitPage();
        }

        private void List_Click( object sender, RoutedEventArgs e ) {
            DataSource.SetPageMode( PageMode.List );
            InitPage();
        }

        private void DataList_MouseDoubleClick( object sender, MouseButtonEventArgs e ) {
            var selectItem = DataList.SelectedItem as ConfigItem;
            OpenPath( selectItem.Path );
        }

        private void SearchText_KeyUp( object sender, KeyEventArgs e ) {
            var text = SearchText.Text.Trim();
            var dataList = DataSource.GetDataList( text );
            var pageMode = DataSource.GetPageMode();
            if( pageMode == PageMode.List ) {
                DataList.ItemsSource = dataList;
            } else if( pageMode == PageMode.Btn ) {
                InitButtons( dataList );
            }
            SetDataCount( dataList.Count );
        }

        private void SetDataCount( int count ) {
            Count.Text = count + "个对象";
        }

        private void Config_Click( object sender, RoutedEventArgs e ) {
            var apps = new List<string> { "notepad++", "notepad" };

            foreach( var app in apps ) {
                try {
                    System.Diagnostics.Process.Start( app, DataSource.SRC_FILE_NAME );
                    return;
                } catch( Exception ) { }
            }

            try {
                System.Diagnostics.Process.Start( DataSource.SRC_FILE_NAME );
            } catch( Exception ex ) {
                MessageBox.Show( ex.Message );
            }
        }
    }
}
