namespace RefactoringLegacyCode.Features;

public interface ICustomerEmailSender
{
    public void SendEmail(StringContent content);
}