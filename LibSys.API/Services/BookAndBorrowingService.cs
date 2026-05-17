using Microsoft.EntityFrameworkCore;
using LibSys.API.Data;
using LibSys.API.DTOs;
using LibSys.API.Models;

namespace LibSys.API.Services;

public interface IBookService
{
    Task<PagedResult<BookDto>> GetBooksAsync(string? search, string? category, int page, int pageSize);
    Task<BookDto?> GetBookByIdAsync(int id);
    Task<BookDto> CreateBookAsync(CreateBookDto dto);
    Task<BookDto?> UpdateBookAsync(int id, UpdateBookDto dto);
    Task<bool> DeleteBookAsync(int id);
}

public class BookService : IBookService
{
    private readonly AppDbContext _db;
    public BookService(AppDbContext db) => _db = db;

    public async Task<PagedResult<BookDto>> GetBooksAsync(string? search, string? category, int page, int pageSize)
    {
        var query = _db.Books.Where(b => b.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(b => b.Title.Contains(search) || b.Author.Contains(search) || b.ISBN.Contains(search));

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(b => b.Category == category);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => ToDto(b))
            .ToListAsync();

        return new PagedResult<BookDto>(items, total, page, pageSize);
    }

    public async Task<BookDto?> GetBookByIdAsync(int id)
    {
        var book = await _db.Books.FindAsync(id);
        return book == null ? null : ToDto(book);
    }

    public async Task<BookDto> CreateBookAsync(CreateBookDto dto)
    {
        var book = new Book
        {
            Title = dto.Title,
            Author = dto.Author,
            ISBN = dto.ISBN,
            Category = dto.Category,
            Description = dto.Description,
            CoverImageUrl = dto.CoverImageUrl,
            TotalCopies = dto.TotalCopies,
            AvailableCopies = dto.TotalCopies
        };
        _db.Books.Add(book);
        await _db.SaveChangesAsync();
        return ToDto(book);
    }

    public async Task<BookDto?> UpdateBookAsync(int id, UpdateBookDto dto)
    {
        var book = await _db.Books.FindAsync(id);
        if (book == null) return null;

        book.Title = dto.Title;
        book.Author = dto.Author;
        book.ISBN = dto.ISBN;
        book.Category = dto.Category;
        book.Description = dto.Description;
        book.CoverImageUrl = dto.CoverImageUrl;
        book.TotalCopies = dto.TotalCopies;

        await _db.SaveChangesAsync();
        return ToDto(book);
    }

    public async Task<bool> DeleteBookAsync(int id)
    {
        var book = await _db.Books.FindAsync(id);
        if (book == null) return false;
        book.IsActive = false;
        await _db.SaveChangesAsync();
        return true;
    }

    private static BookDto ToDto(Book b) =>
        new(b.Id, b.Title, b.Author, b.ISBN, b.Category, b.Description, b.CoverImageUrl, b.TotalCopies, b.AvailableCopies);
}

public interface IBorrowingService
{
    Task<BorrowingDto?> BorrowBookAsync(string userId, BorrowRequestDto dto);
    Task<BorrowingDto?> ReturnBookAsync(int borrowingId, string userId);
    Task<IEnumerable<BorrowingDto>> GetUserBorrowingsAsync(string userId);
    Task<PagedResult<BorrowingDto>> GetAllBorrowingsAsync(int page, int pageSize, string? status);
    Task<DashboardStatsDto> GetDashboardStatsAsync();
}

public class BorrowingService : IBorrowingService
{
    private readonly AppDbContext _db;
    public BorrowingService(AppDbContext db) => _db = db;

    public async Task<BorrowingDto?> BorrowBookAsync(string userId, BorrowRequestDto dto)
    {
        var book = await _db.Books.FindAsync(dto.BookId);
        if (book == null || book.AvailableCopies <= 0) return null;

        var borrowing = new Borrowing
        {
            UserId = userId,
            BookId = dto.BookId,
            BorrowedAt = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(dto.DurationDays),
            Status = BorrowingStatus.Active
        };

        book.AvailableCopies--;
        _db.Borrowings.Add(borrowing);
        await _db.SaveChangesAsync();

        return await GetBorrowingDtoAsync(borrowing.Id);
    }

    public async Task<BorrowingDto?> ReturnBookAsync(int borrowingId, string userId)
    {
        var borrowing = await _db.Borrowings.Include(b => b.Book).FirstOrDefaultAsync(b => b.Id == borrowingId);
        if (borrowing == null) return null;

        borrowing.ReturnedAt = DateTime.UtcNow;
        borrowing.Status = BorrowingStatus.Returned;
        borrowing.Book.AvailableCopies++;

        await _db.SaveChangesAsync();
        return await GetBorrowingDtoAsync(borrowingId);
    }

    public async Task<IEnumerable<BorrowingDto>> GetUserBorrowingsAsync(string userId)
    {
        return await _db.Borrowings
            .Include(b => b.Book)
            .Include(b => b.User)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.BorrowedAt)
            .Select(b => MapToDto(b))
            .ToListAsync();
    }

    public async Task<PagedResult<BorrowingDto>> GetAllBorrowingsAsync(int page, int pageSize, string? status)
    {
        var query = _db.Borrowings.Include(b => b.Book).Include(b => b.User).AsQueryable();

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<BorrowingStatus>(status, out var s))
            query = query.Where(b => b.Status == s);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(b => b.BorrowedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => MapToDto(b))
            .ToListAsync();

        return new PagedResult<BorrowingDto>(items, total, page, pageSize);
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        var totalBooks = await _db.Books.CountAsync(b => b.IsActive);
        var totalMembers = await _db.Users.CountAsync();
        var activeBorrowings = await _db.Borrowings.CountAsync(b => b.Status == BorrowingStatus.Active);
        var overdue = await _db.Borrowings.CountAsync(b => b.Status == BorrowingStatus.Active && b.DueDate < DateTime.UtcNow);
        var reservations = await _db.Reservations.CountAsync(r => r.Status == ReservationStatus.Pending);

        var popularBooks = await _db.Borrowings
            .GroupBy(b => new { b.BookId, b.Book.Title })
            .Select(g => new PopularBookDto(g.Key.BookId, g.Key.Title, g.Count()))
            .OrderByDescending(x => x.BorrowCount)
            .Take(5)
            .ToListAsync();

        var recentActivities = await _db.Borrowings
            .Include(b => b.User)
            .Include(b => b.Book)
            .OrderByDescending(b => b.BorrowedAt)
            .Take(10)
            .Select(b => new RecentActivityDto("Borrow", $"{b.User.FullName} ยืม {b.Book.Title}", b.BorrowedAt))
            .ToListAsync();

        return new DashboardStatsDto(totalBooks, totalMembers, activeBorrowings, overdue, reservations, popularBooks, recentActivities);
    }

    private async Task<BorrowingDto?> GetBorrowingDtoAsync(int id)
    {
        var b = await _db.Borrowings.Include(x => x.Book).Include(x => x.User).FirstOrDefaultAsync(x => x.Id == id);
        return b == null ? null : MapToDto(b);
    }

    private static BorrowingDto MapToDto(Borrowing b) =>
        new(b.Id, b.UserId, b.User?.FullName ?? "", b.BookId, b.Book?.Title ?? "", b.BorrowedAt, b.DueDate, b.ReturnedAt, b.Status.ToString());
}
