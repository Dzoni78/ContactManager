using System.Net.Mail;
using System.Text.RegularExpressions;

namespace ContactManager.Helpers;

public static class ValidationHelper
{
    private static readonly Regex PhoneRegex = new(@"^[0-9+()\-\s./]*$", RegexOptions.Compiled);

    public static bool IsValidEmail(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        try
        {
            return new MailAddress(value).Address == value.Trim();
        }
        catch
        {
            return false;
        }
    }

    public static bool IsValidPhone(string value) =>
        string.IsNullOrWhiteSpace(value) || PhoneRegex.IsMatch(value.Trim());
}
