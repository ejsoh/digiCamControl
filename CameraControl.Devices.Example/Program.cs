using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using CameraControl.Devices.Classes;
using CameraControl.Core.Classes;
using CameraControl.Devices.Wifi;
using Timer = System.Windows.Forms.Timer;

namespace CameraControl.Devices.Example
{

    internal class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
               public WebServer WebServer { get; set; }
        public CameraDeviceManager DeviceManager { get; set; }
        public string FolderForPhotos { get; set; }
        private Timer _liveViewTimer = new Timer();
        private static InputArguments _arguments;
        static void Main(string[] args)
        {
            int rc = 0;

            Console.WriteLine(String.Format("digiCamControl remote command line utility ({0}, {1}) running\n", ApplicationInformation.ExecutingAssemblyVersion, ApplicationInformation.CompileDate));

            Application.Run(new Form1());
            System.Environment.Exit(rc);
           
        }

        void Test()
        {

            DeviceManager = new CameraDeviceManager();
            new Thread(StartLiveView).Start();
            //set live view default frame rate to 15
            Console.WriteLine("Arguments :");
            _liveViewTimer.Interval = 1000 / 15;
            _liveViewTimer.Stop();
            _liveViewTimer.Tick += _liveViewTimer_Tick;

            DeviceManager.SelectedCameraDevice.CameraDisconnected += CameraDevice_CameraDisconnected;

        }

        void CameraDevice_CameraDisconnected(object sender, DisconnectCameraEventArgs eventArgs)
        {
            MethodInvoker method = delegate
            {
                _liveViewTimer.Stop();
                Thread.Sleep(100);
              
            };
           
        }

        void _liveViewTimer_Tick(object sender, EventArgs e)
        {
            LiveViewData liveViewData = null;
            try
            {
                liveViewData = DeviceManager.SelectedCameraDevice.GetLiveViewImage();
            }
            catch (Exception)
            {
                return;
            }

            if (liveViewData == null || liveViewData.ImageData == null)
            {
                return;
            }
            try
            {

                new Bitmap(new MemoryStream(liveViewData.ImageData,
                                                                liveViewData.ImageDataPosition,
                                                                liveViewData.ImageData.Length -
                                                                liveViewData.ImageDataPosition)).Save("image.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

            }
            catch (Exception)
            {

            }
        }



        private void StartLiveView()
        {
            bool retry;
            do
            {
                retry = false;
                try
                {
                    DeviceManager.SelectedCameraDevice.StartLiveView();
                }
                catch (DeviceException exception)
                {
                    if (exception.ErrorCode == ErrorCodes.MTP_Device_Busy || exception.ErrorCode == ErrorCodes.ERROR_BUSY)
                    {
                        // this may cause infinite loop
                        Thread.Sleep(100);
                        retry = true;
                    }
                    else
                    {
                        MessageBox.Show("Error occurred :" + exception.Message);
                    }
                }

            } while (retry);
            MethodInvoker method = () => _liveViewTimer.Start();
            

        }






    }

}
