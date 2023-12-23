﻿namespace ESP32_NF_MQTT_DHT
{
    using Microsoft.Extensions.Logging;

    using Services.Contracts;

    using System;

    /// <summary>
    /// Represents the startup process of the application.
    /// </summary>
    public class Startup
    {
        private readonly IConnectionService _connectionService;
        private readonly IMqttClientService _mqttClient;
        private readonly IDhtService _dhtService;
        private readonly ITcpListenerService _tcpListenerService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="connectionService">Service for managing connections.</param>
        /// <param name="mqttClient">MQTT client service.</param>
        /// <param name="dhtService">DHT sensor service.</param>
        /// <param name="loggerFactory">Factory for creating logger instances.</param>
        public Startup(
            IConnectionService connectionService,
            IMqttClientService mqttClient,
            IDhtService dhtService,
            ILoggerFactory loggerFactory,
            ITcpListenerService tcpListenerService)
        {
            _connectionService = connectionService ?? throw new ArgumentNullException(nameof(connectionService));
            _mqttClient = mqttClient ?? throw new ArgumentNullException(nameof(mqttClient));
            _dhtService = dhtService ?? throw new ArgumentNullException(nameof(dhtService));
            _logger = loggerFactory?.CreateLogger(nameof(Startup)) ?? throw new ArgumentNullException(nameof(loggerFactory));

            _logger.LogInformation("Startup: Initializing application...");
            _tcpListenerService = tcpListenerService;
        }

        /// <summary>
        /// Runs the application services.
        /// </summary>
        public void Run()
        {
            try
            {
                _logger.LogInformation("Startup: Establishing connection...");
                _connectionService.Connect();
                _logger.LogInformation("Startup: Connection established.");

                _logger.LogInformation("Startup: Starting MQTT client...");
                _mqttClient.Start();
                _logger.LogInformation("Startup: MQTT client started.");

                _logger.LogInformation("Startup: Starting DHT service...");
                _dhtService.Start();
                _logger.LogInformation("Startup: DHT service started.");

                _logger.LogInformation("Startup: Starting TcpListener service...");
                _tcpListenerService.Start();
                _logger.LogInformation("Startup: TcpListener service started.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred during startup: {ex.Message}");
                // Consider re-throwing the exception or handling it as needed for your application
            }
        }
    }
}
