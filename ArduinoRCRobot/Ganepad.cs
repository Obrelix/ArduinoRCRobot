using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XInputDotNetPure;

namespace ArduinoWebCam
{
    public class xboxc
    {
        //functions for xbox controller

        public int checkcontroller()
        {
            GamePadState concheck;

            // Simply get the state of the controller from XInput.
            concheck = GamePad.GetState(PlayerIndex.One);

            if (concheck.IsConnected)
            {
                // Controller is connected
                return 1;
            }
            else
            {
                // Controller is not connected 
                return 0;
            }
        }

    }
}
