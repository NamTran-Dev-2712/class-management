using ClassManagement.Domain.Modules.Admin.Constants;
using ClassManagement.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace ClassManagement.Infrastructure.Services.Payment;

// Live subscription/payment tunables: system_settings is the source of truth (MVP-7 style), with the
// SubscriptionOptions/PaymentOptions appsettings values as fallback. Scoped (depends on a scoped cache).
public sealed class SubscriptionPolicy : ISubscriptionPolicy
{
    private readonly SubscriptionOptions _subscription;
    private readonly PaymentOptions _payment;
    private readonly ISystemSettingsService _settings;

    public SubscriptionPolicy(
        IOptions<SubscriptionOptions> subscription,
        IOptions<PaymentOptions> payment,
        ISystemSettingsService settings
    )
    {
        _subscription = subscription.Value;
        _payment = payment.Value;
        _settings = settings;
    }

    public Task<int> GetGracePeriodDaysAsync(CancellationToken cancellationToken = default) =>
        _settings.GetIntAsync(
            SystemSettingKeys.SubscriptionGracePeriodDays,
            _subscription.GracePeriodDays,
            cancellationToken
        );

    public Task<int> GetExpiringNoticeDaysAsync(CancellationToken cancellationToken = default) =>
        _settings.GetIntAsync(
            SystemSettingKeys.SubscriptionExpiringNoticeDays,
            _subscription.ExpiringNoticeDays,
            cancellationToken
        );

    public Task<int> GetOrderTimeoutMinutesAsync(CancellationToken cancellationToken = default) =>
        _settings.GetIntAsync(
            SystemSettingKeys.PaymentOrderTimeoutMinutes,
            _payment.OrderTimeoutMinutes,
            cancellationToken
        );
}
