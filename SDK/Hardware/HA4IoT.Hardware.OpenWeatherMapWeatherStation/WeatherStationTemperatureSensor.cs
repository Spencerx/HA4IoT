﻿using System;
using HA4IoT.Actuators;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Networking;

namespace HA4IoT.Hardware.OpenWeatherMapWeatherStation
{
    public class WeatherStationTemperatureSensor : SingleValueSensorActuatorBase<ActuatorSettings>, ITemperatureSensor
    {
        public WeatherStationTemperatureSensor(ActuatorId id, IHttpRequestController api, ILogger logger) 
            : base(id, api, logger)
        {
            Settings = new ActuatorSettings(id, logger);
        }

        public void SetValue(double value)
        {
            SetValueInternal(Convert.ToSingle(value));
        }
    }
}