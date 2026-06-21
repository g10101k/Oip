<#macro emailLayout>
<!DOCTYPE html>
<html lang="${(locale.currentLanguageTag)!'en'}">
<head>
  <meta charset="utf-8">
  <meta content="width=device-width, initial-scale=1" name="viewport">
  <title>${realmName!'OIP'}</title>
</head>
<body style="margin:0;padding:0;background:#f4f4f5;color:#3f3f46;font-family:Inter,Arial,sans-serif;">
  <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="background:#f4f4f5;margin:0;padding:32px 16px;">
    <tr>
      <td align="center">
        <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="max-width:600px;background:#ffffff;border:1px solid #e5e7eb;border-radius:24px;box-shadow:0 24px 70px rgba(15,23,42,0.12);overflow:hidden;">
          <tr>
            <td style="padding:40px 40px 24px;text-align:center;">
              <div style="display:inline-block;color:#ef4444;font-size:28px;font-weight:700;letter-spacing:0;line-height:1;">OIP</div>
            </td>
          </tr>
          <tr>
            <td style="padding:0 40px 40px;color:#3f3f46;font-size:16px;line-height:1.6;">
              <#nested>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>
</#macro>
