using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using LibSys.API.DTOs;

namespace LibSys.API.Pages.Books;

public class BookDetailModel : PageModel
{
    private readonly IHttpClientFactory _http;
    public BookDto? Book { get; set; }
    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public BookDetailModel(IHttpClientFactory http) => _http = http;

    public async Task OnGetAsync(int id)
    {
        var client = _http.CreateClient("LibSysAPI");
        try
        {
            var res = await client.GetAsync($"/api/books/{id}");
            if (res.IsSuccessStatusCode)
                Book = await res.Content.ReadFromJsonAsync<BookDto>();
        }
        catch { Book = null; }
    }

    public async Task<IActionResult> OnPostAsync(int bookId)
    {
        var token = HttpContext.Session.GetString("JWTToken");
        if (string.IsNullOrEmpty(token))
            return RedirectToPage("/Login");

        var client = _http.CreateClient("LibSysAPI");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        try
        {
            var res = await client.PostAsJsonAsync("/api/borrowings",
                new { BookId = bookId, DurationDays = 14 });

            if (res.IsSuccessStatusCode)
                SuccessMessage = "✅ ยืมหนังสือสำเร็จ! กำหนดคืนภายใน 14 วัน";
            else
                ErrorMessage = "❌ ไม่สามารถยืมหนังสือได้ กรุณาลองใหม่";
        }
        catch { ErrorMessage = "เกิดข้อผิดพลาด กรุณาลองใหม่"; }

        await OnGetAsync(bookId);
        return Page();
    }
}