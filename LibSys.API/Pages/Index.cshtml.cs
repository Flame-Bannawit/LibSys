using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using LibSys.API.DTOs;

namespace LibSys.API.Pages;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _http;
    public List<BookDto> Books { get; set; } = new();
    public DashboardStatsDto? Stats { get; set; }

    public IndexModel(IHttpClientFactory http) => _http = http;

    public async Task OnGetAsync()
    {
        var token = HttpContext.Session.GetString("JWTToken");
        var client = _http.CreateClient("LibSysAPI");

        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

        try
        {
            var res = await client.GetAsync("/api/books?pageSize=6");
            if (res.IsSuccessStatusCode)
            {
                var result = await res.Content.ReadFromJsonAsync<PagedResult<BookDto>>();
                Books = result?.Items?.ToList() ?? new();
            }
        }
        catch { Books = new(); }

        if (HttpContext.Session.GetString("UserRole") == "Admin" && !string.IsNullOrEmpty(token))
        {
            try
            {
                var res = await client.GetAsync("/api/admin/dashboard");
                if (res.IsSuccessStatusCode)
                    Stats = await res.Content.ReadFromJsonAsync<DashboardStatsDto>();
            }
            catch { Stats = null; }
        }
    }
}