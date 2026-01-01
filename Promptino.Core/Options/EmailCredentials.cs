namespace Promptino.Core.Options;

internal class EmailCredentials
{
    public string MailAddress { get; set; }
    public string MailTitle { get; set; }
    public string StmlHost { get; set; }
    public int StmpPort { get; set; }
    public string StmpPassword { get; set; }
}
