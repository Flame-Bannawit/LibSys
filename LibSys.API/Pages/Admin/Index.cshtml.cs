using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using LibSys.API.DTOs;

namespace LibSys.API.Pages.Admin;

public class AdminIndexModel : PageModel
{
    private readonly IHttpClientFactory _http;
    public DashboardStatsDto? Stats { get; set; }

    public AdminIndexModel(IHttpClientFactory http) => _http = http;

    public async Task<IActionResult> OnGetAsync()
    {
        var token = HttpContext.Session.GetString("JWTToken");
        var role = HttpContext.Session.GetString("UserRole");

        if (string.IsNullOrEmpty(token) || role != "Admin")
            return RedirectToPage("/Login");

        var client = _http.CreateClient("LibSysAPI");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        try
        {
            var res = await client.GetAsync("/api/admin/dashboard");
            if (res.IsSuccessStatusCode)
                Stats = await res.Content.ReadFromJsonAsync<DashboardStatsDto>();
        }
        catch { Stats = null; }

        return Page();
    }
}