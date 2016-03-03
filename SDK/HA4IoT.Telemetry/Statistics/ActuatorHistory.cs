﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Data.Json;
using Windows.Storage;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Networking;
using HA4IoT.Networking;
using DayOfWeek = System.DayOfWeek;

namespace HA4IoT.Telemetry.Statistics
{
    public class ActuatorHistory : IStatusProvider
    {
        private readonly IActuator _actuator;
        private readonly ILogger _logger;
        private readonly object _syncRoot = new object();

        private readonly List<ActuatorHistoryEntry> _entriesOfThisMonth = new List<ActuatorHistoryEntry>();
        private readonly string _filename;

        public ActuatorHistory(IActuator actuator, IHttpRequestController apiRequestController, ILogger logger)
        {
            _actuator = actuator;
            _logger = logger;
            _filename = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Actuators", actuator.Id.Value, "History.csv");

            apiRequestController.HandleGet($"statistics/{actuator.Id}").Using(HandleApiGet);
        }

        public void AddEntry(ActuatorHistoryEntry entry)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));

            lock (_syncRoot)
            {
                if (entry.Timestamp.Month != DateTime.Now.Month)
                {
                    _entriesOfThisMonth.Clear();
                    _logger.Verbose("Cleared rolling history due to month change.");
                }

                _entriesOfThisMonth.Add(entry);

                string directory = Path.GetDirectoryName(_filename);
                if (!Directory.Exists(directory))
                {
                    _logger.Verbose("Creating directory... " + directory);

                    Directory.CreateDirectory(directory);
                }

                File.AppendAllText(_filename, entry.ToCsv());
            }
        }


        public JsonObject ExportStatusToJsonObject()
        {
            var entries = new List<ActuatorHistoryEntry>();
            lock (_entriesOfThisMonth)
            {
                entries.AddRange(_entriesOfThisMonth);
            }

            DateTime now = DateTime.Now;
            //entries.Add(new ActuatorHistoryEntry(now, _actuator.Id, _actuator.));

            int firstDayOfWeek = GetFirstDayOfWeek(now);

            var entriesOfThisMonth = entries.Where(e => e.Timestamp.Year == now.Year && e.Timestamp.Month == now.Month).ToList();
            var entriesOfThisWeek = entriesOfThisMonth.Where(e => e.Timestamp.Day >= firstDayOfWeek).ToList();
            var entriesOfThisDay = entriesOfThisWeek.Where(e => e.Timestamp.Day == now.Day).ToList();

            var status = new JsonObject();
            status.SetNamedValue("actuator", _actuator.Id.ExportToJsonObject());
            status.SetNamedValue("durationsOfThisMonth", GetStateDurations(entriesOfThisMonth).ToIndexedJsonObject());
            status.SetNamedValue("durationsOfThisWeek", GetStateDurations(entriesOfThisWeek).ToIndexedJsonObject());
            status.SetNamedValue("durationsOfThisDay", GetStateDurations(entriesOfThisDay).ToIndexedJsonObject());
            return status;
        }

        private int GetFirstDayOfWeek(DateTime timestamp)
        {
            int passedDays = 0;
            switch (timestamp.DayOfWeek)
            {
                case DayOfWeek.Tuesday:
                    {
                        passedDays = 1;
                        break;
                    }

                case DayOfWeek.Wednesday:
                    {
                        passedDays = 2;
                        break;
                    }

                case DayOfWeek.Thursday:
                    {
                        passedDays = 3;
                        break;
                    }

                case DayOfWeek.Friday:
                    {
                        passedDays = 4;
                        break;
                    }

                case DayOfWeek.Saturday:
                    {
                        passedDays = 5;
                        break;
                    }

                case DayOfWeek.Sunday:
                    {
                        passedDays = 6;
                        break;
                    }
            }

            var firstDayOfWeek = timestamp.Subtract(TimeSpan.FromDays(passedDays));
            if (firstDayOfWeek.Month != timestamp.Month)
            {
                return 1;
            }

            return firstDayOfWeek.Day;
        }

        private Dictionary<string, TimeSpan> GetStateDurations(List<ActuatorHistoryEntry> historyEntries)
        {
            var durations = new Dictionary<string, TimeSpan>();
            ActuatorHistoryEntry previousEntry = null;

            foreach (var historyEntry in historyEntries.OrderBy(e => e.Timestamp))
            {
                if (previousEntry == null)
                {
                    previousEntry = historyEntry;
                    continue;
                }

                ////if (historyEntry.OldState != previousEntry.NewState)
                ////{
                ////    _notificationHandler.Warning("Detected wrong states between history entries of actuator '" + _actuator.Id + "'.");
                ////}

                TimeSpan durationOfState = historyEntry.Timestamp - previousEntry.Timestamp;

                TimeSpan totalDuration;
                if (!durations.TryGetValue(previousEntry.NewState, out totalDuration))
                {
                    durations.Add(previousEntry.NewState, durationOfState);
                }
                else
                {
                    durations[previousEntry.NewState] = totalDuration + durationOfState;
                }

                previousEntry = historyEntry;
            }

            return durations;
        }

        private void HandleApiGet(HttpContext httpContext)
        {
            httpContext.Response.Body = new JsonBody(ExportStatusToJsonObject());
        }
    }
}