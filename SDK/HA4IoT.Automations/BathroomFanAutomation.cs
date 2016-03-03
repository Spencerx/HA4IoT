﻿using System;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Core;

namespace HA4IoT.Automations
{
    public class BathroomFanAutomation : AutomationBase<AutomationSettings>
    {
        private readonly IHomeAutomationTimer _timer;
        private IStateMachine _actuator;
        private TimeSpan _fastDuration;
        private TimeSpan _slowDuration;
        private TimedAction _timeout;

        public BathroomFanAutomation(AutomationId id, IHomeAutomationTimer timer)
            : base(id)
        {
            if (timer == null) throw new ArgumentNullException(nameof(timer));

            _timer = timer;
        }

        public BathroomFanAutomation WithTrigger(IMotionDetector motionDetector)
        {
            if (motionDetector == null) throw new ArgumentNullException(nameof(motionDetector));

            motionDetector.GetMotionDetectedTrigger().Triggered += TurnOn;
            motionDetector.GetDetectionCompletedTrigger().Triggered += StartTimeout;

            return this;
        }

        public BathroomFanAutomation WithActuator(IStateMachine actuator)
        {
            _actuator = actuator;
            return this;
        }

        public BathroomFanAutomation WithSlowDuration(TimeSpan duration)
        {
            _slowDuration = duration;
            return this;
        }

        public BathroomFanAutomation WithFastDuration(TimeSpan duration)
        {
            _fastDuration = duration;
            return this;
        }

        private void StartTimeout(object sender, EventArgs e)
        {
            _timeout = _timer.In(_slowDuration).Do(() =>
            {
                _actuator.SetState("2");
                _timeout = _timer.In(_fastDuration).Do(() => _actuator.TurnOff());
            });
        }

        private void TurnOn(object sender, EventArgs e)
        {
            _timeout?.Cancel();
            _actuator?.SetState("1");
        }
    }
}