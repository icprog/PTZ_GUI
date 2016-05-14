
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO.Ports;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PTZ_GUI {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow:Window, INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        Point panelPoint;
        Point originPoint;
        Point xStartPoint;
        Point xEndPoint;
        Point yStartPoint;
        Point yEndPoint;

        //保留的小数位数
        int dec = PointExtend.dec;
        // 绘制的坐标单位像素间隙
        int tickInterval = 30;

        public MainWindow() {
            InitializeComponent();
            InitVar();
            DrawCoordinate();
        }


        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendData_Click(object sender,RoutedEventArgs e) {
            SerialSendStringASCII("start:" + calcPoint.GetPolarTheta() + "," + calcPoint.GetRadius() + "\r\n");
        }

        #region 属性
        /// <summary>
        /// 云平台长度
        /// </summary>
        private double _PTZlength;

        public double PTZLength {
            get {
                return _PTZlength;
            }
            set {
                if(_PTZlength != value) {
                    _PTZlength = value;
                    PropertyChanged(this,new PropertyChangedEventArgs(nameof(PTZLength)));
                }
            }
        }



        /// <summary>
        /// 坐标单位长度
        /// </summary>
        private double _unitLength;

        public double UnitLength {
            get {
                return _unitLength;
            }
            set {
                if(_unitLength != value) {
                    _unitLength = value;
                    PropertyChanged(this,new PropertyChangedEventArgs(nameof(UnitLength)));
                }
            }
        }


        private string[] _usefulPort;

        public string[] UsefulPort {
            get {
                return _usefulPort;
            }
            set {
                if(_usefulPort != value) {
                    _usefulPort = value;
                    PropertyChanged(this,new PropertyChangedEventArgs(nameof(UsefulPort)));

                }
            }
        }


 

        #endregion
        private SerialPort serialPort = new SerialPort();

        /// <summary>
        /// 初始化相关变量
        /// </summary>
        private void InitVar() {
            panelPoint = new Point(coorPanel.Width,coorPanel.Height);
            originPoint = new Point(panelPoint.X / 2,panelPoint.Y / 2);
            xStartPoint = new Point(0,panelPoint.Y / 2);
            xEndPoint = new Point(panelPoint.X,panelPoint.Y / 2);
            yStartPoint = new Point(panelPoint.X / 2,panelPoint.Y);
            yEndPoint = new Point(panelPoint.X / 2,0);
            coorPanel.Children.Add(clickLine);
            clickLabel.HorizontalAlignment = HorizontalAlignment.Left;
            clickLabel.VerticalAlignment = VerticalAlignment.Top;
            coorPanel.Children.Add(clickLabel);

            PTZLength = 200;
            UnitLength = 30;

            ShowCurCoord(new Point(100,100));
            sendButton.IsEnabled = false;
        }

        /// <summary>
        /// 绘制坐标系
        /// </summary>
        private void DrawCoordinate() {
            Line xLine = new Line();
            xLine.StrokeThickness = 2;
            xLine.Stroke = Brushes.Black;
            xLine.X1 = xStartPoint.X;
            xLine.Y1 = xStartPoint.Y;
            xLine.X2 = xEndPoint.X;
            xLine.Y2 = xEndPoint.Y;
            coorPanel.Children.Add(xLine);

            Line yLine = new Line();
            yLine.Stroke = Brushes.Black;
            yLine.StrokeThickness = 2;
            yLine.X1 = yStartPoint.X;
            yLine.Y1 = yStartPoint.Y;
            yLine.X2 = yEndPoint.X;
            yLine.Y2 = yEndPoint.Y;
            coorPanel.Children.Add(yLine);

            //X轴正方向
            for(double i = originPoint.X + tickInterval;i < xEndPoint.X;i += tickInterval) {
                Line xTickPlus = new Line();
                xTickPlus.Stroke = Brushes.Black;
                xTickPlus.StrokeThickness = 2;
                xTickPlus.X1 = i;
                xTickPlus.X2 = i;
                xTickPlus.Y1 = originPoint.Y;
                xTickPlus.Y2 = originPoint.Y - 10;
                coorPanel.Children.Add(xTickPlus);
            }

            //X轴负方向
            for(double i = originPoint.X - tickInterval;i > xStartPoint.X;i -= tickInterval) {
                Line xTickMinus = new Line();
                xTickMinus.Stroke = Brushes.Black;
                xTickMinus.StrokeThickness = 2;
                xTickMinus.X1 = i;
                xTickMinus.X2 = i;
                xTickMinus.Y1 = originPoint.Y;
                xTickMinus.Y2 = originPoint.Y - 10;
                coorPanel.Children.Add(xTickMinus);
            }

            //Y轴正方向
            for(double i = originPoint.Y - tickInterval;i > yEndPoint.Y;i -= tickInterval) {
                Line yTickPlus = new Line();
                yTickPlus.Stroke = Brushes.Black;
                yTickPlus.StrokeThickness = 2;
                yTickPlus.X1 = originPoint.X;
                yTickPlus.X2 = originPoint.X + 10;
                yTickPlus.Y1 = i;
                yTickPlus.Y2 = i;
                coorPanel.Children.Add(yTickPlus);
            }

            //Y轴负方向
            for(double i = originPoint.Y + tickInterval;i < yStartPoint.Y;i += tickInterval) {
                Line yTickMinus = new Line();
                yTickMinus.Stroke = Brushes.Black;
                yTickMinus.StrokeThickness = 2;
                yTickMinus.X1 = originPoint.X;
                yTickMinus.X2 = originPoint.X + 10;
                yTickMinus.Y1 = i;
                yTickMinus.Y2 = i;
                coorPanel.Children.Add(yTickMinus);
            }

        }

        /// <summary>
        /// 坐标转换
        /// </summary>
        /// <param name="point">待转换坐标，相对于coordPanel的坐标</param>
        /// <returns></returns>
        private Point ScreenToCoord(Point point) {
            point.X -= originPoint.X;
            point.Y = originPoint.Y - point.Y;
            return point;
        }

        private Point CoordToScreen(Point point) {
            point.X += originPoint.X;
            point.Y = originPoint.Y - point.Y;
            return point;
        }
     
        /// <summary>
        /// 坐标显示相关
        /// </summary>
        Line  clickLine = new Line();
        Label clickLabel = new Label();
        Point calcPoint = new Point();
     
        /// <summary>
        /// 显示鼠标点击的坐标
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowCurCoord_MouseLeftButtonDown(object sender,System.Windows.Input.MouseButtonEventArgs e) {
            Point clickPoint = e.GetPosition(coorPanel);
            Point data = ScreenToCoord(clickPoint);
            ShowCurCoord(data);
        }

        /// <summary>
        /// 指定坐标点显示
        /// </summary>
        /// <param name="data"></param>
        private void ShowCurCoord(Point data) {
            Point pos = CoordToScreen(data);
            calcPoint.X = Math.Round(data.X * UnitLength / tickInterval,dec);
            calcPoint.Y = Math.Round(data.Y * UnitLength / tickInterval,dec);

            clickLine.Stroke = Brushes.Red;
            clickLine.StrokeThickness = 2;
            clickLine.X1 = originPoint.X;
            clickLine.X2 = pos.X;
            clickLine.Y1 = originPoint.Y;
            clickLine.Y2 = pos.Y;


            clickLabel.Margin = new Thickness(pos.X,pos.Y - 10,0,0);
            clickLabel.Content = "(" + calcPoint.GetPolarTheta() + "°, " + calcPoint.GetRadius() + ")";
        }
       
        /// <summary>
        /// window初始化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender,RoutedEventArgs e) {
             // 串口设置初始化
                string[] portsName = SerialPort.GetPortNames();
                Array.Sort(portsName);
                UsefulPort = portsName;
        }

        /// <summary>
        /// 选择串口号
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void spComBox_SelectionChanged(object sender,SelectionChangedEventArgs e) {
            ComboBox cb = sender as ComboBox;
            if(cb == null)
                return;
            if(serialPort.IsOpen) {
                serialPort.Close();
            }
            serialPort.PortName = (string)cb.SelectedItem;
                serialPort.Parity = Parity.None;
                serialPort.StopBits = StopBits.One;                  
            try{
                serialPort.Open();
                spState.Content = "连接成功";
                spState.Foreground = Brushes.Green;
                sendButton.IsEnabled = true;
            } catch {
                spState.Content = "端口被占用";
                spState.Foreground = Brushes.Red;
                sendButton.IsEnabled = false;
            }
        }


        /// <summary>
        /// 串口发送ASCII
        /// </summary>
        /// <param name="strSend"></param>
        private void SerialSendStringASCII(string strSend) {
            if(!serialPort.IsOpen) {
                return;
            }
            byte[] sendData = Encoding.Default.GetBytes(strSend);
            serialPort.Write(sendData,0,sendData.Length);

        }

     
    }

   
    
    /// <summary>
    /// Point类扩展
    /// </summary>
    public static class PointExtend {
      public  static int dec = 1;
        public static double GetPolarTheta(this Point point) {
            double angle = Math.Atan2(point.Y,point.X) * 180 / Math.PI;
           angle =  (angle > 0) ? angle : 360 + angle;
            return  Math.Round(angle,dec);
        }
        public static double GetRadius(this Point point) {
            return Math.Round((Math.Sqrt(Math.Pow(point.X,2) + Math.Pow(point.Y,2))),dec);
        }

    }

   
}
