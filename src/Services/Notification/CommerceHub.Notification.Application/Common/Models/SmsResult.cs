namespace CommerceHub.Notification.Application.Common.Models;

public class SmsResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }

    public static SmsResult Succeeded(string? message = null) => new() { Success = true, Message = message };
    public static SmsResult Failed(string message) => new() { Success = false, Message = message };
}
