using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using LibSys.API.DTOs;

namespace LibSys.API.Pages.Books;

public class BooksIndexModel : PageModel
{
    private readonly IHttpClientFactory _http;
    public List<BookDto> Books { get; set; } = new();
    public string? Search { get; set; }
    public string? Category { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;

    public BooksIndexModel(IHttpClientFactory http) => _http = http;

    public async Task OnGetAsync(string? search, string? category, int page = 1)
    {
        Search = search;
        Category = category;
        CurrentPage = page;

        var client = _http.CreateClient("LibSysAPI");
        var token = HttpContext.Session.GetString("JWTToken");
        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

        var url = $"/api/books?page={page}&pageSize=12";
        if (!string.IsNullOrEmpty(search)) url += $"&search={Uri.EscapeDataString(search)}";
        if (!string.IsNullOrEmpty(category)) url += $"&category={Uri.EscapeDataString(category)}";

        try
        {
            var res = await client.GetAsync(url);
            if (res.IsSuccessStatusCode)
            {
                var result = await res.Content.ReadFromJsonAsync<PagedResult<BookDto>>();
                Books = result?.Items?.ToList() ?? new();
                TotalPages = (int)Math.Ceiling((double)(result?.Total ?? 0) / 12);
            }
        }
        catch { Books = new(); }
    }
}