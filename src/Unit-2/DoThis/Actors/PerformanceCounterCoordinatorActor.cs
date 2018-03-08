using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;
using Akka.Actor;

namespace ChartApp.Actors
{
    public class PerformanceCounterCoordinatorActor : ReceiveActor
    {
        #region Message Types
        public class Watch
        {
            public CounterType Counter { get; private set; }

            public Watch(CounterType counter)
            {
                Counter = counter;
            }
        }

        public class Unwatch
        {
            public CounterType Counter { get; private set; }

            public Unwatch(CounterType counter)
            {
                Counter = counter;
            }
        }
        #endregion

        private Dictionary<CounterType, IActorRef> _counterActors;

        private IActorRef _chartingActor;

        private static readonly Dictionary<CounterType, Func<PerformanceCounter>> CounterGenerators = 
            new Dictionary<CounterType, Func<PerformanceCounter>>()
            {
                {CounterType.Cpu, () => new PerformanceCounter(
                    "Processor",
                    "% Processor Time",
                    "_Total",
                    true)},
                {CounterType.Memory, () => new PerformanceCounter(
                    "Memory",
                    "% Commited Bytes In Use",
                    true)},
                {CounterType.Disk, () => new PerformanceCounter(
                    "LogicalDisk",
                    "% Disk Time",
                    "_Total",
                    true)}
            };

        private static readonly Dictionary<CounterType, Func<Series>> CounterSeries =
            new Dictionary<CounterType, Func<Series>>()
            {
                {CounterType.Cpu, () => new Series(CounterType.Cpu.ToString()){
                    ChartType = SeriesChartType.SplineArea,
                    Color = Color.DarkGreen
                }},
                {CounterType.Memory, () => new Series(CounterType.Memory.ToString())
                {
                    ChartType = SeriesChartType.FastLine,
                    Color = Color.MediumBlue
                }},
                {CounterType.Disk, () => new Series(CounterType.Disk.ToString())
                {
                    ChartType = SeriesChartType.SplineArea,
                    Color = Color.DarkRed
                }}
            };

        public PerformanceCounterCoordinatorActor(IActorRef chartingActor) :
            this(chartingActor, new Dictionary<CounterType, IActorRef>())
        { }

        public PerformanceCounterCoordinatorActor(IActorRef chartingActor,
            Dictionary<CounterType, IActorRef> counterActors)
        {
            _chartingActor = chartingActor;
            _counterActors = counterActors;

            Receive<Watch>(watch =>
            {
                if (!_counterActors.ContainsKey(watch.Counter))
                {
                    var counterActor = Context.ActorOf(Props.Create<PerformanceCounterActor>(
                        watch.Counter.ToString(), CounterGenerators[watch.Counter]));
                }
            });

            Receive<Unwatch>(unwatch =>
            {
                if (!_counterActors.ContainsKey(unwatch.Counter))
                {
                    return;
                }

                _counterActors[unwatch.Counter].Tell(new UnsubscribeCounter(
                    _chartingActor, unwatch.Counter));

                _chartingActor.Tell(new ChartingActor.RemoveSeries(
                    unwatch.Counter.ToString()));
            });
        }
    }
}
