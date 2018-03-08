using Akka.Actor;

namespace ChartApp.Actors
{
    #region Reporting
    
    public class GatherMetrics { }

    public class Metric
    {
        public string Series { get; private set; }
        public float CounterValue { get; private set; }

        public Metric(string series, float counterValue)
        {
            Series = series;
            CounterValue = counterValue;
        }
    }
    #endregion

    #region Performance Counter Management
    public enum CounterType
    {
        Cpu, Memory, Disk
    }

    public class SubscribeCounter
    {
        public IActorRef Subscriber { get; private set; }
        public CounterType Counter { get; private set; }

        public SubscribeCounter(CounterType counter, IActorRef subscriber)
        {
            Subscriber = subscriber;
            Counter = counter;
        }
    }

    public class UnsubscribeCounter
    {
        public IActorRef Subscriber { get; private set; }
        public CounterType Counter { get; private set; }

        public UnsubscribeCounter(IActorRef subscriber, CounterType counter)
        {
            Subscriber = subscriber;
            Counter = counter;
        }
    }
    #endregion

}
