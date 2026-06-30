using ClassManagement.Application.Exceptions;
using ClassManagement.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace ClassManagement.Infrastructure.Services.Payment;

// Selects the gateway adapter. When Payment:UseFakeProvider is on (dev/test) every provider resolves to
// the in-process simulator so the flow runs offline; in production it maps the enum to the real adapter.
public sealed class PaymentProviderResolver : IPaymentProviderResolver
{
    private readonly PaymentOptions _options;
    private readonly MomoPaymentProvider _momo;
    private readonly VnPayPaymentProvider _vnpay;
    private readonly FakePaymentProvider _fake;

    public PaymentProviderResolver(
        IOptions<PaymentOptions> options,
        MomoPaymentProvider momo,
        VnPayPaymentProvider vnpay,
        FakePaymentProvider fake
    )
    {
        _options = options.Value;
        _momo = momo;
        _vnpay = vnpay;
        _fake = fake;
    }

    public IPaymentProvider Resolve(PaymentProvider provider)
    {
        if (_options.UseFakeProvider)
            return _fake;

        return provider switch
        {
            PaymentProvider.Momo => _momo,
            PaymentProvider.VnPay => _vnpay,
            _ => throw new BadException("Payment.ProviderNotSupported"),
        };
    }
}
