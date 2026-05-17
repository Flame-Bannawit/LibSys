using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;

namespace LibSys.API.Pages;

public class LoginModel : PageModel
{
    private readonly IHttpClientFactory _http;
    public string? ErrorMessage { get; set; }

    public LoginModel(IHttpClientFactory http) => _http = http;

    public IActionResult OnGet()
    {
        if (HttpContext.Session.GetString("JWTToken") != null)
            return RedirectToPage("/Index");
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string email, string password)
    {
        var client = _http.CreateClient("LibSysAPI");
        var response = await client.PostAsJsonAsync("/api/auth/login",
            new { Email = email, Password = password });

        if (!response.IsSuccessStatusCode)
        {
            ErrorMessage = "อีเมลหรือรหัสผ่านไม่ถูกต้อง";
            return Page();
        }

        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        if (result == null) { ErrorMessage = "เกิดข้อผิดพลาด"; return Page(); }

        HttpContext.Session.SetString("JWTToken", result.Token);
        HttpContext.Session.SetString("UserName", result.FullName);
        HttpContext.Session.SetString("UserRole", result.Role);

        return RedirectToPage("/Index");
    }

    public record AuthResponse(string Token, string Email, string FullName, string Role, DateTime Expires);
}