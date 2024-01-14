namespace NHackerNewsClient;

public sealed record Story(
    int Id,
    string? PostedBy,
    int CommentCount,
    int Score,
    int Time,
    string? Title,
    string? Url
);