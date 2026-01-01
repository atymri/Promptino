namespace Promptino.Core.DTOs;

public record EmailModel(
    string To, string Subject, string Body);

public record HtmlConvertorModel(
    string FileName, Dictionary<string, string> Values
    );