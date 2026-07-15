using System.Diagnostics.Metrics;

namespace CommerceHub.ServiceDefaults;

public class BusinessMetrics
{
    private readonly Counter<long> _ordersCreatedCounter;
    private readonly Counter<long> _ordersCompletedCounter;
    private readonly Counter<long> _ordersCancelledCounter;
    private readonly Counter<long> _paymentsProcessedCounter;
    private readonly Counter<long> _paymentsFailedCounter;
    private readonly Counter<long> _userRegistrationsCounter;
    private readonly Counter<long> _userLoginsCounter;
    private readonly Histogram<double> _orderValueHistogram;
    private readonly Histogram<double> _apiResponseTimeHistogram;

    public BusinessMetrics(string serviceName)
    {
        var meter = new Meter($"CommerceHub.{serviceName}", "1.0.0");

        _ordersCreatedCounter = meter.CreateCounter<long>("commercehub.orders.created", "orders");
        _ordersCompletedCounter = meter.CreateCounter<long>("commercehub.orders.completed", "orders");
        _ordersCancelledCounter = meter.CreateCounter<long>("commercehub.orders.cancelled", "orders");
        _paymentsProcessedCounter = meter.CreateCounter<long>("commercehub.payments.processed", "payments");
        _paymentsFailedCounter = meter.CreateCounter<long>("commercehub.payments.failed", "payments");
        _userRegistrationsCounter = meter.CreateCounter<long>("commercehub.users.registrations", "users");
        _userLoginsCounter = meter.CreateCounter<long>("commercehub.users.logins", "users");
        _orderValueHistogram = meter.CreateHistogram<double>("commercehub.order.value", "USD");
        _apiResponseTimeHistogram = meter.CreateHistogram<double>("commercehub.api.response_time", "ms");
    }

    public void RecordOrderCreated() => _ordersCreatedCounter.Add(1);
    public void RecordOrderCompleted() => _ordersCompletedCounter.Add(1);
    public void RecordOrderCancelled() => _ordersCancelledCounter.Add(1);
    public void RecordPaymentProcessed() => _paymentsProcessedCounter.Add(1);
    public void RecordPaymentFailed() => _paymentsFailedCounter.Add(1);
    public void RecordUserRegistration() => _userRegistrationsCounter.Add(1);
    public void RecordUserLogin() => _userLoginsCounter.Add(1);
    public void RecordOrderValue(double value) => _orderValueHistogram.Record(value);
    public void RecordApiResponseTime(double ms) => _apiResponseTimeHistogram.Record(ms);
}
