﻿namespace ESP32_NF_MQTT_DHT.Services.MQTT
{
    using System.Text;
    using System.Threading;

    using ESP32_NF_MQTT_DHT.Helpers;
    using ESP32_NF_MQTT_DHT.Managers.Contracts;
    using ESP32_NF_MQTT_DHT.Services.Contracts;
    using ESP32_NF_MQTT_DHT.Services.MQTT.Contracts;

    using nanoFramework.Json;
    using nanoFramework.M2Mqtt;

    using static ESP32_NF_MQTT_DHT.Helpers.TimeHelper;
    using static ESP32_NF_MQTT_DHT.Settings.DeviceSettings;

    /// <summary>
    /// Service responsible for publishing sensor data and error messages to an MQTT broker.
    /// </summary>
    public class MqttPublishService : IMqttPublishService
    {
        private const int ErrorInterval = 10000;

        private static readonly string DataTopic = $"home/{DeviceName}/messages";
        private static readonly string ErrorTopic = $"home/{DeviceName}/errors";
        private static readonly string SystemTopic = $"home/{DeviceName}/system/status";
        private const int HeartbeatInterval = 30000;

        private readonly LogHelper _logHelper;
        private readonly ManualResetEvent _stopSignal = new ManualResetEvent(false);
        private readonly ManualResetEvent _heartbeatStopSignal = new ManualResetEvent(false);
        private readonly ISensorManager _sensorManager;
        private readonly IInternetConnectionService _internetConnectionService;
        private MqttClient _mqttClient;

        private bool _isHeartbeatRunning = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="MqttPublishService"/> class.
        /// </summary>
        /// <param name="logHelper">The log helper for logging messages.</param>
        /// <param name="sensorManager">The sensor manager for retrieving sensor data.</param>
        /// <param name="internetConnectionService">The internet connection service for checking internet availability.</param>
        public MqttPublishService(LogHelper logHelper, IInternetConnectionService internetConnectionService, ISensorManager sensorManager)
        {
            _logHelper = logHelper;
            _internetConnectionService = internetConnectionService;
            _sensorManager = sensorManager;
        }

        /// <summary>
        /// Sets the MQTT client to be used for publishing messages.
        /// </summary>
        /// <param name="mqttClient">The MQTT client instance.</param>
        public void SetMqttClient(MqttClient mqttClient)
        {
            _mqttClient = mqttClient;
        }

        /// <summary>
        /// Publishes the device status to the MQTT broker.
        /// </summary>
        public void StartHeartbeat()
        {
            if (_isHeartbeatRunning) return;
            _isHeartbeatRunning = true;
            _heartbeatStopSignal.Reset();

            new Thread(() =>
            {
                while (_isHeartbeatRunning)
                {
                    PublishDeviceStatus();
                    _heartbeatStopSignal.WaitOne(HeartbeatInterval, false);
                }
            }).Start();
        }

        /// <summary>
        ///  
        /// </summary>
        public void StopHeartbeat()
        {
            _isHeartbeatRunning = false;
            _heartbeatStopSignal.Set();
        }

        /// <summary>
        /// Publishes the device status to the MQTT broker.
        /// </summary>
        /// <param name="status"></param>
        private void PublishDeviceStatus()
        {
            if (_internetConnectionService.IsInternetAvailable())
            {
                string message = "online";
                _mqttClient.Publish(SystemTopic, Encoding.UTF8.GetBytes(message));
                _logHelper.LogWithTimestamp($"Heartbeat sent: {message}");
            }
            else
            {
                _logHelper.LogWithTimestamp("No internet connection for heartbeat.");
            }
        }

        /// <summary>
        /// Publishes sensor data to the MQTT broker.
        /// </summary>
        public void PublishSensorData()
        {
            var data = _sensorManager.CollectAndCreateSensorData();

            if (data != null)
            {
                var message = JsonSerializer.SerializeObject(data);
                this.CheckInternetAndPublish(DataTopic, message);
            }
            else
            {
                this.PublishError($"[{GetCurrentTimestamp()}] Unable to read sensor data");
                _logHelper.LogWithTimestamp("Unable to read sensor data");
            }
        }

        /// <summary>
        /// Publishes an error message to the MQTT broker.
        /// </summary>
        /// <param name="errorMessage">The error message to be published.</param>
        public void PublishError(string errorMessage)
        {
            this.CheckInternetAndPublish(ErrorTopic, errorMessage);
            _stopSignal.WaitOne(ErrorInterval, false);
        }

        /// <summary>
        /// Checks internet availability and publishes a message to the specified topic.
        /// </summary>
        /// <param name="topic">The topic to publish the message to.</param>
        /// <param name="message">The message to be published.</param>
        private void CheckInternetAndPublish(string topic, string message)
        {
            if (_internetConnectionService.IsInternetAvailable())
            {
                _mqttClient.Publish(topic, Encoding.UTF8.GetBytes(message));
            }
            else
            {
                _logHelper.LogWithTimestamp("No internet connection.");
            }
        }
    }
}