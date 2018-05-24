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
using System.Threading;


namespace DirectoryPositioner {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window {
        public bool EnableMouseRangeCheck {
            get; set;
        }

        public MainWindow() {
            EnableMouseRangeCheck = false;

            this.Topmost = true;
            this.ShowInTaskbar = false;

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
            };

            this.Drop += delegate ( object sender, DragEventArgs e ) {
                if( e.Data.GetDataPresent( DataFormats.FileDrop ) ) {
                    var path = ( (System.Array)e.Data.GetData( DataFormats.FileDrop ) ).GetValue( 0 ).ToString();
                    var window = new Edit( path, this );
                    window.EditCompleted += delegate {
                        InitPage();
                    };
                    window.Owner = this;
                    window.ShowDialog();
                }
            };

            SearchText.Focus();
        }

        private void InitPage() {
            SearchText.Text = "";
            Keyboard.Focus( SearchText );

            DataList.Visibility = Visibility.Collapsed;
            ButtonList.Visibility = Visibility.Collapsed;
            var pageMode = DataSource.GetPageMode();

            this.Width = PointsAndSizes.ListModeWindowSize.Width;
            this.Height = PointsAndSizes.ListModeWindowSize.Width;
            DataList.Visibility = Visibility.Visible;
            InitLists();

            ShowWindowOnLeftBottom();


            this.MouseEnter += delegate {
                EnableMouseRangeCheck = true;
            };

            //如果鼠标移除窗口则隐藏
            ThreadPool.QueueUserWorkItem( delegate {
                var timer = new System.Timers.Timer();
                timer.Enabled = true;
                timer.Interval = 100;
                timer.Start();
                timer.Elapsed += delegate {
                    if( !EnableMouseRangeCheck ) {
                        return;
                    }
                    this.Dispatcher.Invoke( (Action)delegate {
                        var mousePos = System.Windows.Forms.Control.MousePosition;
                        var rect = PointsAndSizes.GetRectByWindow( this );
                        if( !PointsAndSizes.In( rect, mousePos ) ) {
                            this.HiddenWindow();
                            EnableMouseRangeCheck = false;
                        }
                    } );
                };
            } );

            //触发显示窗口操作
            ThreadPool.QueueUserWorkItem( delegate {
                var timer = new System.Timers.Timer();
                timer.Enabled = true;
                timer.Interval = 100;
                timer.Start();
                timer.Elapsed += delegate {
                    var mousePos = System.Windows.Forms.Control.MousePosition;
                    if( mousePos.X <= 1 && mousePos.Y >= SystemParameters.PrimaryScreenHeight - PointsAndSizes.ListModeWindowSize.Height ) {
                        this.Dispatcher.Invoke( (Action)delegate {
                            ShowWindowOnLeftBottom();
                        } );
                    }

                    if( Math.Abs( mousePos.Y - SystemParameters.PrimaryScreenHeight ) <= 1 && mousePos.X <= PointsAndSizes.ListModeWindowSize.Width ) {
                        this.Dispatcher.Invoke( (Action)delegate {
                            ShowWindowOnLeftBottom();
                        } );
                    }
                };

            } );
        }


        private void HiddenWindow() {
            var pos = PointsAndSizes.WindowHidden;
            this.Left = pos.X;
            this.Top = pos.Y;
            this.Hide();
        }

        private void ShowWindowOnLeftBottom() {
            var windowPos = PointsAndSizes.WindowOnLeftBottom;
            this.Left = windowPos.X;
            this.Top = windowPos.Y;

            this.WindowState = WindowState.Normal;
            this.Show();
            this.Activate();
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

        private void OpenPath( string path ) {
            try {
                System.Diagnostics.Process.Start( path );
            } catch( Exception e ) {
                MessageBox.Show( e.Message );
            }
        }

        public void ForceShow() {
            this.Show();
        }

        protected override void OnClosing( System.ComponentModel.CancelEventArgs e ) {
            e.Cancel = true;
            this.HiddenWindow();
        }

        private void WrapPanel_MouseEnter( object sender, MouseEventArgs e ) {
            Close.Visibility = Visibility.Visible;
            Add.Visibility = Visibility.Visible;
            Config.Visibility = Visibility.Visible;
        }

        private void WrapPanel_MouseLeave( object sender, MouseEventArgs e ) {
            Close.Visibility = Visibility.Collapsed;
            Add.Visibility = Visibility.Collapsed;
            Config.Visibility = Visibility.Collapsed;
        }

        private void Close_Click( object sender, RoutedEventArgs e ) {
            Application.Current.Shutdown();
        }

        private void Add_Click( object sender, RoutedEventArgs e ) {
            var ediWid = new Edit(this);
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
            var window = new Edit( selectItem.Name, selectItem.Path, this );
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
            DataList.ItemsSource = dataList;
            SetDataCount( dataList.Count );
        }

        private void SetDataCount( int count ) {
            Count.Text = count + "个对象";
        }

        private void Config_Click( object sender, RoutedEventArgs e ) {
            EnableMouseRangeCheck = false;

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
