namespace CommerceHub.Modules.Notification.Application.Common.Models;

public class EmailResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }

    public static EmailResult Succeeded(string? message = null) => new() { Success = true, Message = message };
    public static EmailResult Failed(string message) => new() { Success = false, Message = message };
}
