namespace LibSys.API.DTOs;

// Auth DTOs
public record RegisterDto(string FullName, string Email, string Password, string StudentId = "");
public record LoginDto(string Email, string Password);
public record AuthResponseDto(string Token, string Email, string FullName, string Role, DateTime Expires);

// Book DTOs
public record BookDto(int Id, string Title, string Author, string ISBN, string Category, string Description, string CoverImageUrl, int TotalCopies, int AvailableCopies);
public record CreateBookDto(string Title, string Author, string ISBN, string Category, string Description = "", string CoverImageUrl = "", int TotalCopies = 1);
public record UpdateBookDto(string Title, string Author, string ISBN, string Category, string Description, string CoverImageUrl, int TotalCopies);

// Borrowing DTOs
public record BorrowRequestDto(int BookId, int DurationDays = 14);
public record BorrowingDto(int Id, string UserId, string UserName, int BookId, string BookTitle, DateTime BorrowedAt, DateTime DueDate, DateTime? ReturnedAt, string Status);

// Reservation DTOs
public record ReservationDto(int Id, string UserId, string UserName, int BookId, string BookTitle, DateTime ReservedAt, DateTime ExpiresAt, string Status);

// Dashboard DTOs
public record DashboardStatsDto(
    int TotalBooks,
    int TotalMembers,
    int ActiveBorrowings,
    int OverdueBorrowings,
    int TotalReservations,
    IEnumerable<PopularBookDto> PopularBooks,
    IEnumerable<RecentActivityDto> RecentActivities
);
public record PopularBookDto(int BookId, string Title, int BorrowCount);
public record RecentActivityDto(string Type, string Description, DateTime Date);

// Pagination
public record PagedResult<T>(IEnumerable<T> Items, int Total, int Page, int PageSize);
