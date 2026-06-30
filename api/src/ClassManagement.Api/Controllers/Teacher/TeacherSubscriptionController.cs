using ClassManagement.Application.Common.Constants;
using ClassManagement.Application.Interfaces.Localization;
using ClassManagement.Application.Modules.Payment.Interfaces;
using ClassManagement.Infrastructure.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;

namespace ClassManagement.Api.Controllers.Teacher;

// A teacher's own premium surface (MVP-8): resource usage now; subscription status, checkout, cancel and
// reactivate are added in later slices.
[ApiController]
[Route("api/teacher/subscription")]
[Authorize(Roles = ApplicationRoles.Teacher)]
public class TeacherSubscriptionController : BaseApiController
{
    private readonly ISender _mediator;
    private readonly IInvoicePdfService _invoicePdf;
    private readonly ILocalizationService _localizer;

    public TeacherSubscriptionController(
        ISender mediator,
        IInvoicePdfService invoicePdf,
        ILocalizationService localizer
    )
    {
        _mediator = mediator;
        _invoicePdf = invoicePdf;
        _localizer = localizer;
    }

    [HttpGet]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    [OutputCache(PolicyName = OutputCachePolicies.TeacherSubscriptionRead)]
    public async Task<IActionResult> GetMySubscription(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMySubscriptionQuery(), cancellationToken);
        return ApiOk(result);
    }

    [HttpGet("usage")]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    [OutputCache(PolicyName = OutputCachePolicies.SubscriptionUsageRead)]
    public async Task<IActionResult> GetUsage(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSubscriptionUsageQuery(), cancellationToken);
        return ApiOk(result);
    }

    [HttpPost("cancel")]
    [EnableRateLimiting(RateLimitOptions.Policies.PaymentCheckout)]
    public async Task<IActionResult> Cancel(CancellationToken cancellationToken)
    {
        await _mediator.Send(new CancelSubscriptionCommand(), cancellationToken);
        await EvictCacheAsync(OutputCacheTags.Subscriptions, cancellationToken);
        return ApiOk("Subscription.Cancelled");
    }

    [HttpPost("reactivate")]
    [EnableRateLimiting(RateLimitOptions.Policies.PaymentCheckout)]
    public async Task<IActionResult> Reactivate(CancellationToken cancellationToken)
    {
        await _mediator.Send(new ReactivateSubscriptionCommand(), cancellationToken);
        await EvictCacheAsync(OutputCacheTags.Subscriptions, cancellationToken);
        return ApiOk("Subscription.Reactivated");
    }

    // Start a checkout: creates a Pending payment order and returns the gateway redirect/QR.
    [HttpPost("checkout")]
    [EnableRateLimiting(RateLimitOptions.Policies.PaymentCheckout)]
    public async Task<IActionResult> Checkout(
        CreatePaymentIntentCommand command,
        CancellationToken cancellationToken
    )
    {
        var result = await _mediator.Send(command, cancellationToken);
        return ApiOk(result, "Payment.CheckoutStarted");
    }

    // Polling fallback for the checkout page while waiting for the provider webhook.
    [HttpGet("payments/{paymentId:guid}/status")]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    public async Task<IActionResult> GetPaymentStatus(
        Guid paymentId,
        CancellationToken cancellationToken
    )
    {
        var result = await _mediator.Send(new GetPaymentStatusQuery(paymentId), cancellationToken);
        return ApiOk(result);
    }

    // Invoice history (T8-07).
    [HttpGet("invoices")]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    [OutputCache(PolicyName = OutputCachePolicies.InvoicesRead)]
    public async Task<IActionResult> GetInvoices(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMyInvoicesQuery(), cancellationToken);
        return ApiOk(result);
    }

    // Download an invoice as a PDF (T8-03). Labels are resolved here so the renderer stays i18n-free.
    [HttpGet("invoices/{invoiceId:guid}/pdf")]
    [EnableRateLimiting(RateLimitOptions.Policies.Export)]
    public async Task<IActionResult> DownloadInvoice(
        Guid invoiceId,
        CancellationToken cancellationToken
    )
    {
        var doc = await _mediator.Send(
            new GetInvoiceForDownloadQuery(invoiceId),
            cancellationToken
        );
        var labels = new InvoicePdfLabels(
            _localizer["Invoice.Pdf.Title"],
            _localizer["Invoice.Pdf.Issuer"],
            _localizer["Invoice.Pdf.InvoiceNumber"],
            _localizer["Invoice.Pdf.IssuedAt"],
            _localizer["Invoice.Pdf.BilledTo"],
            _localizer["Invoice.Pdf.Plan"],
            _localizer["Invoice.Pdf.BillingCycle"],
            _localizer["Invoice.Pdf.Amount"],
            _localizer["Invoice.Pdf.Total"],
            _localizer["Invoice.Pdf.PaidNote"]
        );
        var bytes = _invoicePdf.Render(doc, labels);
        return File(bytes, "application/pdf", $"{doc.InvoiceNumber}.pdf");
    }
}
