﻿namespace ESP32_NF_MQTT_DHT.Services.Contracts
{
    public interface IRelayService
    {
        void TurnOn();

        void TurnOff();
    }
}
