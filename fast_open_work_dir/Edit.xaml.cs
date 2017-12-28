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
using System.Windows.Shapes;

namespace fast_open_work_dir {
    /// <summary>
    /// Edit.xaml 的交互逻辑
    /// </summary>
    public partial class Edit : Window {
        public Edit() {
            InitializeComponent();
        }

        private void Button_Click( object sender, RoutedEventArgs e ) {
            var name = Name.Text.Trim();
            var path = Path.Text.Trim();
            if( name == "" ) {
                MessageBox.Show("请输入目录名称");
                return;
            }
            if( path == "" ) {
                MessageBox.Show("请输入目录路径" );
                return;
            }
            var success = DataSource.AddPath( path, name );
            if( success ) {
                var owner = this.Owner as MainWindow;
                owner.InitButtons();
                this.Close();
            } else {
                MessageBox.Show("添加失败，请重试" );
            }
        }
    }
}
