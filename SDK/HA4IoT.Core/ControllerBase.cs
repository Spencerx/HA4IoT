﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Networking;
using HA4IoT.Core.Discovery;
using HA4IoT.Core.Timer;
using HA4IoT.Hardware.Pi2;
using HA4IoT.Networking;

namespace HA4IoT.Core
{
    public abstract class ControllerBase : IController
    {
        private readonly DeviceCollection _devices = new DeviceCollection();
        private readonly AreaCollection _areas = new AreaCollection();
        private readonly ActuatorCollection _actuators = new ActuatorCollection();
        private readonly AutomationCollection _automations = new AutomationCollection();
        
        private HealthMonitor _healthMonitor;
        private BackgroundTaskDeferral _deferral;
        private HttpServer _httpServer;

        public ILogger Logger { get; protected set; }
        public IHttpRequestController HttpApiController { get; protected set; }
        public IHomeAutomationTimer Timer { get; protected set; }
        public IControllerSettings Settings { get; private set; }

        public void RunAsync(IBackgroundTaskInstance taskInstance)
        {
            if (taskInstance == null) throw new ArgumentNullException(nameof(taskInstance));

            _deferral = taskInstance.GetDeferral();

            Task.Factory.StartNew(InitializeCore, CancellationToken.None, TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }
        
        public void AddArea(IArea area)
        {
            _areas.AddUnique(area.Id, area);
        }

        public IArea Area(AreaId id)
        {
            return _areas.Get(id);
        }

        public IList<IArea> Areas()
        {
            return _areas.GetAll();
        }

        public void AddActuator(IActuator actuator)
        {
            _actuators.AddOrUpdate(actuator.Id, actuator);
        }

        public TActuator Actuator<TActuator>(ActuatorId id) where TActuator : IActuator
        {
            return _actuators.Get<TActuator>(id);
        }

        public TActuator Actuator<TActuator>() where TActuator : IActuator
        {
            return _actuators.Get<TActuator>();
        }

        public IList<TActuator> Actuators<TActuator>() where TActuator : IActuator
        {
            return _actuators.GetAll<TActuator>();
        }

        public IList<IActuator> Actuators()
        {
            return _actuators.GetAll();
        }

        public void AddDevice(IDevice device)
        {
            _devices.AddUnique(device.Id, device);
        }

        public TDevice Device<TDevice>(DeviceId id) where TDevice : IDevice
        {
            return _devices.Get<TDevice>(id);
        }

        public TDevice Device<TDevice>() where TDevice : IDevice
        {
            return _devices.Get<TDevice>();
        }

        public IList<TDevice> Devices<TDevice>() where TDevice : IDevice
        {
            return _devices.GetAll<TDevice>();
        }

        public IList<IDevice> Devices()
        {
            return _devices.GetAll();
        }

        public void AddAutomation(IAutomation automation)
        {
            _automations.AddOrUpdate(automation.Id, automation);
        }

        public IList<TAutomation> Automations<TAutomation>() where TAutomation : IAutomation
        {
            return _automations.GetAll<TAutomation>();
        }

        public TAutomation Automation<TAutomation>(AutomationId id) where TAutomation : IAutomation
        {
            return _automations.Get<TAutomation>(id);
        }

        public IList<IAutomation> Automations()
        {
            return _automations.GetAll();
        }

        protected virtual void Initialize()
        {
        }

        protected void InitializeHealthMonitor(int pi2GpioPinWithLed)
        {
            var pi2PortController = new Pi2PortController();
            var ledPin = pi2PortController.GetOutput(pi2GpioPinWithLed);

            _healthMonitor = new HealthMonitor(ledPin, Timer, HttpApiController);
        }

        private void InitializeHttpApi()
        {
            _httpServer = new HttpServer();
            var httpRequestDispatcher = new HttpRequestDispatcher(_httpServer);
            HttpApiController = httpRequestDispatcher.GetController("api");

            var appPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "app");
            httpRequestDispatcher.MapFolder("app", appPath);
        }

        private HomeAutomationTimer InitializeTimer()
        {
            var timer = new HomeAutomationTimer(Logger);
            Timer = timer;

            return timer;
        }

        private void InitializeLogging()
        {
            var logger = new Logger.Logger();
            logger.ExposeToApi(HttpApiController);
            logger.Info("Starting...");
            Logger = logger;
        }

        private void InitializeCore()
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                
                InitializeHttpApi();
                InitializeLogging();
                LoadControllerSettings();
                InitializeDiscovery();

                HomeAutomationTimer timer = InitializeTimer();

                TryInitialize();
                LoadNonControllerSettings();

                _httpServer.Start(80);
                ExposeToApi();

                stopwatch.Stop();
                Logger.Info("Startup completed after " + stopwatch.Elapsed);

                timer.Run();
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.ToString());
            }
        }

        private void InitializeDiscovery()
        {
            var discoveryServer = new DiscoveryServer(this);
            discoveryServer.Start();
        }

        private void LoadControllerSettings()
        {
            var settings = new ControllerSettings(StoragePath.WithFilename("Configuration.json"), Logger);
            settings.Load();

            Settings = settings;
        }

        private void TryInitialize()
        {
            try
            {
                Initialize();
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "Error while initializing");
            }
        }

        private void LoadNonControllerSettings()
        {
            foreach (var area in _areas.GetAll())
            {
                area.LoadSettings();
            }

            foreach (var actuator in _actuators.GetAll())
            {
                actuator.LoadSettings();
            }

            foreach (var automation in _automations.GetAll())
            {
                automation.LoadSettings();
            }
        }

        private void ExposeToApi()
        {
            new ControllerApiDispatcher(this).ExposeToApi();

            foreach (var area in _areas.GetAll())
            {
                area.ExposeToApi();
            }

            foreach (var actuator in _actuators.GetAll())
            {
                actuator.ExposeToApi();
            }
        }
    }
}