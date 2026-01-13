using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

namespace Core.ValueObjects;
public class Email
{
    public string Address { get; private set; }

    public Email(string address)
    {
        if (!IsValidEmail(address))
            throw new ValidationException("O e-mail informado é inválido.");
        Address = address;
    }

    public Email()
    {
        Address = string.Empty;
    }

    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var mailAddress = new MailAddress(email);

            if (mailAddress.Address != email)
                return false;

            var domainParts = mailAddress.Host.Split('.');
            if (domainParts.Length < 2)
                return false;

            if (domainParts[^1].Length < 2)
                return false;

            return true;
        }
        catch
        {
            return false;
        }
    }

    public override string ToString() => Address;
}
