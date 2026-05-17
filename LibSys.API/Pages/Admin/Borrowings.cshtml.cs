using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using LibSys.API.DTOs;

namespace LibSys.API.Pages.Admin;

public class AdminBorrowingsModel : PageModel
{
    private readonly IHttpClientFactory _http;
    public List<BorrowingDto> Borrowings { get; set; } = new();
    public string? SuccessMessage { get; set; }

    public AdminBorrowingsModel(IHttpClientFactory http) => _http = http;

    private HttpClient GetClient()
    {
        var token = HttpContext.Session.GetString("JWTToken");
        var client = _http.CreateClient("LibSysAPI");
        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        if (HttpContext.Session.GetString("UserRole") != "Admin")
            return RedirectToPage("/Login");

        try
        {
            var res = await GetClient().GetAsync("/api/borrowings?pageSize=50");
            if (res.IsSuccessStatusCode)
            {
                var result = await res.Content.ReadFromJsonAsync<PagedResult<BorrowingDto>>();
                Borrowings = result?.Items?.ToList() ?? new();
            }
        }
        catch { Borrowings = new(); }

        return Page();
    }

    public async Task<IActionResult> OnPostReturnAsync(int borrowingId)
    {
        if (HttpContext.Session.GetString("UserRole") != "Admin")
            return RedirectToPage("/Login");

        try
        {
            var res = await GetClient().PutAsync($"/api/borrowings/{borrowingId}/return", null);
            if (res.IsSuccessStatusCode)
                SuccessMessage = "รับคืนหนังสือสำเร็จ";
        }
        catch { }

        await OnGetAsync();
        return Page();
    }
}