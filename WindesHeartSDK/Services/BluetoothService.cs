﻿using Plugin.BluetoothLE;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using WindesHeartSDK.Devices.MiBand3.Models;
using WindesHeartSDK.Models;

namespace WindesHeartSDK
{
    public class BluetoothService
    {
        //Globals
        private readonly BLEDevice BLEDevice;
        private IDevice IDevice => BLEDevice.Device;

        public static AdapterStatus AdapterStatus;

        private static IDisposable AdapterReadyDisposable;
        private static IDisposable CurrentScan;
        private static IDisposable AdapterChangedDisposable;

        public BluetoothService(BLEDevice device)
        {
            BLEDevice = device;
        }

        /// <summary>
        /// Stops scanning for devices
        /// </summary>
        public static void StopScanning()
        {
            CurrentScan?.Dispose();
        }

        /// <summary>
        /// Scan for devices that are not yet connected.
        /// </summary>
        /// <exception cref="System.Exception">Throws exception when trying to start scan when a scan is already running.</exception>
        /// <param name="callback"></param>
        /// <returns>Bool wheter scanning has started</returns>
        public static bool StartScanning(Action<BLEScanResult> callback)
        {
            var uniqueGuids = new List<Guid>();

            //Start scanning when adapter is powered on.
            if (CrossBleAdapter.Current.Status == AdapterStatus.PoweredOn)
            {
                //Trigger event and add to devices list
                Console.WriteLine("Started scanning");
                CurrentScan = CrossBleAdapter.Current.Scan().Subscribe(result =>
                {
                    if (result.Device != null && !string.IsNullOrEmpty(result.Device.Name) && !uniqueGuids.Contains(result.Device.Uuid))
                    {
                        //Set device
                        BLEDevice device = GetDevice(result.Device);

                        BLEScanResult scanResult = new BLEScanResult(device, result.Rssi, result.AdvertisementData);
                        if (device != null)
                        {
                            device.NeedsAuthentication = true;
                            callback(scanResult);
                        }
                        uniqueGuids.Add(result.Device.Uuid);
                    }
                });
                return true;
            }
            return false;
        }

        /// <summary>
        /// Calls the callback method when Bluetooth adapter state changes to ready
        /// </summary>
        /// <param name="callback">Called when adapter is ready</param>
        public static void WhenAdapterReady(Action callback)
        {
            AdapterReadyDisposable?.Dispose();
            AdapterReadyDisposable = CrossBleAdapter.Current.WhenReady().Subscribe(adapter => callback());
        }

        /// <summary>
        /// Return whether device is currently scanning for devices.
        /// </summary>
        public static bool IsScanning()
        {
            return CrossBleAdapter.Current.IsScanning;
        }

        /// <summary>
        /// Calls the callback method when Bluetooth adapter status changes
        /// </summary>
        /// <param name="callback">Called when status changed</param>
        public static void OnAdapterChanged(Action callback)
        {
            AdapterChangedDisposable?.Dispose();
            AdapterChangedDisposable = CrossBleAdapter.Current.WhenStatusChanged().Subscribe(adapter => callback());
        }

        /// <summary>
        /// Connect current device
        /// </summary>
        public void Connect()
        {
            Console.WriteLine("Connecting started...");

            //Connect
            IDevice.Connect(new ConnectionConfig
            {
                AutoConnect = true,
                AndroidConnectionPriority = ConnectionPriority.High
            });
        }

        /// <summary>
        /// Gets a device based on its uuid
        /// </summary>
        /// <param name="uuid">Uuid of device to find</param>
        /// <returns>The device of the uuid</returns>
        public static async Task<BLEDevice> GetKnownDevice(Guid uuid)
        {
            if (uuid != Guid.Empty)
            {
                var knownDevice = await CrossBleAdapter.Current.GetKnownDevice(uuid);

                if (knownDevice != null)
                {
                    var bleDevice = GetDevice(knownDevice);
                    bleDevice.NeedsAuthentication = false;
                    return bleDevice;
                }
            }
            return null;
        }

        /// <summary>
        /// Disconnect current device.
        /// </summary>
        public void Disconnect(bool rememberDevice)
        {
            //Cancel the connection
            Console.WriteLine("Disconnecting device..");
            Windesheart.ConnectedDevice?.DisposeDisposables();
            IDevice.CancelConnection();
            if (!rememberDevice)
            {
                Windesheart.ConnectedDevice = null;
            }
        }

        /// <summary>
        /// Enables logging of device status on change.
        /// </summary>
        public static void StartListeningForAdapterChanges()
        {
            bool startListening = false;
            AdapterReadyDisposable?.Dispose();
            AdapterReadyDisposable = CrossBleAdapter.Current.WhenStatusChanged().Subscribe(async status =>
            {
                if (status != AdapterStatus)
                {
                    AdapterStatus = status;
                    if (status == AdapterStatus.PoweredOff && Windesheart.ConnectedDevice != null)
                    {
                        Windesheart.ConnectedDevice?.Disconnect();
                    }

                    if (status == AdapterStatus.PoweredOn && Windesheart.ConnectedDevice != null && startListening)
                    {
                        var device = await GetKnownDevice(Windesheart.ConnectedDevice.Uuid);
                        device?.Connect(Windesheart.ConnectedDevice?.ConnectionCallback);
                    }
                    startListening = true;
                }
            });
        }


        /// <summary>
        /// Returns the right BLEDevice based on the ScanResult
        /// </summary>
        private static BLEDevice GetDevice(IDevice device)
        {
            var name = device.Name;

            if (name.Equals("Mi Band 3") || name.Equals("Xiaomi Mi Band 3"))
            {
                return new MiBand3(device);
            }

            //Create additional if-statements for devices other than Mi Band 3/Xiami Band 3.
            return null;
        }
    }
}


