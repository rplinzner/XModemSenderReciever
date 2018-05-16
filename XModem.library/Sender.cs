using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.Forms.MessageBox;

namespace XModem.library
{
    public static class Sender
    {
        public static void Send(Port port, byte controll, string text)
        {
            byte[] textToBytes = Encoding.Default.GetBytes(text);
            GETRESPONSE:
            byte signal = 0;
            int elapsedTime = 0;
            MessageBox.Show("Waiting for Signal", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            while (elapsedTime < 60 && signal != Port.ControlValues["NAK"] && signal != Port.ControlValues["C"])
            {
                signal = port.Read();
                Thread.Sleep(1000);
                elapsedTime++;
            }

            if (elapsedTime == 60)
            {
                var result = MessageBox.Show("Waiting time exceeded. Transmission failed", "Warning", MessageBoxButtons.RetryCancel,
                    MessageBoxIcon.Error);
                if (result == DialogResult.Retry) goto GETRESPONSE;
            }
            else
            {
                MessageBox.Show("Signal Recieved", "Success", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            MemoryStream stringStream = new MemoryStream(textToBytes);
            BinaryReader bReader = new BinaryReader(stringStream);
            byte[] block = new byte[128];
            byte[] header = new byte[3];
            byte numOfBlock = 0;
            while (bReader.BaseStream.Position != bReader.BaseStream.Length)
            {
                for (int i = 0; i < 128; i++)
                {
                    if (bReader.BaseStream.Position != bReader.BaseStream.Length)
                        block[i] = bReader.ReadByte();
                    else
                    {
                        break;
                    }

                }

                header[0] = Port.ControlValues["SOH"];
                header[1] = numOfBlock;
                header[2] = (byte)(255 - numOfBlock);
                do
                {
                    port.Write(header, 0, 3);
                    port.Write(block, 0, 128);
                    if (controll == Port.ControlValues["C"])
                    {
                        var crc = port.GetCrcSum(block);
                        port.WriteCharacter((byte)((crc >> 8) & 0xFF));
                        port.WriteCharacter((byte)(crc & 0xFF));
                    }
                    else
                    {
                        var ckSum = port.GetCkSum(block);
                        port.WriteCharacter(ckSum);
                    }

                    signal = 0;
                    while (signal != Port.ControlValues["ACK"] && signal != Port.ControlValues["NAK"])
                    {
                        signal = port.Read();

                    }

                    if (signal == Port.ControlValues["NAK"])
                    {
                        MessageBox.Show("Error during block transmission. Retrying", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                } while (signal == Port.ControlValues["NAK"]);

                numOfBlock++;
                block = new byte[128];
            }

            do
            {
                port.WriteCharacter(Port.ControlValues["EOT"]);
                Thread.Sleep(100);
                signal = port.Read();
            } while (signal != Port.ControlValues["ACK"]);

            MessageBox.Show("Data Transmission Ended", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            bReader.Close();
            stringStream.Close();
        }
    }
}
