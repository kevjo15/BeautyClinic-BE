using System.Net;
using System.Text;

namespace Infrastructure_Layer.Notifications
{
    public static class EmailTemplateBuilder
    {
        public static (string Html, string PlainText) BuildBookingTemplate(
            string title,
            string message,
            string? brandName,
            string? logoUrl,
            string? supportEmail,
            string? appBaseUrl)
        {
            var safeTitle = WebUtility.HtmlEncode(title);
            var safeMessage = WebUtility.HtmlEncode(message).Replace("\n", "<br />");
            var safeBrand = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(brandName) ? "Bokning" : brandName);

            var logoBlock = string.IsNullOrWhiteSpace(logoUrl)
                ? ""
                : $"<img src=\"{WebUtility.HtmlEncode(logoUrl)}\" alt=\"{safeBrand}\" style=\"max-height:48px;\" />";

            var supportBlock = string.IsNullOrWhiteSpace(supportEmail)
                ? ""
                : $"<p style=\"margin:16px 0 0;font-size:14px;color:#6b7280;\">Kontakt: <a href=\"mailto:{WebUtility.HtmlEncode(supportEmail)}\">{WebUtility.HtmlEncode(supportEmail)}</a></p>";

            var appLinkBlock = string.IsNullOrWhiteSpace(appBaseUrl)
                ? ""
                : $"<p style=\"margin:12px 0 0;\"><a href=\"{WebUtility.HtmlEncode(appBaseUrl)}\" style=\"color:#2563eb;text-decoration:none;\">Visa bokning</a></p>";

            var html = $@"
<!DOCTYPE html>
<html>
  <head>
    <meta charset=""utf-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1"" />
    <title>{safeTitle}</title>
  </head>
  <body style=""margin:0;padding:0;background:#f5f7fb;font-family:Arial, Helvetica, sans-serif;color:#111827;"">
    <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#f5f7fb;padding:24px 0;"">
      <tr>
        <td align=""center"">
          <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""max-width:560px;background:#ffffff;border-radius:12px;box-shadow:0 2px 8px rgba(17,24,39,0.08);overflow:hidden;"">
            <tr>
              <td style=""padding:24px 28px 16px;"">
                {logoBlock}
                <h1 style=""margin:16px 0 8px;font-size:22px;color:#0f172a;"">{safeTitle}</h1>
                <p style=""margin:0;font-size:16px;line-height:1.6;color:#111827;"">{safeMessage}</p>
                {appLinkBlock}
                {supportBlock}
              </td>
            </tr>
            <tr>
              <td style=""padding:16px 28px 24px;font-size:12px;color:#94a3b8;"">
                Du får detta mail eftersom du har gjort en bokning hos {safeBrand}.
              </td>
            </tr>
          </table>
        </td>
      </tr>
    </table>
  </body>
</html>";

            var plainText = new StringBuilder()
                .AppendLine(title)
                .AppendLine()
                .AppendLine(message)
                .AppendLine()
                .AppendLine(string.IsNullOrWhiteSpace(appBaseUrl) ? "" : $"Visa bokning: {appBaseUrl}")
                .AppendLine(string.IsNullOrWhiteSpace(supportEmail) ? "" : $"Kontakt: {supportEmail}")
                .ToString()
                .Trim();

            return (html, plainText);
        }

        /// <summary>
        /// Generell mall med tydlig call-to-action-knapp (t.ex. lösenordsåterställning).
        /// </summary>
        public static (string Html, string PlainText) BuildActionTemplate(
            string title,
            string message,
            string actionText,
            string actionUrl,
            string? brandName,
            string? supportEmail,
            string? footerNote)
        {
            var safeTitle = WebUtility.HtmlEncode(title);
            var safeMessage = WebUtility.HtmlEncode(message).Replace("\n", "<br />");
            var safeBrand = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(brandName) ? "ElsaBeauty" : brandName);
            var safeActionText = WebUtility.HtmlEncode(actionText);
            var safeActionUrl = WebUtility.HtmlEncode(actionUrl);

            var supportBlock = string.IsNullOrWhiteSpace(supportEmail)
                ? ""
                : $"<p style=\"margin:20px 0 0;font-size:14px;color:#6b7280;\">Frågor? Kontakta oss på <a href=\"mailto:{WebUtility.HtmlEncode(supportEmail)}\" style=\"color:#a34a63;\">{WebUtility.HtmlEncode(supportEmail)}</a></p>";

            var footerBlock = string.IsNullOrWhiteSpace(footerNote) ? "" : footerNote;

            var html = $@"
<!DOCTYPE html>
<html>
  <head>
    <meta charset=""utf-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1"" />
    <title>{safeTitle}</title>
  </head>
  <body style=""margin:0;padding:0;background:#faf4f1;font-family:Arial, Helvetica, sans-serif;color:#111827;"">
    <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#faf4f1;padding:24px 0;"">
      <tr>
        <td align=""center"">
          <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""max-width:560px;background:#ffffff;border-radius:12px;box-shadow:0 2px 8px rgba(17,24,39,0.08);overflow:hidden;"">
            <tr>
              <td style=""padding:28px 28px 8px;"">
                <p style=""margin:0;font-size:20px;font-weight:bold;color:#a34a63;"">{safeBrand}</p>
              </td>
            </tr>
            <tr>
              <td style=""padding:8px 28px 16px;"">
                <h1 style=""margin:8px 0;font-size:22px;color:#0f172a;"">{safeTitle}</h1>
                <p style=""margin:0;font-size:16px;line-height:1.6;color:#111827;"">{safeMessage}</p>
                <p style=""margin:24px 0 8px;"">
                  <a href=""{safeActionUrl}""
                     style=""display:inline-block;background:#a34a63;color:#ffffff;text-decoration:none;padding:12px 28px;border-radius:8px;font-size:16px;font-weight:bold;"">
                    {safeActionText}
                  </a>
                </p>
                <p style=""margin:12px 0 0;font-size:13px;color:#6b7280;"">Fungerar inte knappen? Kopiera länken:<br /><a href=""{safeActionUrl}"" style=""color:#a34a63;word-break:break-all;"">{safeActionUrl}</a></p>
                {supportBlock}
              </td>
            </tr>
            <tr>
              <td style=""padding:16px 28px 24px;font-size:12px;color:#94a3b8;"">
                {footerBlock}
              </td>
            </tr>
          </table>
        </td>
      </tr>
    </table>
  </body>
</html>";

            var plainText = new StringBuilder()
                .AppendLine(title)
                .AppendLine()
                .AppendLine(message)
                .AppendLine()
                .AppendLine($"{actionText}: {actionUrl}")
                .AppendLine(string.IsNullOrWhiteSpace(supportEmail) ? "" : $"Kontakt: {supportEmail}")
                .ToString()
                .Trim();

            return (html, plainText);
        }
    }
}
