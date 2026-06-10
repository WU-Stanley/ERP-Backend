namespace WUIAM.Services;

public class EmailTemplateService
{
  public static string GeneratePasswordResetSuccessEmailHtml(string fullName)
  {
    return $@"
<!DOCTYPE html>
<html>
<head>
  <meta charset='UTF-8'>
  <title>Password Reset Successful</title>
</head>
<body style='font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0;'>
  <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f4f4f4; padding: 30px;'>
    <tr>
      <td align='center'>
        <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 10px rgba(0,0,0,0.1);'>
          <tr>
            <td style='background-color: #1DCB51 ;padding: 20px 30px; color: #ffffff; font-size: 24px; font-weight: bold;'>
              Password Reset Successful 🔐
            </td>
          </tr>
          <tr>
            <td style='padding: 30px; color: #333333;'>
              <p style='font-size: 16px;'>Hello <strong>{fullName}</strong>,</p>
              <p style='font-size: 16px;'>Your password has been <strong>reset successfully</strong>. You can now log in with your new password.</p>
              <p style='font-size: 15px;'>If you did not request this change, please contact our support team immediately.</p>
              <p style='font-size: 15px;'>Thank you for using https://erp.uat.wigweuniversity.edu.ng!</p>

              <p style='font-size: 15px;'>Best regards,<br>
              <strong>WU IT Support Team</strong><br>
              <a href='mailto:teamit@wigweuniversity.edu.ng' style='color: #1a73e8;'>teamit@wigweuniversity.edu.ng</a><br>
              <a href='https://erp.uat.wigweuniversity.edu.ng' target='_blank' style='color: #1a73e8;'>https://erp.uat.wigweuniversity.edu.ng</a></p>
            </td>
          </tr>
          <tr>
            <td style='background-color: #f1f1f1; padding: 20px; text-align: center; font-size: 13px; color: #888;'>
              © 2025 WU. All rights reserved.
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
  }
  public static string GenerateWelcomeEmailHtml(string fullName, string email, string userName, string password)
  {
    return $@"
<!DOCTYPE html>
<html>
<head>
  <meta charset='UTF-8'>
  <title>Welcome to WuERP</title>
</head>
<body style='font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0;'>
  <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f4f4f4; padding: 30px;'>
    <tr>
      <td align='center'>
        <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 10px rgba(0,0,0,0.1);'>
          <tr>
            <td style='background-color: #1DCB51 ;padding: 20px 30px; color: #ffffff; font-size: 24px; font-weight: bold;'>
              Welcome to WuERP 🎉
            </td>
          </tr>
          <tr>
            <td style='padding: 30px; color: #333333;'>
              <p style='font-size: 16px;'>Dear <strong>{fullName}</strong>,</p>
              <p style='font-size: 16px;'>Your account has been successfully created on <strong>https://erp.uat.wigweuniversity.edu.ng</strong>. You can now log in and start using the platform.</p>

              <h3 style='color: #1a73e8;'>🔐 Login Credentials</h3>
              <table style='font-size: 15px; margin-bottom: 20px;'>
                <tr>
                  <td><strong>Username:</strong></td>
                  <td>{userName}</td>
                </tr>
                <tr>
                  <td><strong>Email:</strong></td>
                  <td>{email}</td>
                </tr>
                <tr>
                  <td><strong>Temporary Password:</strong></td>
                  <td>{password}</td>
                </tr>
              </table>

              <p style='font-size: 15px;'><strong>Note:</strong> You can log in using either your <strong>email</strong> or <strong>username</strong> along with your password.</p>

              <p style='font-size: 15px; color: #d93025;'><strong>⚠️ Please change your password immediately after login for security reasons.</strong></p>

              <p style='font-size: 15px;'>If you need any assistance, feel free to reach out to our support team.</p>

              <p style='font-size: 15px;'>Thank you and welcome aboard!</p>

              <p style='font-size: 15px;'>Warm regards,<br>
              <strong>WU IT Support Team</strong><br>
              <a href='mailto:teamit@wigweuniversity.edu.ng' style='color: #1a73e8;'>teamit@wigweuniversity.edu.ng</a><br>
              <a href='https://erp.uat.wigweuniversity.edu.ng' target='_blank' style='color: #1a73e8;'>https://erp.uat.wigweuniversity.edu.ng</a></p>
            </td>
          </tr>
          <tr>
            <td style='background-color: #f1f1f1; padding: 20px; text-align: center; font-size: 13px; color: #888;'>
              © 2025 WU. All rights reserved.
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
  }

