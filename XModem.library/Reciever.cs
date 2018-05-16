using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XModem.library
{
    public static class Reciever
    {
        public static string Recieve(Port port, byte controll)
        {
            string recievedText = "";
            int time = 0;
            byte signal = 0;
            while (time < 60000)
            {
                if (time % 1000 == 0)
                {
                    port.WriteCharacter(controll);
                }

                time += 1000;
                Thread.Sleep(1000);
                signal = port.Read();
                if(signal == Port.ControlValues["SOH"])
                {
                    break;
                }
            }
            byte[] header = new byte[3];
            byte[] dataBlock = new byte[128];
            while (signal == Port.ControlValues["SOH"])
            {
                header[0] = signal;
                header[1] = port.Read();
                header[2] = port.Read();
                for (int i = 0; i < 128; i++)
                {
                    dataBlock[i] = port.Read();
                }

                short retrievedCrcSum = 0;
                if (controll == Port.ControlValues["C"])
                {
                    retrievedCrcSum = (short)((port.Read() << 8) + port.Read());
                }
                else
                {
                    retrievedCrcSum = port.Read();
                }

                if (retrievedCrcSum != port.GetCrcSum(dataBlock) && controll == Port.ControlValues["C"] ||
                    retrievedCrcSum != port.GetCkSum(dataBlock) && controll == Port.ControlValues["NAK"])
                {
                    port.WriteCharacter(Port.ControlValues["NAK"]);
                }
                else if (header[2] != (byte) (255 - header[1]))
                {
                   port.WriteCharacter(Port.ControlValues["NAK"]);
                }
                else
                {
                    recievedText += Encoding.UTF8.GetString(dataBlock);
                    port.WriteCharacter(Port.ControlValues["ACK"]);
                }

                signal = port.Read();
            }

            if (signal == Port.ControlValues["EOT"])
            {
                port.WriteCharacter(Port.ControlValues["ACK"]);
            }


            return recievedText;
        }

    }
}
