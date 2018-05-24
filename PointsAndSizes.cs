using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace DirectoryPositioner {
    static class PointsAndSizes {
        public static Size BtnModeWindowSize {
            get {
               return new Size( 525, 350 );
            }
        }

        public static Size ListModeWindowSize {
            get {
                return new Size( 300, 300 );
            }
        }

        public static Point WindowOnLeftBottom {
            get {
                return new Point( 0, Convert.ToInt32( System.Windows.SystemParameters.PrimaryScreenHeight - ListModeWindowSize.Height ) );
            }
        }

        public static Point WindowHidden {
            get {
                return new Point( - ListModeWindowSize.Width, Convert.ToInt32( System.Windows.SystemParameters.PrimaryScreenHeight ) );
            }
        }

        public static System.Windows.Rect GetRectByWindow( System.Windows.Window window ) {
            var rect = new System.Windows.Rect( window.Left, window.Top, window.Width, window.Height );
            return rect;
        }

        public static bool In( System.Windows.Rect rect, Point point ) {
            if( rect.X <= point.X && 
                rect.X + rect.Width >= point.X && 
                rect.Y <= point.Y && 
                rect.Y + rect.Height >= point.Y ) {
                return true;
            } else {
                return false;
            }
        }
    }
}
