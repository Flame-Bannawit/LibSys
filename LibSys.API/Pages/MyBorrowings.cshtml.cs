using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using LibSys.API.DTOs;

namespace LibSys.API.Pages;

public class MyBorrowingsModel : PageModel
{
    private readonly IHttpClientFactory _http;
    public List<BorrowingDto> Borrowings { get; set; } = new();

    public MyBorrowingsModel(IHttpClientFactory http) => _http = http;

    public async Task<IActionResult> OnGetAsync()
    {
        var token = HttpContext.Session.GetString("JWTToken");
        if (string.IsNullOrEmpty(token))
            return RedirectToPage("/Login");

        var client = _http.CreateClient("LibSysAPI");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        try
        {
            var res = await client.GetAsync("/api/borrowings/my");
            if (res.IsSuccessStatusCode)
                Borrowings = await res.Content.ReadFromJsonAsync<List<BorrowingDto>>() ?? new();
        }
        catch { Borrowings = new(); }

        return Page();
    }
}