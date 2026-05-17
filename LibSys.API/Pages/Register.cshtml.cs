using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using System.Net.Http.Headers;

namespace LibSys.API.Pages;

public class RegisterModel : PageModel
{
    private readonly IHttpClientFactory _http;
    public string? ErrorMessage { get; set; }

    public RegisterModel(IHttpClientFactory http) => _http = http;

    public IActionResult OnGet()
    {
        if (HttpContext.Session.GetString("JWTToken") != null)
            return RedirectToPage("/Index");
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string fullName, string email, string password, string? studentId)
    {
        var client = _http.CreateClient("LibSysAPI");
        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            FullName = fullName,
            Email = email,
            Password = password,
            StudentId = studentId ?? ""
        });

        if (!response.IsSuccessStatusCode)
        {
            ErrorMessage = "ไม่สามารถสมัครสมาชิกได้ อีเมลนี้อาจถูกใช้งานแล้ว";
            return Page();
        }

        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        if (result == null) { ErrorMessage = "เกิดข้อผิดพลาด กรุณาลองใหม่"; return Page(); }

        HttpContext.Session.SetString("JWTToken", result.Token);
        HttpContext.Session.SetString("UserName", result.FullName);
        HttpContext.Session.SetString("UserRole", result.Role);

        return RedirectToPage("/Index");
    }

    public record AuthResponse(string Token, string Email, string FullName, string Role, DateTime Expires);
}