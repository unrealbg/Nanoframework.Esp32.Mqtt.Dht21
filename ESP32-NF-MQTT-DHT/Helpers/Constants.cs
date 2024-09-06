﻿namespace ESP32_NF_MQTT_DHT.Helpers
{
    public static class Constants
    {
        public const int MaxReconnectAttempts = 20;
        public const int ReconnectDelay = 10000;
        public const int ErrorInterval = 10000;
        public const int SensorDataInterval = 300000;
        public const double InvalidTemperature = -50;
        public const double InvalidHumidity = -100;
    }
}