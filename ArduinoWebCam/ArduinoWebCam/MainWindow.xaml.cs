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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Ports;
using System.IO;
using System.Net;
using XInputDotNetPure;
using System.Threading;
using System.Drawing;
using System.Drawing.Imaging;
using AForge.Video;
using AForge;

namespace ArduinoWebCam
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //The following variables are used across several threads
        float shared_left_trig = 0;
        float shared_left_stick_x = 0;
        float shared_left_stick_y = 0;
        float shared_right_trig = 0;
        float shared_right_stick_x = 0;
        float shared_right_stick_y = 0;

        int shared_A = 0;
        int shared_B = 0;
        int shared_X = 0;
        int shared_Y = 0;

        int shared_dpad_up = 0;
        int shared_dpad_down = 0;
        int shared_dpad_left = 0;
        int shared_dpad_right = 0;

        int shared_left_shoulder = 0;
        int shared_right_shoulder = 0;

        int shared_start = 0;
        int shared_back = 0;

        
        int Mode = 0;
        string Movemend_Command = string.Empty;
        string Movemend_CommandCC = "S";
        string Char_Command = "0";
        string Custom_Controls = "F";
        
        
        //counters used for managing the toggling of car functions
        //eg headlights or slow mode. These counters are used to stop repeated toggling on/off
        //if the xbox controller button is held down too long
        int samplecount = 0;
        int oldsamplecount1 = 0;
        SerialPort arduinoport = new SerialPort();

        MJPEGStream stream;

        void Stream_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                System.Drawing.Image bmp = (Bitmap)eventArgs.Frame.Clone();

                MemoryStream ms = new MemoryStream();
                bmp.Save(ms, ImageFormat.Bmp);
                ms.Seek(0, SeekOrigin.Begin);
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.StreamSource = ms;
                bi.EndInit();

                bi.Freeze();
                Dispatcher.BeginInvoke(new ThreadStart(delegate
                {
                    frameholder.Source = bi;
                }));
            }
            catch (Exception)
            {
            }
        }

        public MainWindow()
        {
            InitializeComponent();

        }


  
    
        private void StartCaptureButton_Click(object sender, RoutedEventArgs e)
        {
            stream.Start();
        }

        private void StopCaptureButton_Click(object sender, RoutedEventArgs e)
        {

            stream.Stop();
            BitmapImage img = new BitmapImage();
            img.BeginInit();
            img.UriSource = new Uri(@"pack://application:,,/Images/default_camera.png");
            img.EndInit();
            frameholder.Source = img;
        }

     

        private void ConnectXBox_Click(object sender, EventArgs e)
        {
            int check = 0;
            //The xboxc class is included in Program.cs
            xboxc controlcheck = new xboxc();

            check = controlcheck.checkcontroller();
            if (check == 1)
            {
                statusXbox.Text = "Connected";
                statusXbox.Background = System.Windows.Media.Brushes.Green;
            }
            else
            {
                statusXbox.Background = System.Windows.Media.Brushes.Red;
                statusXbox.Text = "Not Connected";
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Thread for handling Xbox 360 controller
            Thread updatecontroller = new Thread(new ThreadStart(UpdateState));
            updatecontroller.IsBackground = true;
            updatecontroller.Start();

            //Thread for handling serial communications
            Thread updateIO = new Thread(new ThreadStart(UpdateSerial));
            updateIO.IsBackground = true;
            updateIO.Start();


            stream = new MJPEGStream("http://192.168.2.4:4747/mjpegfeed?640x480");
            stream.NewFrame += Stream_NewFrame;
        }

        //---------------------------------------------------------------------------------------------------------
        //The following section contains code for handling the Xbox360 controller
        //---------------------------------------------------------------------------------------------------------

        //Delegates are required so that the background thread can update data in the GUI thread
        //See tutorials on C# cross thread operations for more details

        private delegate void Mode_delegate(float i);

        private delegate void left_trig_delegate(float i);
        private delegate void left_stick_x_delegate(float i);
        private delegate void left_stick_y_delegate(float i);

        private delegate void right_trig_delegate(float i);
        private delegate void right_stick_x_delegate(float i);
        private delegate void right_stick_y_delegate(float i);

        private delegate void left_shoulder_delegate(int status);
        private delegate void right_shoulder_delegate(int status);

        private delegate void start_delegate(int status);
        private delegate void back_delegate(int status);

        private delegate void dpad_up_delegate(int status);
        private delegate void dpad_down_delegate(int status);
        private delegate void dpad_left_delegate(int status);
        private delegate void dpad_right_delegate(int status);

        private delegate void button_A_delegate(int status);
        private delegate void button_B_delegate(int status);
        private delegate void button_X_delegate(int status);
        private delegate void button_Y_delegate(int status);
        
        private delegate void rumble_delegate(int status);

        private void UpdateState()
        {
            while (true)
            {
                GamePadState state = GamePad.GetState(PlayerIndex.One);

                //Read analog control values and save into shared variables

                shared_left_trig = state.Triggers.Left;
                left_trig_value.Dispatcher.Invoke(new left_trig_delegate(display_left_trig), shared_left_trig);
                shared_left_stick_x = state.ThumbSticks.Left.X;
                left_stick_x_value.Dispatcher.Invoke(new left_stick_x_delegate(display_left_stick_x), shared_left_stick_x);
                shared_left_stick_y = state.ThumbSticks.Left.Y;
                left_stick_y_value.Dispatcher.Invoke(new left_stick_y_delegate(display_left_stick_y), shared_left_stick_y);

                shared_right_trig = state.Triggers.Right;
                right_trig_value.Dispatcher.Invoke(new right_trig_delegate(display_right_trig), shared_right_trig);
                shared_right_stick_x = state.ThumbSticks.Right.X;
                right_stick_x_value.Dispatcher.Invoke(new right_stick_x_delegate(display_right_stick_x), shared_right_stick_x);
                shared_right_stick_y = state.ThumbSticks.Right.Y;
                right_stick_y_value.Dispatcher.Invoke(new right_stick_y_delegate(display_right_stick_y), shared_right_stick_y);

                //Update digital button values
                //The xinputdotnetpure wrapper returns the text "Pressed" or "Released" for button state
                //The following if statements read this status for the various buttons, and update the shared status variables

                if (state.Buttons.Start.ToString().Equals("Pressed"))
                {
                    shared_start= 1;
                }
                else if (state.Buttons.Start.ToString().Equals("Released"))
                {
                    shared_start = 0;
                }
                if (state.Buttons.Back.ToString().Equals("Pressed"))
                {
                    shared_back = 1;
                }
                else if (state.Buttons.Back.ToString().Equals("Released"))
                {
                    shared_back = 0;
                }

                if (state.Buttons.LeftShoulder.ToString().Equals("Pressed"))
                {
                    shared_left_shoulder = 1;
                }
                else if (state.Buttons.LeftShoulder.ToString().Equals("Released"))
                {
                    shared_left_shoulder = 0;
                }
                if (state.Buttons.RightShoulder.ToString().Equals("Pressed"))
                {
                    shared_right_shoulder = 1;
                }
                else if (state.Buttons.RightShoulder.ToString().Equals("Released"))
                {
                    shared_right_shoulder = 0;
                }

                if (state.DPad.Up.ToString().Equals("Pressed"))
                {
                    //simple check to stop function toggling on/off when button is held too long
                    if ((samplecount - oldsamplecount1) > 3 || (samplecount - oldsamplecount1) < 0)
                    {
                        shared_dpad_up = 1;
                    }
                    oldsamplecount1 = samplecount;
                }
                else if (state.DPad.Up.ToString().Equals("Released"))
                {
                    shared_dpad_up = 0;
                }

                if (state.DPad.Down.ToString().Equals("Pressed"))
                {
                    //simple check to stop function toggling on/off when button is held too long
                    if ((samplecount - oldsamplecount1) > 3 || (samplecount - oldsamplecount1) < 0)
                    {
                        shared_dpad_down = 1;
                    }
                    oldsamplecount1 = samplecount;
                }
                else if (state.DPad.Down.ToString().Equals("Released"))
                {
                    shared_dpad_down = 0;
                }

                if (state.DPad.Left.ToString().Equals("Pressed"))
                {
                    //simple check to stop function toggling on/off when button is held too long
                    if ((samplecount - oldsamplecount1) > 3 || (samplecount - oldsamplecount1) < 0)
                    {
                        shared_dpad_left = 1;
                    }
                    oldsamplecount1 = samplecount;
                }
                else if (state.DPad.Left.ToString().Equals("Released"))
                {
                    shared_dpad_left = 0;
                }

                if (state.DPad.Right.ToString().Equals("Pressed"))
                {
                    //simple check to stop function toggling on/off when button is held too long
                    if ((samplecount - oldsamplecount1) > 3 || (samplecount - oldsamplecount1) < 0)
                    {
                        shared_dpad_right = 1;
                    }
                    oldsamplecount1 = samplecount;
                }
                else if (state.DPad.Right.ToString().Equals("Released"))
                {
                    shared_dpad_right = 0;
                }


                if (state.Buttons.A.ToString().Equals("Pressed"))
                {
                    shared_A = 1;
                    //GamePad.SetVibration(PlayerIndex.One, 0.5f,0.0f);     //test code - left motor vibrates when A is pressed
                }
                else if (state.Buttons.A.ToString().Equals("Released"))
                {
                    shared_A = 0;
                    //GamePad.SetVibration(PlayerIndex.One, 0.0f, 0.0f);    //test code - vibration is off when A is released
                }
                if (state.Buttons.B.ToString().Equals("Pressed"))
                {
                    //simple check to stop function toggling on/off when button is held too long
                    if ((samplecount - oldsamplecount1) > 3 || (samplecount - oldsamplecount1) < 0)
                    {
                        shared_B = 1;
                    }
                    oldsamplecount1 = samplecount;
                }
                else if (state.Buttons.B.ToString().Equals("Released"))
                {
                    shared_B = 0;
                }
                if (state.Buttons.X.ToString().Equals("Pressed"))
                {
                    shared_X = 1;
                }
                else if (state.Buttons.X.ToString().Equals("Released"))
                {
                    shared_X = 0;
                }
                if (state.Buttons.Y.ToString().Equals("Pressed"))
                {
                    //simple check to stop function toggling on/off when button is held too long
                    if ((samplecount - oldsamplecount1) > 3 || (samplecount - oldsamplecount1) < 0)
                    {
                        shared_Y = 1;
                    }
                    oldsamplecount1 = samplecount;
                }
                else if (state.Buttons.Y.ToString().Equals("Released"))
                {
                    shared_Y = 0;
                }

                //The following 'invokes' update the GUI with the various button statuses
                start_status.Dispatcher.Invoke(new left_shoulder_delegate(display_start), shared_start);
                back_status.Dispatcher.Invoke(new right_shoulder_delegate(display_back), shared_back);
                left_shoulder_status.Dispatcher.Invoke(new left_shoulder_delegate(display_left_shoulder), shared_left_shoulder);
                right_shoulder_status.Dispatcher.Invoke(new right_shoulder_delegate(display_right_shoulder), shared_right_shoulder);
                dpad_up_status.Dispatcher.Invoke(new dpad_up_delegate(display_dpad_up), shared_dpad_up);
                dpad_down_status.Dispatcher.Invoke(new dpad_down_delegate(display_dpad_down), shared_dpad_down);
                dpad_left_status.Dispatcher.Invoke(new dpad_left_delegate(display_dpad_left), shared_dpad_left);
                dpad_right_status.Dispatcher.Invoke(new dpad_right_delegate(display_dpad_right), shared_dpad_right);
                A_button_status.Dispatcher.Invoke(new button_A_delegate(display_button_A), shared_A);
                B_button_status.Dispatcher.Invoke(new button_B_delegate(display_button_B), shared_B);
                X_button_status.Dispatcher.Invoke(new button_X_delegate(display_button_X), shared_X);
                Y_button_status.Dispatcher.Invoke(new button_Y_delegate(display_button_Y), shared_Y);

              


                //Put a limit on how frequently this updates
                Thread.Sleep(5);

                //Frame counter for managing repeated on/off toggling of car functions
                //For example, this counter is used for the headlight on/off toggle, to stop it
                //turning on/off continuously when the button is held down.
                samplecount++;
                //manually reset counter if it gets large
                if (samplecount > 2000000000)
                {
                    samplecount = 0;
                }
            }
        }

      
        

        

        //Functions for use with the delegates
        private void display_left_trig(float i)
        {
            left_trig_value.Text = String.Format("{0:0.000}", i);
            if (i == 0)
            {
                
                left_trig_value.Background = System.Windows.Media.Brushes.LightBlue;
            }
            else
            {
                left_trig_value.Background = System.Windows.Media.Brushes.Orange;
            }
        }

        private void display_left_stick_x(float i)
        {
            left_stick_x_value.Text = String.Format("{0:0.000}", i);
            if (i == 0)
            {
                left_stick_x_value.Background = System.Windows.Media.Brushes.LightBlue;
            }
            else if (i < 0)
            {
                left_stick_x_value.Background = System.Windows.Media.Brushes.Orange;
            }
            else if (i > 0)
            {
                left_stick_x_value.Background = System.Windows.Media.Brushes.Orange;
            }
        }

        private void display_left_stick_y(float i)
        {
            left_stick_y_value.Text = String.Format("{0:0.000}", i);
            if (i == 0)
            {
                left_stick_y_value.Background = System.Windows.Media.Brushes.LightBlue;
            }
            else if(i < 0)
            {
                
                left_stick_y_value.Background = System.Windows.Media.Brushes.Orange;
            }
            else if (i > 0)
            {
                
                left_stick_y_value.Background = System.Windows.Media.Brushes.Orange;
            }
        }

        private void display_right_trig(float i)
        {
            right_trig_value.Text = String.Format("{0:0.000}", i);
            if (i == 0)
            {

                right_trig_value.Background = System.Windows.Media.Brushes.LightBlue;
            }
            else 
            {
                right_trig_value.Background = System.Windows.Media.Brushes.Orange;
            }
          
        }

        /// <summary>
        /// /////////////////////////////////////////////////////////////////////////////////////
        /// </summary>
        /// <param name="i"></param>

        private void display_right_stick_x(float i)
        {
            right_stick_x_value.Text = String.Format("{0:0.000}", i);
            if (i == 0)
            {
                
                right_stick_x_value.Background = System.Windows.Media.Brushes.LightBlue;
            }
            else if (i < 0)
            {
               
                right_stick_x_value.Background = System.Windows.Media.Brushes.Orange;
            }
              else if (i > 0)
            {
                
                right_stick_x_value.Background = System.Windows.Media.Brushes.Orange;
            }
        }

        private void display_right_stick_y(float i)
        {
            right_stick_y_value.Text = String.Format("{0:0.000}", i);
            if (i == 0)
            {
               
                right_stick_y_value.Background = System.Windows.Media.Brushes.LightBlue;
            }
            else if (i < 0)
            {
                
                right_stick_y_value.Background = System.Windows.Media.Brushes.Orange;
            }
              else if (i > 0)
            {
                
                right_stick_y_value.Background = System.Windows.Media.Brushes.Orange;
            }
        }

        private void display_left_shoulder(int status)
        {
            if (status == 1)
            {
               
                left_shoulder_status.Background = System.Windows.Media.Brushes.Orange;
                
            }
            else
            {
                left_shoulder_status.Background = System.Windows.Media.Brushes.LightBlue;
            }
        }

        private void display_right_shoulder(int status)
        {
            if (status == 1)
            {
                
                right_shoulder_status.Background = System.Windows.Media.Brushes.Orange;
            }
            else
            {
                right_shoulder_status.Background = System.Windows.Media.Brushes.LightBlue;
            }
        }

        private void display_start(int status)
        {
            if (status == 1)
            {
                Arduino_Connection();
                stream.Start();
                start_status.Background = System.Windows.Media.Brushes.Orange;
                
            }
            else
            {
                start_status.Background = System.Windows.Media.Brushes.LightBlue;
            }
        }

        private void display_back(int status)
        {
            if (status == 1)
            {
                Arduino_Connection();
                stream.Stop();
                back_status.Background = System.Windows.Media.Brushes.Orange;
            }
            else
            {
                back_status.Background = System.Windows.Media.Brushes.LightBlue;
            }
        }

        private void display_dpad_up(int status)
        {
            if (status == 1)
            {
                if (Mode == 0)
                {
                    Mode = 1;
                    mode.Text = "1";
                    mode.Background = System.Windows.Media.Brushes.Green;
                }
                dpad_up_status.Background = System.Windows.Media.Brushes.Orange;
            }
            else
            {
                dpad_up_status.Background = System.Windows.Media.Brushes.LightBlue;
            }
        }
        private void display_dpad_down(int status)
        {
            if (status == 1)
            {
                if (Mode == 1)
                {
                    Mode = 0;
                    mode.Text = "0";
                    mode.Background = System.Windows.Media.Brushes.Yellow;
                }
                dpad_down_status.Background = System.Windows.Media.Brushes.Orange;
            }
            else
            {
                dpad_down_status.Background = System.Windows.Media.Brushes.LightBlue;
            }
        }
        private void display_dpad_left(int status)
        {
            if (status == 1)
            {
                dpad_left_status.Background = System.Windows.Media.Brushes.Orange;
            }
            else
            {
                dpad_left_status.Background = System.Windows.Media.Brushes.LightBlue;
            }
        }
        private void display_dpad_right(int status)
        {
            if (status == 1)
            {
                dpad_right_status.Background = System.Windows.Media.Brushes.Orange;
            }
            else
            {
                dpad_right_status.Background = System.Windows.Media.Brushes.LightBlue;
            }
        }

        private void display_button_A(int status)
        {
            if (status == 1)
            {
                reset();
                A_button_status.Background = System.Windows.Media.Brushes.Orange;
            }
            else
            {
                A_button_status.Background = System.Windows.Media.Brushes.LightBlue;
            }
        }

        private void display_button_B(int status)
        {
            if (status == 1)
            {
                B_button_status.Background = System.Windows.Media.Brushes.Orange;
            }
            else
            {
                B_button_status.Background = System.Windows.Media.Brushes.LightBlue;
            }
        }

        private void display_button_X(int status)
        {
            if (status == 1)
            {
                X_button_status.Background = System.Windows.Media.Brushes.Orange;
            }
            else
            {
                X_button_status.Background = System.Windows.Media.Brushes.LightBlue;
            }
        }

        private void display_button_Y(int status)
        {
            if (status == 1)
            {
                Y_button_status.Background = System.Windows.Media.Brushes.Orange;
            }
            else
            {
                Y_button_status.Background = System.Windows.Media.Brushes.LightBlue;
            }
        }
        
        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            Arduino_Connection();
        }

        private void Arduino_Connection()
        {
            if (arduinoport.IsOpen == false)
            {
                try
                {
                    //open serial port, if not already opened
                    if (arduinoport.IsOpen == false)
                    {
                        //arduinoport.PortName = "COM3";
                        //Default portname is set by the textbox "comport_textbox"
                        arduinoport.PortName = comport_TextBlock.Text;
                        arduinoport.BaudRate = 9600;
                        arduinoport.DataBits = 8;
                        arduinoport.StopBits = StopBits.One;
                        arduinoport.Parity = Parity.None;
                        arduinoport.Open();
                    }
                    status.Text = "Connected";
                    status.Background = System.Windows.Media.Brushes.Green;
                }
                catch (Exception)
                {

                }
            }
            else
            {
                try
                {
                    arduinoport.Close();
                    status.Text = "Disconnected";
                    status.Background = System.Windows.Media.Brushes.Red;
                }
                catch (Exception)
                {

                }
            }
        }
       
      
        

        private void Up_Click(object sender, RoutedEventArgs e)
        {

            if (checkBox.IsChecked == true)
            {
                up(); 
            }
        }

        private void Down_Click(object sender, RoutedEventArgs e)
        {
            if (checkBox.IsChecked == true)
            {
                down();
            }
        }

        private void Left_Click(object sender, RoutedEventArgs e)
        {
            if (checkBox.IsChecked == true)
            {
                left();
            }
        }

        private void Right_Click(object sender, RoutedEventArgs e)
        {
            if (checkBox.IsChecked == true)
            {
                right();
            }
        }

        private void UpLeft_Click(object sender, RoutedEventArgs e)
        {
            if (checkBox.IsChecked == true)
            {
                upleft();
            }
        }

        private void UpRight_Click(object sender, RoutedEventArgs e)
        {
            if (checkBox.IsChecked == true)
            {
                upright();
            }
        }

        private void DownLeft_Click(object sender, RoutedEventArgs e)
        {
            if (checkBox.IsChecked == true)
            {
                downleft();
            }
        }

        private void DownRight_Click(object sender, RoutedEventArgs e)
        {
            if (checkBox.IsChecked == true)
            {
                downRight();
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            if (checkBox.IsChecked == true)
            {
                reset();
            }
        }
        

       

       


        private void up()
        {
            Char_Command = "U";
        }
        private void down()
        {
            Char_Command = "D";
        }
        private void left()
        {
            Char_Command = "L";
        }
        private void right()
        {
            Char_Command = "R";
        }
        private void upleft()
        {
            Char_Command = "7";
        }
        private void downleft()
        {
            Char_Command = "1";
        }
        private void downRight()
        {
            Char_Command = "3";
        }
        private void upright()
        {
            Char_Command = "9";
        }
        private void reset()
        {
            Char_Command = "S";
        }

        private string calculateMovment()
        {

            if (Custom_Controls == "F")
            {
                if (shared_left_stick_x > 0)
                {
                    Movemend_Command = "R";
                }
                else if (shared_left_stick_x < 0)
                {
                    Movemend_Command = "L";
                }
                else if (shared_right_trig > 0)
                {
                    Movemend_Command = "F";
                }
                else if (shared_right_trig < shared_left_trig)
                {
                    Movemend_Command = "B";
                }
                else
                {
                    Movemend_Command = "S";
                }

                
            }
            else
            {
                Movemend_Command = Movemend_CommandCC;
            }
            return Movemend_Command;
        }


        private delegate void output_delegate(string output);
        private delegate void input_delegate(string input);
        private delegate void connection_delegate(string status);
        private void display_output(string output)
        {
            Serialtxt.Text = output;
        }
        private void display_input(string input)
        {
            SerialtxtInput.Text = input;
        }
        private void display_status(string state)
        {

            if (state == "Disconnected")
            {
                status.Background = System.Windows.Media.Brushes.Red;

            }
            else
            {
                status.Background = System.Windows.Media.Brushes.Green;
            }
            status.Text = state;
        }
        
        private void UpdateSerial()
        {
            string received = string.Empty;
            //serial operations
            //polling function
            while (true)
            {
                //If serial is connected
                if (arduinoport.IsOpen == true)
                {
                    try
                    {
                        received = arduinoport.ReadLine();
                    }
                    catch (Exception)
                    {

                    }
                    if (received.Contains("A") && received.Contains("Z"))
                    {
                        try
                        {
                            SerialtxtInput.Dispatcher.Invoke(new input_delegate(display_input), received);
                            string output = string.Empty;
                            Char_Command_Calc();
                            output = calculateMovment() + "," + Char_Command + ",Z,"
                                
                                + Mode+ ","+ Custom_Controls + ","  ;
                            //output = calculateMovment() + "," + Char_Command + ",Z" + Environment.NewLine;
                            arduinoport.DiscardOutBuffer();
                            arduinoport.WriteLine(output);
                            Serialtxt.Dispatcher.Invoke(new output_delegate(display_output), output);
                            received = "\n";
                            status.Dispatcher.Invoke(new connection_delegate(display_status), "Connected");
                        }
                        catch (Exception)
                        {

                        }

                        Thread.Sleep(10);
                    }
                }
                else
                {
                    Serialtxt.Dispatcher.Invoke(new output_delegate(display_output), "No connection");
                    SerialtxtInput.Dispatcher.Invoke(new input_delegate(display_input), "No connection");
                    status.Dispatcher.Invoke(new connection_delegate(display_status), "Disconnected");
                    
                }
                if (arduinoport.IsOpen == false)
                {
                    Thread.Sleep(20);
                }
            }
        }

        private void Char_Command_Calc()
        {

            if (Custom_Controls == "F")
            {
                if (shared_right_stick_x < 0)
                {
                    left();
                }
                else if (shared_right_stick_x > 0)
                {
                    right();
                }
                if (shared_right_stick_y > 0)
                {
                    up();
                }
                else if (shared_right_stick_y < 0)
                {
                    down();
                }
                if (shared_right_stick_x == 0 && shared_right_stick_y == 0)
                {
                    Char_Command = "0";
                } 
            }
        }

        private string CalculateX()
        {
            string X = string.Empty;
            float status = 0;
            int x, nx;
            status = (shared_right_stick_x * 100);
            
            x = Convert.ToInt16(status);
            nx = map(100, -100, 0, 180, x);
            if (nx < 10)
            {
                nx = 10;
            }
            
            X = String.Format("{0:000}", nx)  ;
           
            return X;
        }
        private string CalculateY()
        {
            string X = string.Empty;
            float status = 0;
            int y, ny;
            status = (shared_right_stick_y * 100);

            y = Convert.ToInt16(status);
            ny = map(100, -100, 0, 180, y);
            if (ny < 20)
            {
                ny = 20;
            }
            else if (ny > 145)
            {
                ny = 145;
            }
            X = String.Format("{0:000}", ny);

            return X;
        }

        private  int map(
            int originalStart, int originalEnd, // original range
            int newStart, int newEnd, // desired range
            int value) // value to convert
        {
            double scale = (double)(newEnd - newStart) / (originalEnd - originalStart);
            return (int)(newStart + ((value - originalStart) * scale));
        }

        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            if (checkBox.IsChecked == true)
            {
                Custom_Controls = "T";
            }
            else
            {
                Custom_Controls = "F";
            }
        }

        private void Forward_Click(object sender, RoutedEventArgs e)
        {
            if (checkBox.IsChecked == true)
            {
                Movemend_CommandCC = "F";
            }
        }

        private void BackwardClick(object sender, RoutedEventArgs e)
        {
            if (checkBox.IsChecked == true)
            {
                Movemend_CommandCC = "B";
            }
        }

        private void TurnRight_Click(object sender, RoutedEventArgs e)
        {
            if (checkBox.IsChecked == true)
            {
                Movemend_CommandCC = "R";
            }
        }

        private void TurnLeft_Click(object sender, RoutedEventArgs e)
        {
            if (checkBox.IsChecked == true)
            {
                Movemend_CommandCC = "L";
            }
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            if (checkBox.IsChecked == true)
            {
                Movemend_CommandCC = "S";
            }
        }
        private void StopCamera(object sender, RoutedEventArgs e)
        {
            if (checkBox.IsChecked == true)
            {
                Char_Command = "0";
            }
        }
    }


}