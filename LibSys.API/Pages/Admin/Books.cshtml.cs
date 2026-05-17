using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using LibSys.API.DTOs;

namespace LibSys.API.Pages.Admin;

public class AdminBooksModel : PageModel
{
    private readonly IHttpClientFactory _http;
    public List<BookDto> Books { get; set; } = new();
    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public AdminBooksModel(IHttpClientFactory http) => _http = http;

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
            var res = await GetClient().GetAsync("/api/books?pageSize=100");
            if (res.IsSuccessStatusCode)
            {
                var result = await res.Content.ReadFromJsonAsync<PagedResult<BookDto>>();
                Books = result?.Items?.ToList() ?? new();
            }
        }
        catch { Books = new(); }

        return Page();
    }

    public async Task<IActionResult> OnPostAddAsync(string title, string author,
        string isbn, string category, int totalCopies, string? description)
    {
        if (HttpContext.Session.GetString("UserRole") != "Admin")
            return RedirectToPage("/Login");

        try
        {
            var res = await GetClient().PostAsJsonAsync("/api/books", new
            {
                Title = title,
                Author = author,
                ISBN = isbn,
                Category = category,
                TotalCopies = totalCopies,
                Description = description ?? ""
            });
            SuccessMessage = res.IsSuccessStatusCode ? "เพิ่มหนังสือสำเร็จ" : null;
            ErrorMessage = res.IsSuccessStatusCode ? null : "ไม่สามารถเพิ่มหนังสือได้";
        }
        catch { ErrorMessage = "เกิดข้อผิดพลาด"; }

        await OnGetAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int bookId)
    {
        if (HttpContext.Session.GetString("UserRole") != "Admin")
            return RedirectToPage("/Login");

        try
        {
            var res = await GetClient().DeleteAsync($"/api/books/{bookId}");
            SuccessMessage = res.IsSuccessStatusCode ? "ลบหนังสือสำเร็จ" : null;
            ErrorMessage = res.IsSuccessStatusCode ? null : "ไม่สามารถลบหนังสือได้";
        }
        catch { ErrorMessage = "เกิดข้อผิดพลาด"; }

        await OnGetAsync();
        return Page();
    }
}