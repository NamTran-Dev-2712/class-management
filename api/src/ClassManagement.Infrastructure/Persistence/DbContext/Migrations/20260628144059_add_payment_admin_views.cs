using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassManagement.Infrastructure.Persistence.DbContext.Migrations
{
    /// <inheritdoc />
    public partial class add_payment_admin_views : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // vw_subscriptions — a subscription joined to its teacher + plan for the admin list (A8-01).
            migrationBuilder.Sql(
                """
                CREATE VIEW vw_subscriptions AS
                SELECT
                    s.id,
                    s.public_id,
                    s.teacher_id,
                    u.public_id      AS teacher_public_id,
                    u.display_name   AS teacher_name,
                    u.email          AS teacher_email,
                    p.name           AS plan_name,
                    p.price_vnd      AS plan_price_vnd,
                    p.billing_cycle  AS billing_cycle,
                    s.status,
                    s.started_at,
                    s.expires_at,
                    s.cancelled_at,
                    s.grace_period_ends_at,
                    s.payment_type,
                    s.admin_note,
                    s.created_at,
                    s.updated_at
                FROM subscriptions s
                JOIN users u ON u.id = s.teacher_id
                JOIN plans p ON p.id = s.plan_id;
                """
            );

            // vw_payments — a payment joined to its teacher + plan for the admin payment history (A8-03).
            migrationBuilder.Sql(
                """
                CREATE VIEW vw_payments AS
                SELECT
                    pay.id,
                    pay.public_id,
                    pay.teacher_id,
                    u.public_id    AS teacher_public_id,
                    u.display_name AS teacher_name,
                    u.email        AS teacher_email,
                    p.name         AS plan_name,
                    pay.provider,
                    pay.billing_cycle,
                    pay.amount_vnd,
                    pay.status,
                    pay.provider_transaction_id,
                    pay.created_at,
                    pay.completed_at
                FROM payments pay
                JOIN users u ON u.id = pay.teacher_id
                JOIN plans p ON p.id = pay.plan_id;
                """
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS vw_payments;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS vw_subscriptions;");
        }
    }
}
