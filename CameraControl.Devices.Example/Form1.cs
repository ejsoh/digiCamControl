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
using Timer = System.Timers.Timer;

namespace CameraControl.Devices.Example
{
   
    public partial class Form1 : Form
    {
        public ICameraDevice CameraDevice { get; set; }
        public WebServer WebServer { get; set; }
        public CameraDeviceManager DeviceManager { get; set; }
        public string FolderForPhotos { get; set; }
        private Timer _liveViewTimer = new Timer(500);
        

            

            public Form1()
        {
            
        DeviceManager = new CameraDeviceManager();
            

            //DeviceManager.CameraSelected += DeviceManager_CameraSelected;
            // For experimental Canon driver support- to use canon driver the canon sdk files should be copied in application folder
            DeviceManager.UseExperimentalDrivers = true;
            DeviceManager.DisableNativeDrivers = false;
           
            FolderForPhotos = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Test");
            DeviceManager.ConnectToCamera();
            Console.WriteLine(DeviceManager.ConnectedDevices);
            Thread.Sleep(100);
            foreach (ICameraDevice cameraDevice in DeviceManager.ConnectedDevices)
            {
                Console.WriteLine("here");
                Console.WriteLine(cameraDevice);
                Console.WriteLine(cameraDevice.DisplayName.Contains("Canon"));
                if (cameraDevice.DisplayName.Contains("Canon"))
                {
                    Console.WriteLine(DeviceManager.SelectedCameraDevice);
                    DeviceManager.SelectedCameraDevice = cameraDevice;
                    cameraDevice.StartLiveView();
                    _liveViewTimer.Stop();
                    _liveViewTimer.Elapsed += _liveViewTimer_Tick;
                    _liveViewTimer.AutoReset = true;
                    _liveViewTimer.Enabled = true;
                    new Thread(StartLiveView).Start();
                    //DeviceManager.SelectedCameraDevice = cameraDevice;
                }
            }
           


            DeviceManager.SelectedCameraDevice.CameraDisconnected += CameraDevice_CameraDisconnected;
            
            Console.WriteLine("\nPress the Enter key to exit the application...\n");
            Console.WriteLine("The application started at {0:HH:mm:ss.fff}", DateTime.Now);
            Console.ReadLine();
            

            Console.WriteLine("Terminating the application...");
            

        }


        void DeviceManager_CameraSelected(ICameraDevice oldcameraDevice, ICameraDevice newcameraDevice)
        {
            MethodInvoker method = delegate
            {
                newcameraDevice.GetCapability(CapabilityEnum.LiveView);
            };
            if (InvokeRequired)
                BeginInvoke(method);
            else
                method.Invoke();
        }



        void CameraDevice_CameraDisconnected(object sender, DisconnectCameraEventArgs eventArgs)
        {
            MethodInvoker method = delegate
            {
                _liveViewTimer.Stop();
                Thread.Sleep(100);
                Close();
            };
            if (InvokeRequired)
                BeginInvoke(method);
            else
                method.Invoke();
        }

        void _liveViewTimer_Tick(object sender, EventArgs e)
        {
          
            LiveViewData liveViewData = null;
            try
            {
                Console.WriteLine(DeviceManager.SelectedCameraDevice);
                Console.WriteLine("selected");
                liveViewData = DeviceManager.SelectedCameraDevice.GetLiveViewImage();
                Console.WriteLine(liveViewData.ImageData);
            }
            catch (Exception)
            {
                return;
            }

            if (liveViewData == null || liveViewData.ImageData == null)
            {
                Console.WriteLine("null");
                return;
            }
            try
            {

                Console.WriteLine("사진 만들어");
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
                    
                    Console.WriteLine(DeviceManager.ConnectedDevices);
                    Thread.Sleep(100);
                    Console.WriteLine("라이브뷰");
                    //DeviceManager.SelectedCameraDevice.StartLiveView();
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
            Console.WriteLine("사진 만들어1");
            MethodInvoker method = () => _liveViewTimer.Start();
            if (InvokeRequired)
            {
                Console.WriteLine("사진 만들어트루");
                BeginInvoke(method);
                ///  _liveViewTimer_Tick();
                /// _liveViewTimer.Start();
                ///new Thread(StartLiveView).Start();

            }
            
            else
                method.Invoke();

        }

        

       


    }
}