  public static string GenerateTwoFactorTokenEmailHtml(string fullName, string twoFactorToken)
  {
    return $@"
<!DOCTYPE html>
<html>
<head>
  <meta charset='UTF-8'>
  <title>Your Two-Factor Authentication Token</title>
</head>
<body style='font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0;'>
  <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f4f4f4; padding: 30px;'>
    <tr>
      <td align='center'>
        <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 10px rgba(0,0,0,0.1);'>
          <tr>
            <td style='background-color: #1DCB51; padding: 20px 30px; color: #ffffff; font-size: 24px; font-weight: bold;'>
              Two-Factor Authentication 🔐
            </td>
          </tr>
          <tr>
            <td style='padding: 30px; color: #333333;'>
              <p style='font-size: 16px;'>Hello <strong>{fullName}</strong>,</p>
              <p style='font-size: 16px;'>Your two-factor authentication (2FA) token is:</p>

              <div style='background-color: #f0f4ff; padding: 15px; font-size: 20px; font-weight: bold; color: #1a73e8; text-align: center; margin: 20px 0; border-radius: 6px;'>
                {twoFactorToken}
              </div>

              <p style='font-size: 15px;'>Please enter this token to complete your login. This code will expire shortly for security reasons.</p>

              <p style='font-size: 15px;'>If you didn’t request this, please secure your account or contact support immediately.</p>

              <p style='font-size: 15px;'>Thank you,<br>
            <strong>WU IT Support Team</strong><br>
              <a href='mailto:teamit@wigweuniversity.edu.ng</a><br>
              <a href='https://erp.uat.wigweuniversity.edu.ng' target='_blank' style='color: #1a73e8;'>erp.uat.wigweuniversity.edu.ng</a></p>
             </td>
          </tr>
          <tr>
            <td style='background-color: #f1f1f1; padding: 20px; text-align: center; font-size: 13px; color: #888;'>
              © 2025 WU. All rights reserved.
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
  }
  public static string GenerateLoginNotificationEmailHtml(string fullName, DateTime loginTime)
  {
    return $@"
<!DOCTYPE html>
<html>
<head>
  <meta charset='UTF-8'>
  <title>Login Notification</title>
</head>
<body style='font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0;'>
  <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f4f4f4; padding: 30px;'>
    <tr>
      <td align='center'>
        <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 10px rgba(0,0,0,0.1);'>
          <tr>
            <td style='background-color: #1DCB51; padding: 20px 30px; color: #ffffff; font-size: 24px; font-weight: bold;'>
              Login Notification ✅
            </td>
          </tr>
          <tr>
            <td style='padding: 30px; color: #333333;'>
              <p style='font-size: 16px;'>Hello <strong>{fullName}</strong>,</p>
              <p style='font-size: 16px;'>You have successfully logged in to <strong>https://erp.uat.wigweuniversity.edu.ng</strong> on:</p>

              <div style='background-color: #f0f4ff; padding: 12px; font-size: 16px; font-weight: bold; color: #1a73e8; text-align: center; border-radius: 6px; margin: 15px 0;'>
                {loginTime.ToString("f")}
              </div>

              <p style='font-size: 15px;'>If this wasn’t you, please report immediately to the ICT unit for investigation and assistance.</p>

              <p style='font-size: 15px;'>Thank you for using https://erp.uat.wigweuniversity.edu.ng.</p>

              <p style='font-size: 15px;'>Best regards,<br>
              <strong>WU IT Support Team</strong><br>
              <a href='mailto:teamit@wigweuniversity.edu.ng</a><br>
              <a href='https://erp.uat.wigweuniversity.edu.ng' target='_blank' style='color: #1a73e8;'>https://erp.uat.wigweuniversity.edu.ng</a></p>
            </td>
          </tr>
          <tr>
            <td style='background-color: #f1f1f1; padding: 20px; text-align: center; font-size: 13px; color: #888;'>
              © 2025 https://erp.aut.wigweuniversity.edu.ng. All rights reserved.
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
  }
  public static string GeneratePasswordResetTokenEmailHtml(string fullName, string resetToken)
  {
    return $@"
<!DOCTYPE html>
<html>
<head>
  <meta charset='UTF-8'>
  <title>Password Reset Request</title>
</head>
<body style='font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0;'>
  <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f4f4f4; padding: 30px;'>
    <tr>
      <td align='center'>
        <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 10px rgba(0,0,0,0.1);'>
          <tr>
            <td style='background-color: #1DCB51; padding: 20px 30px; color: #ffffff; font-size: 24px; font-weight: bold;'>
              Password Reset Request 🔄
            </td>
          </tr>
          <tr>
            <td style='padding: 30px; color: #333333;'>
              <p style='font-size: 16px;'>Hello <strong>{fullName}</strong>,</p>
              <p style='font-size: 16px;'>You have requested to reset your password. Use the following token to proceed:</p>

              <div style='background-color: #f0f4ff; padding: 15px; font-size: 20px; font-weight: bold; color: #1a73e8; text-align: center; margin: 20px 0; border-radius: 6px;'>
                {resetToken}
              </div>

              <p style='font-size: 15px;'>If you did not request this, please ignore this email or contact support immediately to secure your account.</p>

              <p style='font-size: 15px;'>Thank you,<br>
              <strong>WU IT Support Team</strong><br>
              <a href='mailto:teamit@wigweuniversity.edu.ng</a><br>
              <a href='https://erp.uat.wigweuniversity.edu.ng' target='_blank' style='color: #1a73e8;'>erp.wigweuniversity.edu.ng</a></p>
            </td>
          </tr>
          <tr>
            <td style='background-color: #f1f1f1; padding: 20px; text-align: center; font-size: 13px; color: #888;'>
              © 2025 https://erp.uat.wigweuniversity.edu.ng. All rights reserved.
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
  }
}